using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Google.Protobuf.Collections;
using KNFA.Bots.MTB.Events.Mumble;
using Microsoft.Extensions.Logging;
using MurmurRPC;
using SlimMessageBus;

namespace KNFA.Bots.MTB.Services.Mumble
{
    public class MumbleClientService : BackgroundService, IMumbleInfo
    {
        private readonly IMessageBus _messageBus;
        private readonly ILogger<MumbleClientService> _logger;
        private readonly V1.V1Client _grpcClient;

        public MumbleClientService(
            IMessageBus messageBus,
            V1.V1Client grpcClient,
            ILogger<MumbleClientService> logger)
        {
            _messageBus = messageBus ?? throw new ArgumentNullException(nameof(messageBus));
            _grpcClient = grpcClient ?? throw new ArgumentNullException(nameof(grpcClient));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        protected override ILogger Logger => _logger;

        protected override Task OnStart(CancellationToken ct) => Task.CompletedTask;

        protected override async Task Execute(CancellationToken ct)
        {
            var previousUsersState = await GetUsersAsync();
            while (!ct.IsCancellationRequested)
            {
                await Task.Delay(TimeSpan.FromSeconds(1), ct);
                var currentState = await GetUsersAsync();

                var deltaJoined = currentState.Except(previousUsersState).ToArray();
                var deltaLeft = previousUsersState.Except(currentState).ToArray();

                foreach (var user in deltaJoined)
                {
                    await _messageBus.Publish(new UserJoined(user.Username));
                }

                foreach (var user in deltaLeft)
                {
                    await _messageBus.Publish(new UserLeft(user.Username));
                }

                previousUsersState = currentState;
            }
        }

        protected override Task OnStop(CancellationToken ct) => Task.CompletedTask;

        protected override void OnError(Exception e) => Environment.FailFast("Mumble failed", e);

        public async Task<User[]> GetUsersAsync()
        {
            var serverResponse = await _grpcClient.ServerQueryAsync(new Server.Types.Query());
            var server = serverResponse.Servers.First();
            var userResponse = await _grpcClient.UserQueryAsync(new MurmurRPC.User.Types.Query { Server = new Server { Id = server.Id } });
            if (userResponse == null) return Array.Empty<User>();
            return userResponse.Users.ToDto();
        }
    }

    public static class GrpcUsersExtensions
    {
        public static User[] ToDto(this RepeatedField<MurmurRPC.User> users)
            => users.Select(x => new User(x.Id, x.Name)).ToArray();
    }

    public interface IMumbleInfo
    {
        Task<User[]> GetUsersAsync();
    }
}
