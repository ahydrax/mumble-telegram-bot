using System;

namespace KNFA.Bots.MTB.Events.Telegram
{
    public class UserListRequested
    {
        public string Requester { get; }

        public UserListRequested(string requester)
        {
            Requester = requester ?? throw new ArgumentNullException(nameof(requester));
        }
    }
}
