namespace APIBank.Models.Requests
{
    public class TransferRequest
    {
        [Required]
        public decimal Amount { get; set; } //double?
        [Required]
        public int FromAccountId { get; set; }
        [Required]
        public int ToAccountId { get; set; }
    }
}
