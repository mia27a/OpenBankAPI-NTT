using APIBank.ModelEntities;
using APIBank.Services.Interfaces;

namespace APIBank.Services
{
    public class TransferService : ITransferService
    {
        private readonly PostgresContext _context;
        private readonly ILogger<TransferService> _logger;

        public TransferService(PostgresContext context, ILogger<TransferService> logger)
        {
            _context = context;
            _logger = logger;
        }

        public void Create(TransferRequest model)
        {
            using (var transaction = _context.Database.BeginTransaction())
            {
                //Create Transfer and Save
                var transfer = new Transfer()
                {
                    Amount = model.Amount,
                    FromAccountId = model.FromAccountId,
                    ToAccountId = model.ToAccountId
                };

                _context.Transfers.Add(transfer);
                _context.SaveChanges();


                // Create Movement in each account and Save
                Transaction movementFrom = CreateTransaction(transfer.FromAccountId, -transfer.Amount, DateTime.Now);
                Transaction movementTo = CreateTransaction(transfer.ToAccountId, transfer.Amount, DateTime.Now);

                _context.SaveChanges();

                //Update each Account Balance
                var fromAccount = UpdateAccountBalance(movementFrom, transfer.Amount);
                var toAccount = UpdateAccountBalance(movementTo, transfer.Amount);

                if (fromAccount == toAccount)
                {
                    _logger.LogInformation("Error while transferring funds:");
                    throw new AppException("You cannot transfer to the same account you are transferring from.");
                }
                if (fromAccount.Currency != toAccount.Currency)
                {
                    _logger.LogInformation("Error while transferring funds:");
                    throw new AppException("You can only transfer to accounts using the same currency.");
                }
                if (fromAccount.Balance < transfer.Amount)
                {
                    _logger.LogInformation("Error updating account balance:");
                    throw new AppException("Insufficient amount in account to transfer from.");
                }

                _context.SaveChanges();

                // Commit transaction if all commands succeed, transaction will auto-rollback when disposed if either commands fails
                transaction.Commit();
            }
        }

        public List<Transfer> GetAll()
        {
            if (_context.Transfers == null)
            {
                _logger.LogInformation("Error getting all user transfers:");
                throw new KeyNotFoundException("Table 'Transfers' not found");
            }

            return _context.Transfers.ToList();
        }

        //Helper Methods
        private Transaction CreateTransaction(int AccountId, decimal Amount, DateTime CreatedAt)
        {
            var transaction = new Transaction()
            {
                AccountId = AccountId,
                Amount = Amount,
                CreatedAt = CreatedAt
            };

            _context.Transactions.Add(transaction);

            return transaction;
        }

        private Account UpdateAccountBalance(Transaction movement, decimal amount)
        {
            var account = _context.Accounts.Find(movement.AccountId);
            if (account == null)
            {
                _logger.LogInformation("Error updating account balance:");
                throw new AppException("Account does not exist.");
            }
            

            account.Balance += movement.Amount;
            return account;
        }
    }
}