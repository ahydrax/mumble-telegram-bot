using System;
using System.Linq;
using Grpc.Core;
using KNFA.Bots.MTB.Configurations;
using KNFA.Bots.MTB.Consumers;
using KNFA.Bots.MTB.Events.Mumble;
using KNFA.Bots.MTB.Events.Telegram;
using KNFA.Bots.MTB.Services.Mumble;
using KNFA.Bots.MTB.Services.Telegram;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using MurmurRPC;
using SlimMessageBus;
using SlimMessageBus.Host.AspNetCore;
using SlimMessageBus.Host.Config;
using SlimMessageBus.Host.Memory;

namespace KNFA.Bots.MTB
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        public void ConfigureServices(IServiceCollection services)
        {
            var appConfig = Configuration.Get<ApplicationConfiguration>();
            ThrowIfAppConfigIsInvalid(appConfig);

            AppContext.SetSwitch("System.Net.Http.SocketsHttpHandler.Http2UnencryptedSupport", true);

            services.AddGrpcClient<V1.V1Client>(options =>
            {
                options.Address = new Uri(appConfig.Mumble.GrpcAddress);
                options.ChannelOptionsActions.Add(channelOptions => channelOptions.Credentials = ChannelCredentials.Insecure);
            });

            services.AddSingleton(appConfig.Telegram);
            services.AddSingleton<TelegramService>();
            services.AddHostedService(x => x.GetRequiredService<TelegramService>());
            services.AddSingleton<ITelegramMessageSender>(x => x.GetRequiredService<TelegramService>());

            services.AddSingleton(BuildMessageBus);
            services.AddSingleton(appConfig.Mumble);
            services.AddSingleton<MumbleClientService>();
            services.AddHostedService(x => x.GetRequiredService<MumbleClientService>());
            services.AddSingleton<IMumbleInfo>(x => x.GetRequiredService<MumbleClientService>());

            services.AddTransient<SendTextMessageConsumer>();
            services.AddTransient<UserListRequestedConsumer>();
            services.AddTransient<UserJoinedConsumer>();
            services.AddTransient<UserLeftConsumer>();

            services.AddHttpContextAccessor();
            services.AddControllersWithViews();
        }

        private static void ThrowIfAppConfigIsInvalid(ApplicationConfiguration appConfig)
        {
            if (appConfig.Mumble == null)
                throw new ApplicationException($"{nameof(appConfig.Mumble)} is null");
            if (appConfig.Mumble.GrpcAddress == null)
                throw new ApplicationException($"{nameof(appConfig.Mumble.GrpcAddress)} is null");

            if (appConfig.Telegram == null)
                throw new ApplicationException($"{nameof(appConfig.Telegram)} is null");
            if (appConfig.Telegram.BotToken == null)
                throw new ApplicationException($"{nameof(appConfig.Telegram.BotToken)} is null");
            if (appConfig.Telegram.BotUsername == null)
                throw new ApplicationException($"{nameof(appConfig.Telegram.BotUsername)} is null");
        }

        private static IMessageBus BuildMessageBus(IServiceProvider serviceProvider)
        {
            var domainAssembly = typeof(Startup).Assembly;

            var mbb = MessageBusBuilder.Create()
                .Produce<UserLeft>(x => x.DefaultTopic(x.Settings.MessageType.Name))
                .Produce<UserJoined>(x => x.DefaultTopic(x.Settings.MessageType.Name))
                .Produce<UserListRequested>(x => x.DefaultTopic(x.Settings.MessageType.Name))
                .Produce<SendTextMessage>(x => x.DefaultTopic(x.Settings.MessageType.Name))
                .Do(builder =>
                {
                    var consumers = domainAssembly
                        .GetTypes()
                        .Where(t => t.IsClass && !t.IsAbstract)
                        .SelectMany(t => t.GetInterfaces(), (t, i) => new { Type = t, Interface = i })
                        .Where(x => x.Interface.IsGenericType && x.Interface.GetGenericTypeDefinition() == typeof(IConsumer<>))
                        .Select(x => new { HandlerType = x.Type, EventType = x.Interface.GetGenericArguments()[0] })
                        .ToList();

                    foreach (var consumerType in consumers)
                    {
                        builder.Consume(consumerType.EventType,
                            x => x.Topic(x.MessageType.Name).WithConsumer(consumerType.HandlerType));
                    }
                })
                .WithDependencyResolver(new AspNetCoreMessageBusDependencyResolver(serviceProvider))
                .WithProviderMemory(new MemoryMessageBusSettings
                {
                    EnableMessageSerialization = false
                });

            return mbb.Build();
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            app.UseForwardedHeaders();
            app.UseRouting();
            app.UseStaticFiles();
            app.UseEndpoints(endpoints => { endpoints.MapControllers(); });
        }
    }
}
