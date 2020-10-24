using System.Threading;
using System.Threading.Tasks;

namespace KNFA.Bots.MTB.Services.Telegram
{
    public interface ITelegramMessageSender
    {
        Task SendTextMessage(string text, CancellationToken ct);
    }
}
