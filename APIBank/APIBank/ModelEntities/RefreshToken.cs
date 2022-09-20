using System.Text.Json.Serialization;

namespace APIBank.ModelEntities
{
    
    public partial class RefreshToken
    {
        [Key]
        [JsonIgnore]
        public int Id { get; set; }
        public int UserId { get; set; }
        public string RefToken { get; set; }
        public DateTime Expires { get; set; }
        public DateTime Created { get; set; }
        public string CreatedByIp { get; set; }
        public DateTime? Revoked { get; set; }
        public string? RevokedByIp { get; set; }
        public string? ReplacedByToken { get; set; }
        public string? ReasonRevoked { get; set; }
        public bool IsExpired
        {
            get => DateTime.UtcNow >= Expires;
            set { }
        }
        public bool IsRevoked
        {
            get => Revoked != null;
            set { }
        }
        public bool IsActive
        {
            get => !IsRevoked && !IsExpired;
            set { }
        }

        public virtual User User { get; set; }

    }
}
