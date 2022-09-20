using APIBank.Models.Accounts;

namespace APIBank.Services.Interfaces
{
    public interface IAccountService
    {
        void Create(CreateAccountRequest model, int userId);
        List<AccountRe> GetAll(int userId);
        AccountMovims GetById(int accountId, int userId);
    }
}
