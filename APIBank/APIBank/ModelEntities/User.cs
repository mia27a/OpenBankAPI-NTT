using System;
using System.Text.Json.Serialization;

namespace APIBank.ModelEntities
{
    public partial class User
    {
        public User()
        {
            Accounts = new HashSet<Account>();
        }

        public int Id { get; set; }
        public string Email { get; set; } = null!;
        public string FullName { get; set; } = null!;
        public string Username { get; set; } = null!;
        //public string Password { get; set; } = null!;
        public DateTime CreatedAt { get; set; }
        public DateTime PasswordChangedAt { get; set; }

        [JsonIgnore]
        public string Password_hash { get; set; } //Jason JWT

        public virtual ICollection<Account> Accounts { get; set; }
    }
}
