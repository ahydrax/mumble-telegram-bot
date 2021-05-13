namespace KNFA.Bots.MTB
{
    namespace Configurations
    {
        public record ApplicationConfiguration
        {
            public MumbleConfiguration Mumble { get; init; } = null!;
            public TelegramConfiguration Telegram { get; init; } = null!;
        }

        public record MumbleConfiguration
        {
            public string GrpcAddress { get; init; } = null!;
        }

        public record TelegramConfiguration
        {
            public string BotToken { get; init; } = null!;
            public string BotUsername { get; init; } = null!;
            public long HostGroupId { get; init; }
        }
    }
}
