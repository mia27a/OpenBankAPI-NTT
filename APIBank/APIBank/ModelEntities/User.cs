using System;

namespace APIBank.ModelEntities
{
    public class User
    {
        public User()
        {
            Accounts = new HashSet<Account>();
            RefreshTokens = new HashSet<RefreshToken>();
        }

        public int Id { get; set; }

        public string Email { get; set; } = null!;

        public string FullName { get; set; } = null!;

        public string Username { get; set; } = null!;

        public string PasswordHash { get; set; }

        public DateTime CreatedAt { get; set; }

        public DateTime PasswordChangedAt { get; set; }

        public virtual ICollection<Account> Accounts { get; set; }

        public virtual ICollection<RefreshToken> RefreshTokens { get; set; }
    }
}
