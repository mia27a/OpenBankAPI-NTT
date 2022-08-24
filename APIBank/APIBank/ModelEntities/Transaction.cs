using System;

namespace APIBank.ModelEntities
{
    public partial class Transaction
    {
        public int Id { get; set; }
        [ForeignKey("Account")]
        public int AccountId { get; set; }
        public decimal Amount { get; set; }
        public decimal Balance { get; set; }
        public DateTime CreatedAt { get; set; }
        /*public string TransactionType { get; set; } = null!;*/

        public virtual Account Account { get; set; } = null!;
    }
}
