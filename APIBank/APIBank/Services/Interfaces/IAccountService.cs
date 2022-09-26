using APIBank.Models.Responses;

namespace APIBank.Services.Interfaces
{
    public interface IAccountService
    {
        AccountRequestResponse Create(AccountCreateRequest model, int userId);
        List<AccountRequestResponse> GetAll(int userId);
        AccountMovims GetById(int accountId, int userId);
    }
}
