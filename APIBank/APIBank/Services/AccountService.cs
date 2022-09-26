using APIBank.Models.Responses;
using APIBank.Services.Interfaces;

namespace APIBank.Services
{
    public class AccountService : IAccountService
    {
        private readonly PostgresContext _context;
        private readonly ILogger<AccountService> _logger;

        public AccountService(PostgresContext context, ILogger<AccountService> logger)
        {
            _context = context;
            _logger = logger;
        }

        public AccountRequestResponse Create(AccountCreateRequest model, int userId)
        {
            if (_context.Accounts == null || _context.Transactions == null)
            {
                _logger.LogInformation("Error creating account:");
                throw new KeyNotFoundException("A table you are trying to access was not found");
            }

            using (var transaction = _context.Database.BeginTransaction())
            {
                var account = new Account();
                var movim = new Transaction();

                account.UserId = userId;
                account.Currency = model.Currency;
                account.Balance = model.Amount;

                _context.Accounts.Add(account);
                _context.SaveChanges();

                movim.AccountId = account.Id;
                movim.Amount = model.Amount;
                movim.Balance = account.Balance;

                _context.Transactions.Add(movim);
                _context.SaveChanges();

                // Commit transaction if all commands succeed, transaction will auto-rollback when disposed if either commands fails
                transaction.Commit();

                return new AccountRequestResponse()
                {
                    Balance = account.Balance,
                    CreatedAt = DateTime.UtcNow,
                    Currency = account.Currency,
                    Id = account.Id,
                };
            }
        }

        public List<AccountRequestResponse> GetAll(int userId)
        {
            if (_context.Accounts == null)
            {
                throw new KeyNotFoundException("Table 'Accounts' not found");
            }

            return _context.Accounts.Where(u => u.UserId == userId).Select(u => new AccountRequestResponse
            {
                Balance = u.Balance,
                CreatedAt = u.CreatedAt,
                Currency = u.Currency,
                Id = u.Id
            }).ToList();
        }

        public AccountMovims GetById(int accountId, int userId)
        {
            if (_context.Accounts == null || _context.Transactions == null)
            {
                throw new KeyNotFoundException("Table not found");
            }

            var account = _context.Accounts
                            .Include(a => a.Transactions)
                            .Where(a => a.Id == accountId && a.UserId == userId)
                            .Select(a => new AccountMovims
                            {
                                Account = new AccountRequestResponse
                                {
                                    Balance = a.Balance,
                                    Currency = a.Currency,
                                    CreatedAt = a.CreatedAt,
                                    Id = a.Id

                                },
                                Movims = a.Transactions.Select(b => new Movim
                                {
                                    Amount = b.Amount,
                                    CreatedAt = b.CreatedAt
                                }).ToArray()
                            }).SingleOrDefault();

            if (account == null)
            {
                throw new AppException("Source Account does not exist or does not belong to this user.");
            }

            return account;
        }

        #region Comentados
        //public Account Update(int id, UpdateAccountRequest model)
        //{
        //    if (_context.Accounts == null)
        //    {
        //        throw new KeyNotFoundException("Table 'Accounts' not found");
        //    }
        //    var account = _context.Accounts.Find(id);
        //    if (account == null)
        //    {
        //        throw new KeyNotFoundException("Account not found");
        //    }

        //    _mapper.Map(model, account);
        //    _context.Accounts.Update(account);
        //    _context.SaveChanges();
        //    return account;
        //}


        /*public void Delete(int id)
        {
            var account = GetAccount(id);
            _context.Accounts.Remove(account);
            _context.SaveChanges();
        }*/
        #endregion
    }
}