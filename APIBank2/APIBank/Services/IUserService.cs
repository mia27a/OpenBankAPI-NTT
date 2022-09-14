namespace APIBank.Services
{
    public interface IUserService
    {
        LoginResponse Login(LoginRequest model, string ipAddress);
        LoginResponse RefreshToken(string token, string ipAddress);
        void RevokeToken(string token, string ipAddress);
        IEnumerable<User> GetAll();
        User GetById(int id);
        void Create(CreateUserRequest model);
        void Update(int id, UpdateRequest model);
        void Delete(int id);
    }
}
