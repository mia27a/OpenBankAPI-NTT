using APIBank.Models.MyRequestResponses;
using APIBank.Services.Interfaces;
using APIBank.Services.Persistence;
using Microsoft.Extensions.Options;

namespace APIBank.Services
{
    public class UserService : IUserService
    {
        private PostgresContext _context;
        private IJwtUtils _jwtUtils;
        private readonly AppSettings _appSettings; // usado para o Remove Old Refresh Tokens
        private readonly IMapper _mapper;
        private readonly IUserPersistence _userPersistence;

        public UserService(PostgresContext context, IJwtUtils jwtUtils, IOptions<AppSettings> appSettings, IMapper mapper, IUserPersistence userPersistence)
        {
            _context = context;
            _jwtUtils = jwtUtils;
            _appSettings = appSettings.Value;
            _mapper = mapper;
            _userPersistence = userPersistence;
        }

        public LoginResponse Login(UserLoginRequest model, string ipAddress)
        {
            User user = _userPersistence.GetUserByUsername(model);

            // validate
            if (!BCrypt.Net.BCrypt.Verify(model.Password, user.PasswordHash))
                throw new AppException("Password is incorrect"); // CustomBadRequest

            #region Version Without RefreshToken
            //var response = _mapper.Map<LoginResponse>(user);
            //response.Token = _jwtUtils.GenerateToken(user);
            //return response;
            #endregion

            // authentication successful so generate jwt and refresh tokens           
            RefreshToken refreshToken = _jwtUtils.GenerateRefreshToken(ipAddress);
            user.RefreshTokenCollection.Add(refreshToken);
            _context.SaveChanges();
            string jwtToken = _jwtUtils.GenerateToken(user, /*AQUI!*/refreshToken);//AQUI!


            // remove old refresh tokens from user --- to be implemented correctly!!!
            RemoveOldRefreshTokens(user);

            // save changes to db
            _context.Update(user);
            _context.Update(refreshToken);
            _context.SaveChanges();

            return new LoginResponse(user, jwtToken, refreshToken.RefToken);
        }

        public LoginResponse RefreshToken(string token, string ipAddress)
        {
            var user = GetUserByRefreshToken(token);

            List<RefreshToken> refreshTokenCollection = GetAllUserRefreshTokens(user.Id).ToList();
            var refreshToken = refreshTokenCollection.SingleOrDefault(x => x.RefToken == token);
            //var refreshToken = user.RefreshTokenCollection.SingleOrDefault(x => x.RefToken == token);

            if (refreshToken.IsRevoked)
            {
                // revoke all descendant tokens in case this token has been compromised
                RevokeDescendantRefreshTokens(refreshToken, user, ipAddress, $"Attempted reuse of revoked ancestor token: {token}");
                _context.Update(user);
                _context.Update(refreshToken);
                _context.SaveChanges();
            }

            if (!refreshToken.IsActive)
            {
                throw new AppException("Invalid token"); // Custom Bad Request
            }

            // replace old refresh token with a new one (rotate token)
            var newRefreshToken = RotateRefreshToken(refreshToken, ipAddress);
            user.RefreshTokenCollection.Add(newRefreshToken);

            // remove old refresh tokens from user --- to be implemented correctly!!!
            RemoveOldRefreshTokens(user);

            // save changes to db
            _context.Update(user);
            _context.SaveChanges();

            // generate new jwt
            var jwtToken = _jwtUtils.GenerateToken(user, newRefreshToken); //AQUI!

            return new LoginResponse(user, jwtToken, newRefreshToken.RefToken);
        }

