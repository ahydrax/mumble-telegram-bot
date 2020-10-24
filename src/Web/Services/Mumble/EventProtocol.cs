using KNFA.Bots.MTB.Events.Telegram;
using Microsoft.Extensions.Logging;
using MumbleSharp;
using MumbleSharp.Model;
using SlimMessageBus;

namespace KNFA.Bots.MTB.Services.Mumble
{
    public class EventProtocol : BasicMumbleProtocol
    {
        private readonly IMessageBus _messageBus;
        private readonly ILogger<EventProtocol> _logger;
        private bool _initialized;

        public EventProtocol(IMessageBus messageBus, ILogger<EventProtocol> logger)
        {
            _messageBus = messageBus;
            _logger = logger;
        }

        public void SetInitialized() => _initialized = true;

        protected override void UserJoined(User user)
        {
            base.UserJoined(user);

            if (!_initialized) return;
            _logger.LogInformation("User joined {Username}", user.Name);
            _messageBus.Publish(new TextMessage($"{user.Name} joined"));
        }

        protected override void UserLeft(User user)
        {
            base.UserLeft(user);

            if (!_initialized) return;
            _logger.LogInformation("User left {Username}", user.Name);
            _messageBus.Publish(new TextMessage($"{user.Name} left"));
        }
    }
}
