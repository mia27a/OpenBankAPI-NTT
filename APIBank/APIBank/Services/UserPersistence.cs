namespace APIBank.Services
{
    public class UserPersistence : IUserPersistence
    {
        private PostgresContext _context;

        public UserPersistence(PostgresContext postgresContext)
        {
            _context = postgresContext;
        }
        public User GetUserByUsername(UserLoginRequest model)
        {
            return _context.Users.SingleOrDefault(x => x.Username == model.Username) ?? throw new AppException("Username is incorrect");
        }
    }
}
