namespace APIBank.Services
{
    public interface IUserPersistence
    {
        public User GetUserByUsername(UserLoginRequest model);
    }
}
