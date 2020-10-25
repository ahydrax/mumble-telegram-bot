using System;
using System.Threading;
using System.Threading.Tasks;
using KNFA.Bots.MTB.Configuration;
using Microsoft.Extensions.Logging;
using MumbleSharp;

namespace KNFA.Bots.MTB.Services.Mumble
{
    public class MumbleClientService : BackgroundService
    {
        private readonly EventProtocol _mumbleProtocol;
        private readonly MumbleConfiguration _configuration;
        private readonly ILogger<MumbleClientService> _logger;
        private readonly MumbleConnection _mumbleConnection;

        public MumbleClientService(
            EventProtocol mumbleProtocol,
            MumbleConfiguration configuration,
            ILogger<MumbleClientService> logger)
        {
            _mumbleProtocol = mumbleProtocol ?? throw new ArgumentNullException(nameof(mumbleProtocol));
            _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _mumbleConnection = new MumbleConnection(configuration.Host, configuration.Port, mumbleProtocol);
        }

        protected override ILogger Logger => _logger;

        protected override Task OnStart(CancellationToken ct)
        {
            _logger.LogInformation(AppDomain.CurrentDomain.BaseDirectory);

            _mumbleConnection.Connect(
                _configuration.Username,
                _configuration.Password,
                new string[0],
                _configuration.Host);

            return Task.CompletedTask;
        }

        protected override async Task Execute(CancellationToken ct)
        {
            // sorry
            Task.Run(async () =>
            {
                await Task.Delay(TimeSpan.FromSeconds(10));
                _mumbleProtocol.SetInitialized();
            });

            while (_mumbleConnection.State != ConnectionStates.Disconnected)
            {
                if (_mumbleConnection.Process())
                {
                    await Task.Yield();
                }
                else
                {
                    await Task.Delay(100, ct);
                }

                ct.ThrowIfCancellationRequested();
            }
        }

        protected override Task OnStop(CancellationToken ct)
        {
            _mumbleConnection.Close();
            return Task.CompletedTask;
        }

        protected override void OnError(Exception e) => Environment.FailFast("Mumble failed", e);
    }
}
