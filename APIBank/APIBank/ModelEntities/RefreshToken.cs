namespace APIBank.ModelEntities
{
    public partial class RefreshToken
    {
        [Key]
        public int Id { get; set; }
        public int UserId { get; set; }
        public string Token { get; set; }
        public DateTime Expires { get; set; }
        public DateTime Created { get; set; }
        public string CreatedByIp { get; set; }
        public DateTime? Revoked { get; set; }
        public string? RevokedByIp { get; set; }
        public string? ReplacedByToken { get; set; }
        public string? ReasonRevoked { get; set; }
        //[NotMapped]
        //public bool IsExpired
        //{
        //    get
        //    {
        //        return DateTime.UtcNow >= Expires;
        //    }
        //}
        //////[NotMapped]
        //public bool IsRevoked
        //{
        //    get => Revoked != null;
        //}
        //////[NotMapped]
        //public bool IsActive
        //{
        //    get => !IsRevoked && !IsExpired;
        //}

        public virtual User User { get; set; }
    }
}
