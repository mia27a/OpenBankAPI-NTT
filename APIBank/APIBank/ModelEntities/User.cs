using System;
using System.Text.Json.Serialization;

namespace APIBank.ModelEntities
{
    public class User
    {
        public User()
        {
            Accounts = new HashSet<Account>();
            RefreshTokenCollection = new List<RefreshToken>();
        }

        public int Id { get; set; }

        public string Email { get; set; } = null!;

        public string FullName { get; set; } = null!;

        public string Username { get; set; } = null!;

        [JsonIgnore]
        public string PasswordHash { get; set; }

        public DateTime CreatedAt { get; set; }

        public DateTime PasswordChangedAt { get; set; }

        public virtual ICollection<Account> Accounts { get; set; }

        [JsonIgnore]
        public virtual List<RefreshToken> RefreshTokenCollection { get; set; }
    }
}
