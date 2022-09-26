using APIBank.Models.MyRequestResponses;

namespace APIBank.Services.Interfaces
{
    public interface IUserService
    {
        LoginResponse Login(UserLoginRequest model, string ipAddress);
        void Create(UserCreateRequest model);
        IEnumerable<User> GetAll();
        User GetById(int id);
        void Update(int id, UserUpdateRequest model);
        void Delete(int id);
        public IQueryable<RefreshToken> GetAllUserRefreshTokens(int id);
        LoginResponse RefreshToken(string token, string ipAddress);
        void RevokeToken(string refToken, string ipAddress);
        public RefreshToken GetRefreshTokenById(int id);
    }
}