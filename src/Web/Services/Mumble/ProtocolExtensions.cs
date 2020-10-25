using System.Collections.Generic;
using System.Linq;
using MumbleSharp;
using MumbleSharp.Model;

namespace KNFA.Bots.MTB.Services.Mumble
{
    public static class ProtocolExtensions
    {
        public static IEnumerable<User> GetUsers(this IMumbleProtocol protocol)
            => protocol.Users.Where(x => x.Id != protocol.LocalUser.Id);
    }
}
