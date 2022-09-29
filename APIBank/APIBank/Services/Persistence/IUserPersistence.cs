namespace APIBank.Services.Persistence
{
    public interface IUserPersistence
    {
        public User GetUserByUsername(UserLoginRequest model);
    }
}
