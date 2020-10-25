using System;
using System.Threading.Tasks;
using KNFA.Bots.MTB.Events.Mumble;
using KNFA.Bots.MTB.Events.Telegram;
using SlimMessageBus;

namespace KNFA.Bots.MTB.Consumers
{
    public class UserJoinedConsumer : IConsumer<UserJoined>
    {
        private readonly IMessageBus _messageBus;

        public UserJoinedConsumer(IMessageBus messageBus)
        {
            _messageBus = messageBus ?? throw new ArgumentNullException(nameof(messageBus));
        }

        public async Task OnHandle(UserJoined message, string name)
        {
            await _messageBus.Publish(new TextMessage($"{message.Username} joined"));
        }
    }
}
