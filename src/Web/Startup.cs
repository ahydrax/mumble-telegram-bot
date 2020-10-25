using System;
using System.Linq;
using KNFA.Bots.MTB.Configuration;
using KNFA.Bots.MTB.Consumers;
using KNFA.Bots.MTB.Events.Mumble;
using KNFA.Bots.MTB.Events.Telegram;
using KNFA.Bots.MTB.Services.Mumble;
using KNFA.Bots.MTB.Services.Telegram;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using MumbleSharp;
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

            services.AddSingleton(appConfig.Telegram);
            services.AddSingleton<TelegramService>();
            services.AddHostedService(x => x.GetRequiredService<TelegramService>());
            services.AddSingleton<ITelegramMessageSender>(x => x.GetRequiredService<TelegramService>());

            services.AddSingleton(BuildMessageBus);
            services.AddSingleton(appConfig.Mumble);
            services.AddSingleton<EventProtocol>();
            services.AddSingleton<MumbleClientService>();
            services.AddHostedService(x => x.GetRequiredService<MumbleClientService>());

            services.AddTransient<NewTextMessageConsumer>();
            services.AddTransient<UserListRequestedConsumer>();
            services.AddTransient<UserJoinedConsumer>();
            services.AddTransient<UserLeftConsumer>();

            services.AddHttpContextAccessor();
            services.AddControllersWithViews();
        }

        private static IMessageBus BuildMessageBus(IServiceProvider serviceProvider)
        {
            var domainAssembly = typeof(Startup).Assembly;

            var mbb = MessageBusBuilder.Create()
                .Produce<UserLeft>(x => x.DefaultTopic(x.Settings.MessageType.Name))
                .Produce<UserJoined>(x => x.DefaultTopic(x.Settings.MessageType.Name))
                .Produce<UserListRequested>(x => x.DefaultTopic(x.Settings.MessageType.Name))
                .Produce<TextMessage>(x => x.DefaultTopic(x.Settings.MessageType.Name))
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
            app.UseRouting();
            app.UseEndpoints(endpoints => { endpoints.MapControllers(); });
        }
    }
}
