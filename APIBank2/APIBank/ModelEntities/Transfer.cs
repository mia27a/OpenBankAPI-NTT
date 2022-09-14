using System;

namespace APIBank.ModelEntities
{
    public partial class Transfer
    {
        public int Id { get; set; }
        public decimal Amount { get; set; }
        [MinLength(3), MaxLength(3)]
        public string Currency { get; set; } = "EUR";
        public int FromAccountId { get; set; }
        public int ToAccountId { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
