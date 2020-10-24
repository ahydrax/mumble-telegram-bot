namespace KNFA.Bots.MTB.Configuration
{
    public class ApplicationConfiguration
    {
        public MumbleConfiguration Mumble { get; set; } = new MumbleConfiguration();
        public TelegramConfiguration Telegram { get; set; } = new TelegramConfiguration();
    }
}
