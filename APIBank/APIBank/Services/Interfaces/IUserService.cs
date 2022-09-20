namespace APIBank.Services.Interfaces
{
    public interface IUserService
    {
        LoginResponse Login(LoginRequest model, string ipAddress);
        void Create(CreateUserRequest model);
        IEnumerable<User> GetAll();
        User GetById(int id);
        void Update(int id, UpdateRequest model);
        void Delete(int id);
        public IQueryable<RefreshToken> GetAllUserRefreshTokens(int id);

        LoginResponse RefreshToken(string token, string ipAddress);
        //void RevokeToken(string token, string ipAddress);
    }
}