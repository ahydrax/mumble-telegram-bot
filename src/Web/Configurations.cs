namespace KNFA.Bots.MTB
{
    namespace Configurations
    {
        public record ApplicationConfiguration
        {
            public MumbleConfiguration Mumble { get; init; }
            public TelegramConfiguration Telegram { get; init; }
        }

        public record MumbleConfiguration
        {
            public string GrpcAddress { get; init; }
        }

        public record TelegramConfiguration
        {
            public string BotToken { get; init; }
            public string BotUsername { get; init; }
            public long HostGroupId { get; init; }
        }
    }
}