        public void Create(UserCreateRequest model)
        {
            if (_context.Users == null)
            {
                throw new KeyNotFoundException("Table 'Users' not found");
            }
            // validate
            if (_context.Users.Any(x => x.Username == model.Username) || _context.Users.Any(x => x.Email == model.Email))
                throw new AppException("Username or Email are already taken"); // Custom BadRequest

            // map model to new user object
            var user = _mapper.Map<User>(model);

            // hash password
            user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(model.Password);

            // save user
            _context.Users.Add(user);
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

        public void Update(int id, UserUpdateRequest model)
        {
            User user = GetUser(id);

            // validate
            if (model.Username != user.Username && _context.Users.Any(x => x.Username == model.Username))
                throw new AppException("Username '" + model.Username + "' is already taken"); //Custom BadRequest

            // hash password if it was entered
            if (!string.IsNullOrEmpty(model.Password))
                user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(model.Password);

            // copy model to user and save
            _mapper.Map(model, user);
            _context.Users.Update(user);
            _context.SaveChanges();
        }

        public void Delete(int id)
        {
            var user = GetUser(id);
            _context.Users.Remove(user);
            _context.SaveChanges();
        }

        public IQueryable<RefreshToken> GetAllUserRefreshTokens(int id)
        {
            var query = _context.RefreshTokens
                .Where(rt => rt.UserId == id)
                .AsNoTracking()
                .Select(
                    rt => new RefreshToken
                    {
                        Id = rt.Id,
                        UserId = rt.UserId,
                        RefToken = rt.RefToken,
                        Expires = rt.Expires,
                        Created = rt.Created,
                        CreatedByIp = rt.CreatedByIp,
                        Revoked = rt.Revoked,
                        RevokedByIp = rt.RevokedByIp,
                        ReplacedByToken = rt.ReplacedByToken,
                        ReasonRevoked = rt.ReasonRevoked,
                        IsExpired = rt.IsExpired,
                        IsRevoked = rt.IsRevoked,
                        IsActive = rt.IsActive
                    });

            if (query == null)
                return null;

            return query;
        }


        public void RevokeToken(string refToken, string ipAddress)
        {
            User user = GetUserByRefreshToken(refToken);
            List<RefreshToken> refreshTokenCollection = GetAllUserRefreshTokens(user.Id).ToList();
            var refreshtoken = _context.FindAsync<RefreshToken>(refreshTokenCollection.Single(refTokenQuery => refTokenQuery.RefToken == refToken).Id).GetAwaiter().GetResult();
            //user.RefreshTokenCollection.Single(refTokenQuery => refTokenQuery.RefToken == refToken); // Old Version

            if (!refreshtoken.IsActive)
            {
                throw new AppException("Invalid token"); // Custom BadRequest
            }

            // revoke token and save
            RevokeRefreshToken(refreshtoken, ipAddress, "Revoked without replacement");
            //_context.Update(user);
            //_context.Update(refreshtoken);
            _context.SaveChanges();
        }


        //Helper Methods
        private User GetUser(int id)
        {
            if (_context.Users == null)
                throw new KeyNotFoundException("Table 'Users' not found");

            var user = _context.Users.Find(id);

            if (user == null)
                throw new KeyNotFoundException("User not found");

            return user;
        }

        private User GetUserByRefreshToken(string refToken)
        {
            User? user = _context.Users.SingleOrDefault(userQuery => userQuery.RefreshTokenCollection.Any(refTokenQuery => refTokenQuery.RefToken == refToken));

            if (user == null)
                throw new AppException("Invalid token"); // Custom BadRequest

            return user;
        }

        public RefreshToken GetRefreshTokenById(int id)
        {
            if (_context.RefreshTokens == null)
                throw new KeyNotFoundException("Table 'RefreshToken' not found");

            var refreshToken = _context.RefreshTokens.Find(id);

            if (refreshToken == null)
                throw new KeyNotFoundException("Refresh Token not found");

            return refreshToken;
        }

        private RefreshToken RotateRefreshToken(RefreshToken refreshToken, string ipAddress)
        {
            var newRefreshToken = _jwtUtils.GenerateRefreshToken(ipAddress);
            RevokeRefreshToken(refreshToken, ipAddress, "Replaced by new token", newRefreshToken.RefToken);
            return newRefreshToken;
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

        private void RevokeDescendantRefreshTokens(RefreshToken refreshToken, User user, string ipAddress, string reason)
        {
            // recursively traverse the refresh token chain and ensure all descendants are revoked
            if (!string.IsNullOrEmpty(refreshToken.ReplacedByToken))
            {
                List<RefreshToken> childTokenCollection = GetAllUserRefreshTokens(user.Id).ToList();
                var childToken = childTokenCollection.SingleOrDefault(x => x.RefToken == refreshToken.ReplacedByToken);
                //var childToken = user.RefreshTokenCollection.SingleOrDefault(x => x.RefToken == refreshToken.ReplacedByToken); // Old Version


                if (childToken.IsActive)
                {
                    RevokeRefreshToken(childToken, ipAddress, reason);
                }
                else
                {
                    RevokeDescendantRefreshTokens(childToken, user, ipAddress, reason);
                }
            }
        }

        private void RemoveOldRefreshTokens(User user) //TESTAR!!!!!!!!!!!!!!!!!!!!!!!! Algo não está a funcionar bem...
        {
            // remove old inactive refresh tokens from user based on Remover in app settings

            //user.RefreshTokenCollection.RemoveAll(x => !x.IsActive && x.Created.AddDays(_appSettings.RefreshTokenRemover) <= DateTime.UtcNow); //OldVersion with Collection Not Working
            List<RefreshToken> refreshTokenCollection = GetAllUserRefreshTokens(user.Id).ToList();
            foreach (var refToken in refreshTokenCollection.Where(x => !x.IsActive && x.Created.AddDays(_appSettings.RefreshTokenRemover) <= DateTime.UtcNow))
            {
                {
                    _context.Remove(refToken);
                    _context.SaveChanges();
                }
            }
        }
    }
}