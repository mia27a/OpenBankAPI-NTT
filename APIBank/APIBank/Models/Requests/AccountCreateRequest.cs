namespace APIBank.Models.Requests
{
    public class AccountCreateRequest
    {
        public decimal Amount { get; set; }

        [MinLength(3), MaxLength(3)]
        public string Currency { get; set; } = "EUR";
    }
}
