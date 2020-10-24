using System;

namespace KNFA.Bots.MTB.Events.Telegram
{
    public class TextMessage
    {
        public string Text { get; }

        public TextMessage(string text)
        {
            Text = text ?? throw new ArgumentNullException(nameof(text));
        }
    }
}
