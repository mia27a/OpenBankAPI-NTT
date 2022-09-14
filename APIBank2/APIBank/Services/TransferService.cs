namespace APIBank.Services
{
    public class TransferService : ITransferService
    {
        private postgresContext _context;
        private readonly IMapper _mapper;

        public TransferService(postgresContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        public void Create(TransferRequest model)
        {
            using (var transaction = _context.Database.BeginTransaction())
            {
                try
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


                    // Create Transactions in each account and Save
                    Transaction movementFrom = CreateTransaction(transfer.FromAccountId, -transfer.Amount, DateTime.Now);
                    Transaction movementTo = CreateTransaction(transfer.ToAccountId, transfer.Amount, DateTime.Now);

                    _context.SaveChanges();


                    //Update Account Balance
                    var accountFrom = UpdateAccountBalance(movementFrom);
                    var accountTo = UpdateAccountBalance(movementTo);

                    _context.SaveChanges();


                    // Commit transaction if all commands succeed, transaction will auto-rollback
                    // when disposed if either commands fails
                    transaction.Commit();

                }
                catch (Exception)
                {
                    //TODO
                }
            }
        }

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

        private Account UpdateAccountBalance(Transaction movement)
        {
            var account = _context.Accounts.Find(movement.AccountId);
            if (account != null)
            {
                account.Balance += movement.Amount;
            }
            return account;
        }

        public List<Transfer> GetAll()
        {
            if(_context.Transfers == null)
            {
                throw new KeyNotFoundException("Table 'Transfers' not found");
            }

            return _context.Transfers.ToList();
        }

        //public AccountMovims GetById(int accountId, int userId)
        //{
        //    if (_context.Accounts == null || _context.Transactions == null)
        //    {
        //        throw new KeyNotFoundException("A table you are trying to access was not found");
        //    }

        //    var account = _context.Accounts
        //                    .Include(a => a.Transactions)
        //                    .Where(a => a.Id == accountId && a.UserId == userId)
        //                    .Select(a => new AccountMovims
        //                    {
        //                        Account = new AccountRe
        //                        {
        //                            Balance = a.Balance,
        //                            Currency = a.Currency,
        //                            CreatedAt = a.CreatedAt,
        //                            Id = a.Id

        //                        },
        //                        Movims = a.Transactions.Select(b => new Movim
        //                        {
        //                            Amount = b.Amount,
        //                            CreatedAt = b.CreatedAt
        //                        }).ToArray()
        //                    }).SingleOrDefault();

        //    if (account == null)
        //    {
        //        throw new KeyNotFoundException("Account not found");
        //    }


        //    return account;
        //}


        //public void Update(int id, UpdateRequest model)
        //{
        //    var account = GetAccount(id);

        //    // copy model to account and save
        //    _mapper.Map(model, account);
        //    _context.Accounts.Update(account);
        //    _context.SaveChanges();
        //}
        
        /*public void Delete(int id)
        {
            var account = GetAccount(id);
            _context.Accounts.Remove(account);
            _context.SaveChanges();
        }*/
    }
}
