using System;

namespace KNFA.Bots.MTB.Events.Mumble
{
    public class UserJoined
    {
        public string Username { get; }

        public UserJoined(string username)
        {
            Username = username ?? throw new ArgumentNullException(nameof(username));
        }
    }
}
