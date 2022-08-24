namespace APIBank.Models.Accounts
{
    public class AccountRe
    {
        public decimal Balance { get; set; }
        
        [MinLength(3), MaxLength(3)]
        public string Currency { get; set; } = "EUR";
        
        public DateTime CreatedAt { get; set; }

        [Required]
        public int Id { get; set; }
    }
}
