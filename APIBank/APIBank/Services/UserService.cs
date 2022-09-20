using APIBank.ModelEntities;
using APIBank.Services.Interfaces;
using Microsoft.Extensions.Options;

namespace APIBank.Services
{
    public class UserService : IUserService
    {
        private PostgresContext _context;
        private IJwtUtils _jwtUtils;
        private readonly AppSettings _appSettings; // usado para o Remove Old Refresh Tokens
        private readonly IMapper _mapper;

        public UserService(PostgresContext context, IJwtUtils jwtUtils, IOptions<AppSettings> appSettings, IMapper mapper)
        {
            _context = context;
            _jwtUtils = jwtUtils;
            _appSettings = appSettings.Value;
            _mapper = mapper;
        }

        public LoginResponse Login(LoginRequest model, string ipAddress)
        {
            User user = _context.Users.SingleOrDefault(x => x.Username == model.Username);

            // validate
            if (user == null || !BCrypt.Net.BCrypt.Verify(model.Password, user.PasswordHash))
                throw new AppException("Username or password is incorrect"); // CustomBadRequest

            #region Version Without RefreshToken
            //var response = _mapper.Map<LoginResponse>(user);
            //response.Token = _jwtUtils.GenerateToken(user);
            //return response;
            #endregion

            // authentication successful so generate jwt and refresh tokens
            string jwtToken = _jwtUtils.GenerateToken(user);
            RefreshToken refreshToken = _jwtUtils.GenerateRefreshToken(ipAddress);
            user.RefreshTokenCollection.Add(refreshToken);

            #region RemoveOldRefreshTokens to be implemented
            // remove old refresh tokens from user --- to be implemented
            //RemoveOldRefreshTokens(user);
            #endregion

            // save changes to db
            _context.Update(user);
            _context.SaveChanges();

            return new LoginResponse(user, jwtToken, refreshToken.RefToken);
        }

        public LoginResponse RefreshToken(string token, string ipAddress)
        {
            var user = GetUserByRefreshToken(token);
            var refreshToken = user.RefreshTokenCollection.SingleOrDefault(x => x.RefToken == token);

            if (refreshToken?.IsRevoked == true)
            {
                // revoke all descendant tokens in case this token has been compromised
                RevokeDescendantRefreshTokens(refreshToken, user, ipAddress, $"Attempted reuse of revoked ancestor token: {token}");
                _context.Update(user);
                _context.SaveChanges();
            }

            if (refreshToken?.IsActive == false)
            {
                throw new AppException("Invalid token"); // CustomBad Request
            }

            // replace old refresh token with a new one (rotate token)
            var newRefreshToken = RotateRefreshToken(refreshToken, ipAddress);
            user.RefreshTokenCollection.Add(newRefreshToken);

            #region RemoveOldRefreshTokens to be implemented
            // remove old refresh tokens from user
            //RemoveOldRefreshTokens(user);
            #endregion

            // save changes to db
            _context.Update(user);
            _context.SaveChanges();

            // generate new jwt
            var jwtToken = _jwtUtils.GenerateToken(user);

            return new LoginResponse(user, jwtToken, newRefreshToken.RefToken);
        }

        public void Create(CreateUserRequest model)
        {
            if(_context.Users == null)
            {
                throw new KeyNotFoundException("Table 'Users' not found");
            }
            // validate
            if(_context.Users.Any(x => x.Username == model.Username) || _context.Users.Any(x => x.Email == model.Email))
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
            if(_context.Users == null)
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
            if(model.Username != user.Username && _context.Users.Any(x => x.Username == model.Username))
                throw new AppException("Username '" + model.Username + "' is already taken"); //Custom BadRequest

            // hash password if it was entered
            if(!string.IsNullOrEmpty(model.Password))
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


        #region Future Implementations: RevokeToken
        //public void RevokeToken(string token, string ipAddress)
        //{
        //    var user = GetUserByRefreshToken(token);
        //    RefreshToken refreshToken = user.RefreshTokenCollection.Single(x => x.RefToken == token);

        //    if (!refreshToken.IsActive)
        //    {
        //        throw new AppException("Invalid token"); // Custom BadRequest
        //    }

        //    // revoke token and save
        //    RevokeRefreshToken(refreshToken, ipAddress, "Revoked without replacement");
        //    _context.Update(user);
        //    _context.SaveChanges();
        //}
        #endregion


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

        private User GetUserByRefreshToken(string token)
        {
            var user = _context.Users.SingleOrDefault(u => u.RefreshTokenCollection.Any(t => t.RefToken == token));

            if (user == null)
                throw new AppException("Invalid token"); // Custom BadRequest

            return user;
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
                var childToken = user.RefreshTokenCollection.SingleOrDefault(x => x.RefToken == refreshToken.ReplacedByToken);

                //if(childToken.Revoked == null && !(DateTime.UtcNow >= childToken.Expires))
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


        #region Future Implementations - Helper Methods: RemoveOldRefreshTokens
        //private void RemoveOldRefreshTokens(User user) //TESTAR!!!!!!!!!!!!!!!!!!!!!!!!
        //{
        //    // remove old inactive refresh tokens from user based on Remover in app settings
        //    user.RefreshTokenCollection.RemoveAll(x => !x.IsActive && x.Created.AddDays(_appSettings.RefreshTokenRemover) <= DateTime.UtcNow);


        //    //x.Revoked != null && DateTime.UtcNow >= x.Expires
        //    // remove old inactive refresh tokens from user based on Remover in app settings
        //    //foreach (var rtoken in user.RefreshTokenCollection.Where(x => !x.IsActive && x.Created.AddDays(_appSettings.RefreshTokenRemover) <= DateTime.UtcNow))
        //    //{
        //    //    _context.RefreshTokens.Remove(rtoken);
        //    //}
        //}

        #endregion
    }
}