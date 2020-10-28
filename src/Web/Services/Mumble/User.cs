using System;

namespace KNFA.Bots.MTB.Services.Mumble
{
    public class User : IEquatable<User>
    {
        public uint Id { get; }
        public string Username { get; }

        public User(uint id, string username)
        {
            Id = id;
            Username = username;
        }

        public bool Equals(User? other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return Id == other.Id && Username == other.Username;
        }

        public override bool Equals(object? obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((User) obj);
        }

        public override int GetHashCode() => HashCode.Combine(Id, Username);
    }
}
