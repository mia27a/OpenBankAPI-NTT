namespace APIBank.Services
{
    public interface IUserService
    {
        LoginResponse Login(LoginRequest model);
        IEnumerable<User> GetAll();
        User GetById(int id);
        void Create(CreateUserRequest model);
        void Update(int id, UpdateRequest model);
        void Delete(int id);
    }
}
