using APIBank.Models.Accounts;
using APIBank.Models.Transfers.Requests;

namespace APIBank.Services
{
    public interface ITransferService
    {
        List<Transfer> GetAll();

        //AccountMovims GetById(int accountId, int userId);

        void Create(TransferRequest model);

        //void Update(int id, UpdateRequest model);

        //void Delete(int id);
    }
}
