namespace APIBank.ModelEntities
{
    public partial class Account
    {
        public Account()
        {
            Transactions = new HashSet<Transaction>();
        }

        public int Id { get; set; }
        public int UserId { get; set; }
        public decimal Balance { get; set; }
        [MinLength(3), MaxLength(3)]
        public string Currency { get; set; } = "EUR";
        public DateTime CreatedAt { get; set; }

        public virtual User User { get; set; } = null!;
        public virtual ICollection<Transaction> Transactions { get; set; }
    }
}
