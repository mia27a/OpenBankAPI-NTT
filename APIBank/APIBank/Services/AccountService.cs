using APIBank.Models.Accounts;
using APIBank.Models.Transactions;
using Microsoft.EntityFrameworkCore;

namespace APIBank.Services
{
    public class AccountService : IAccountService
    {
        private postgresContext _context;
        private readonly IMapper _mapper;

        public AccountService(postgresContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        public void Create(CreateAccountRequest model)
        {
            if(_context.Accounts == null || _context.Transactions == null)
            {
                throw new KeyNotFoundException("A table you are trying to access was not found");
            }
            
            using (var transaction = _context.Database.BeginTransaction())
            {
                try
                {
                    Account account = new Account();
                    Transaction movim = new Transaction();

                    account.UserId = model.UserId;
                    account.Currency = model.Currency;
                    account.Balance = model.Amount;

                    _context.Accounts.Add(account);
                    _context.SaveChanges();

                    movim.AccountId = account.Id;
                    movim.Amount = model.Amount;
                    movim.Balance = account.Balance;

                    _context.Transactions.Add(movim);
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

        public List<AccountRe> GetAll(int userId)
        {
            if(_context.Accounts == null)
            {
                throw new KeyNotFoundException("Table 'Accounts' not found");
            }
            var accounts = _context.Accounts.Where(u => u.UserId == userId).Select(u => new AccountRe
            {
                Balance = u.Balance,
                CreatedAt = u.CreatedAt,
                Currency = u.Currency,
                Id = u.Id
            }).ToList();

            return accounts;
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
                                Account = new AccountRe
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
                throw new KeyNotFoundException("Account not found");
            }


            return account;
        }

        #region GetById Attempts
        /* public List<AccountRe> GetById(int id, int userId)
        {
            if (_context.Accounts == null)
            {
                throw new KeyNotFoundException("Table 'Accounts' not found");
            }

            var account = _context.Accounts.Where(a => a.Id == id && a.UserId == userId);

            *//*.Find(id);*//*
            if (account == null)
            {
                throw new KeyNotFoundException("Account not found");
            }


            return _mapper.Map<List<AccountRe>>(account);
        }*/

        //public List<AccountMovims> GetById(int accountId, int userId)
        //{
        //    List<AccountMovims> result = new List<AccountMovims>();

        //    if (_context.Accounts == null && _context.Transactions == null)
        //    {
        //        throw new KeyNotFoundException("Table not found");
        //    }

        //    var account = _context.Accounts.Where(a => a.Id == accountId && a.UserId == userId).Select(a =>
        //        new AccountRe
        //        {
        //            Balance = a.Balance,
        //            Currency = a.Currency,
        //            CreatedAt = a.CreatedAt,
        //            Id = a.Id

        //        }).SingleOrDefault();

        //    var transactions = _context.Transactions.Where(t => t.AccountId == accountId).Select(t =>
        //        new Movim
        //        {
        //            Amount = t.Amount,
        //            CreatedAt = t.CreatedAt
        //        }
        //        ).ToArray();

        //    AccountMovims accMov = new AccountMovims
        //    {
        //        Account = account,
        //        Movims = transactions
        //    };

        //    result.Add(accMov);

        //    /*.Find(id);*/
        //    if (account == null)
        //    {
        //        throw new KeyNotFoundException("Account not found");
        //    }


        //    return result;
        //}
        #endregion


        public Account Update(int id, UpdateAccountRequest model)
        {
            //USERS
            if (_context.Accounts == null)
            {
                throw new KeyNotFoundException("Table 'Accounts' not found");
            }
            var account = _context.Accounts.Find(id);
            if (account == null)
            {
                throw new KeyNotFoundException("Account not found");
            }

            _mapper.Map(model, account);
            _context.Accounts.Update(account);
            _context.SaveChanges();
            return account;
        }
        
        /*public void Delete(int id)
        {
            var account = GetAccount(id);
            _context.Accounts.Remove(account);
            _context.SaveChanges();
        }*/
    }
}
