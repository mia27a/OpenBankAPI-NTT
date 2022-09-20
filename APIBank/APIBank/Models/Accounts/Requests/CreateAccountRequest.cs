using Newtonsoft.Json;

namespace APIBank.Models.Accounts.Requests
{
    public class CreateAccountRequest
    {
        //[ForeignKey("User")]
        //[JsonIgnore]
        //public int UserId { get; set; }
        public decimal Amount { get; set; }

        [MinLength(3), MaxLength(3)]
        public string Currency { get; set; } = "EUR";
    }
}
