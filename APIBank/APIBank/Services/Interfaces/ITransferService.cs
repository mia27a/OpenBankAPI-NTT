namespace APIBank.Services.Interfaces
{
    public interface ITransferService
    {
        void Create(TransferRequest model);
        List<Transfer> GetAll();
    }
}
