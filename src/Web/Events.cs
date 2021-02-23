namespace KNFA.Bots.MTB
{
    namespace Events.Mumble
    {
        public record UserJoined(string Username);

        public record UserLeft(string Username);
    }

    namespace Events.Telegram
    {
        public record SendTextMessage(string Text);

        public record UserListRequested(string Requester);
    }
}
