using System;
using System.Linq;
using System.Threading.Tasks;
using KNFA.Bots.MTB.Events.Telegram;
using Microsoft.Extensions.Logging;
using MumbleSharp;
using SlimMessageBus;

namespace KNFA.Bots.MTB.Services.Mumble
{
    public class UserListRequestedConsumer : IConsumer<UserListRequested>
    {
        private readonly IMessageBus _messageBus;
        private readonly EventProtocol _protocol;
        private readonly ILogger<UserListRequestedConsumer> _logger;

        public UserListRequestedConsumer(
            IMessageBus messageBus,
            EventProtocol protocol,
            ILogger<UserListRequestedConsumer> logger)
        {
            _messageBus = messageBus ?? throw new ArgumentNullException(nameof(messageBus));
            _protocol = protocol ?? throw new ArgumentNullException(nameof(protocol));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task OnHandle(UserListRequested message, string name)
        {
            var userListText =
                string.Join("\r\n",
                    _protocol.Users
                        .Where(x => x.Id != _protocol.LocalUser.Id)
                        .Select(x => x.Name));

            await _messageBus.Publish(new TextMessage(userListText));

            _logger.LogInformation("User list requested by {Requester}", message.Requester);
        }
    }
}
