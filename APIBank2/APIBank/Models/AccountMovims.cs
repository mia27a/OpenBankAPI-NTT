using APIBank.Models.Accounts;
using APIBank.Models.Transactions;

namespace APIBank.Models
{
    public class AccountMovims
    {
        public AccountRe Account { get; set; }
        public Movim[] Movims { get; set; }
    }
}
