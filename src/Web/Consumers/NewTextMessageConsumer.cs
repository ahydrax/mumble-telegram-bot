using System;
using System.Threading;
using System.Threading.Tasks;
using KNFA.Bots.MTB.Events.Telegram;
using KNFA.Bots.MTB.Services.Telegram;
using SlimMessageBus;

namespace KNFA.Bots.MTB.Consumers
{
    public class NewTextMessageConsumer : IConsumer<TextMessage>
    {
        private readonly ITelegramMessageSender _messageSender;

        public NewTextMessageConsumer(ITelegramMessageSender messageSender)
        {
            _messageSender = messageSender ?? throw new ArgumentNullException(nameof(messageSender));
        }

        public async Task OnHandle(TextMessage message, string name)
        {
            await _messageSender.SendTextMessage(message.Text, CancellationToken.None);
        }
    }
}
