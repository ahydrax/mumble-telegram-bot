using System;
using System.Linq;
using System.Threading.Tasks;
using KNFA.Bots.MTB.Events.Telegram;
using KNFA.Bots.MTB.Services.Mumble;
using Microsoft.Extensions.Logging;
using SlimMessageBus;

namespace KNFA.Bots.MTB.Consumers
{
    public class UserListRequestedConsumer : IConsumer<UserListRequested>
    {
        private readonly IMessageBus _messageBus;
        private readonly IMumbleInfo _mumbleInfo;
        private readonly ILogger<UserListRequestedConsumer> _logger;

        public UserListRequestedConsumer(
            IMessageBus messageBus,
            IMumbleInfo mumbleInfo,
            ILogger<UserListRequestedConsumer> logger)
        {
            _messageBus = messageBus ?? throw new ArgumentNullException(nameof(messageBus));
            _mumbleInfo = mumbleInfo ?? throw new ArgumentNullException(nameof(mumbleInfo));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task OnHandle(UserListRequested message, string name)
        {
            var users = await _mumbleInfo.GetUsersAsync();
            if (users.Length == 0)
            {
                await _messageBus.Publish(new SendTextMessage("No users connected"));
            }
            else
            {
                var userListText = string.Join("\r\n", users.Select(x => x.Username));
                await _messageBus.Publish(new SendTextMessage(userListText));
            }

            _logger.LogInformation("User list requested by {Requester}", message.Requester);
        }
    }
}
