using APIBank.Models.Accounts;
using APIBank.Models.Transfers.Requests;

namespace APIBank.Services.Interfaces
{
    public interface ITransferService
    {
        void Create(TransferRequest model);
        List<Transfer> GetAll();
    }
}
