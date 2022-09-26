using Microsoft.Extensions.Primitives;
using System.IdentityModel.Tokens.Jwt;

namespace APIBank.Controllers
{
    [ApiController, Authorize, Route("api/[controller]")]
    public class CustomControllerBase : ControllerBase
    {
        protected int GetUserIdFromClaim()
        {
            var token = GetToken();

            if (!int.TryParse(token.Claims.FirstOrDefault(claim => claim.Type == "id").Value, out int userId))
                throw new AppException("UserId Cannot be null");

            return userId;
        }

        protected int GetSessionIdFromClaim()
        {
            var token = GetToken();

            if (!int.TryParse(token.Claims.FirstOrDefault(claim => claim.Type == "sessionId").Value, out int sessionId))
                throw new AppException("SessionId Cannot be null");

            return sessionId;
        }

        private JwtSecurityToken GetToken()
        {
            if (!Request.Headers.TryGetValue("Authorization", out StringValues authToken))
                throw new AppException("Missing Authorization Header.");

            return new(authToken.ToString().Replace("Bearer ", ""));// remove Bearer prefix
        }
    }
}