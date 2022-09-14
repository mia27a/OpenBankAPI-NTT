using APIBank.ModelEntities;
using Microsoft.Extensions.Options;

namespace APIBank.Services
{
    public class UserService : IUserService
    {
        private postgresContext _context;
        private IJwtUtils _jwtUtils;
        private readonly AppSettings _appSettings;
        private readonly IMapper _mapper;

        public UserService(postgresContext context, IJwtUtils jwtUtils, IOptions<AppSettings> appSettings, IMapper mapper)
        {
            _context = context;
            _jwtUtils = jwtUtils;
            _appSettings = appSettings.Value;
            _mapper = mapper;
        }

        private User GetUser(int id)
        {
            if (_context.Users == null) throw new KeyNotFoundException("Table 'Users' not found");

            var user = _context.Users.Find(id);

            if (user == null) throw new KeyNotFoundException("User not found");

            return user;
        }

        public LoginResponse Login(LoginRequest model, string ipAddress)
        {
            User user = _context.Users.SingleOrDefault(x => x.Username == model.Username);

            // validate
            if (user == null || !BCrypt.Net.BCrypt.Verify(model.Password, user.PasswordHash))
                throw new AppException("Username or password is incorrect");

            //var response = _mapper.Map<LoginResponse>(user);
            //response.Token = _jwtUtils.GenerateToken(user);
            //return response;

            // authentication successful so generate jwt and refresh tokens
            string jwtToken = _jwtUtils.GenerateToken(user);
            RefreshToken refreshToken = _jwtUtils.GenerateRefreshToken(ipAddress);
            user.RefreshTokens.Add(refreshToken);

            // remove old refresh tokens from user
            //RemoveOldRefreshTokens(user);

            // save changes to db
            _context.Update(user);
            _context.SaveChanges();

            return new LoginResponse(user, jwtToken, refreshToken.Token);
        }

        public void Create(CreateUserRequest model)
        {
            if (_context.Users == null)
            {
                throw new KeyNotFoundException("Table 'Users' not found");
            }
            // validate
            if (_context.Users.Any(x => x.Username == model.Username))
                throw new AppException("Username '" + model.Username + "' is already taken");
            if (_context.Users.Any(x => x.Email == model.Email))
                throw new AppException("Email '" + model.Email + "' is already taken");

            // map model to new user object
            var user = _mapper.Map<User>(model);

            // hash password
            user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(model.Password);

            // save user
            _context.Users.Add(user);
            _context.SaveChanges();
        }

        public void Delete(int id)
        {
            var user = GetUser(id);
            _context.Users.Remove(user);
            _context.SaveChanges();
        }

        public IEnumerable<User> GetAll()
        {
            if (_context.Users == null)
            {
                throw new KeyNotFoundException("Table 'Users' not found");
            }
            return _context.Users;
        }

        public User GetById(int id)
        {
            return GetUser(id);
        }

        public void Update(int id, UpdateRequest model)
        {
            User user = GetUser(id);

            // validate
            if (model.Username != user.Username && _context.Users.Any(x => x.Username == model.Username))
                throw new AppException("Username '" + model.Username + "' is already taken");

            // hash password if it was entered
            if (!string.IsNullOrEmpty(model.Password))
                user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(model.Password);

            // copy model to user and save
            _mapper.Map(model, user);
            _context.Users.Update(user);
            _context.SaveChanges();
        }

        public LoginResponse RefreshToken(string token, string ipAddress)
        {
            var user = GetUserByRefreshToken(token);
            var refreshToken = user.RefreshTokens.SingleOrDefault(x => x.Token == token);

            //if (refreshToken?.IsRevoked == true)//
            if (refreshToken?.Revoked != null)
            {
                // revoke all descendant tokens in case this token has been compromised
                RevokeDescendantRefreshTokens(refreshToken, user, ipAddress, $"Attempted reuse of revoked ancestor token: {token}");
                _context.Update(user);
                _context.SaveChanges();
            }

            /*if (refreshToken?.IsActive == false)*/
            if (refreshToken?.Revoked != null && DateTime.UtcNow >= refreshToken.Expires)
            {
                throw new AppException("Invalid token");
            }
                

            // replace old refresh token with a new one (rotate token)
            var newRefreshToken = RotateRefreshToken(refreshToken, ipAddress);
            user.RefreshTokens.Add(newRefreshToken);

            // remove old refresh tokens from user
            //RemoveOldRefreshTokens(user);

            // save changes to db
            _context.Update(user);
            _context.SaveChanges();

            // generate new jwt
            var jwtToken = _jwtUtils.GenerateToken(user);

            return new LoginResponse(user, jwtToken, newRefreshToken.Token);
        }

        public void RevokeToken(string token, string ipAddress)
        {
            var user = GetUserByRefreshToken(token);
            var refreshToken = user.RefreshTokens.Single(x => x.Token == token);

            //if (!refreshToken.IsActive)
            if (refreshToken.Revoked != null && DateTime.UtcNow >= refreshToken.Expires)
            {
                throw new AppException("Invalid token");
            }

            // revoke token and save
            RevokeRefreshToken(refreshToken, ipAddress, "Revoked without replacement");
            _context.Update(user);
            _context.SaveChanges();
        }



        private User GetUserByRefreshToken(string token)
        {
            var user = _context.Users.SingleOrDefault(u => u.RefreshTokens.Any(t => t.Token == token));

            if (user == null)
                throw new AppException("Invalid token");

            return user;
        }

        private RefreshToken RotateRefreshToken(RefreshToken refreshToken, string ipAddress)
        {
            var newRefreshToken = _jwtUtils.GenerateRefreshToken(ipAddress);
            RevokeRefreshToken(refreshToken, ipAddress, "Replaced by new token", newRefreshToken.Token);
            return newRefreshToken;
        }

        //private void RemoveOldRefreshTokens(User user)
        //{
        //    // remove old inactive refresh tokens from user based on Remover in app settings
        //    user.RefreshTokens.RemoveAll(x => /*!x.IsActive*/x.Revoked != null && DateTime.UtcNow >= x.Expires && x.Created.AddDays(_appSettings.RefreshTokenRemover) <= DateTime.UtcNow);
        //}



        private void RevokeDescendantRefreshTokens(RefreshToken refreshToken, User user, string ipAddress, string reason)
        {
            // recursively traverse the refresh token chain and ensure all descendants are revoked
            if (!string.IsNullOrEmpty(refreshToken.ReplacedByToken))
            {
                var childToken = user.RefreshTokens.SingleOrDefault(x => x.Token == refreshToken.ReplacedByToken);
                //if (childToken.IsActive)
                if (childToken.Revoked == null && !(DateTime.UtcNow >= childToken.Expires))
                {
                    RevokeRefreshToken(childToken, ipAddress, reason);
                }
                else
                {
                    RevokeDescendantRefreshTokens(childToken, user, ipAddress, reason);
                }
            }
        }

        private static void RevokeRefreshToken(RefreshToken? token, string ipAddress, string reason = null, string replacedByToken = null)
        {
            if (token == null)
                return;
            token.Revoked = DateTime.UtcNow;
            token.RevokedByIp = ipAddress;
            token.ReasonRevoked = reason;
            token.ReplacedByToken = replacedByToken;
        }
    }
}