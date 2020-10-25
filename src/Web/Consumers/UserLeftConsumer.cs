using System;
using System.Threading.Tasks;
using KNFA.Bots.MTB.Events.Mumble;
using KNFA.Bots.MTB.Events.Telegram;
using SlimMessageBus;

namespace KNFA.Bots.MTB.Consumers
{
    public class UserLeftConsumer : IConsumer<UserLeft>
    {
        private readonly IMessageBus _messageBus;

        public UserLeftConsumer(IMessageBus messageBus)
        {
            _messageBus = messageBus ?? throw new ArgumentNullException(nameof(messageBus));
        }

        public async Task OnHandle(UserLeft message, string name)
        {
            await _messageBus.Publish(new TextMessage($"{message.Username} left"));
        }
    }
}
