using APIBank.Models.Accounts;

namespace APIBank.Services
{
    public interface IAccountService
    {
        List<AccountRe> GetAll(int userId);
        AccountMovims GetById(int accountId);
        void Create(CreateAccountRequest model);

        /*void Update(int id, UpdateRequest model);
        void Delete(int id);*/
    }
}
