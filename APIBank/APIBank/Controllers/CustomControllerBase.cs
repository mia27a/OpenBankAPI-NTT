using Microsoft.Extensions.Primitives;
using System.IdentityModel.Tokens.Jwt;
using System.Reflection;

namespace APIBank.Controllers
{
    [ApiController, Authorize, Route("api/[controller]")]
    public class CustomControllerBase : ControllerBase
    {
        protected int GetUserIdFromClaim()
        {
            if (!Request.Headers.TryGetValue("Authorization", out StringValues authToken))
                throw new AppException("Missing Authorization Header.");
                //return new TokenUserIdResponse { ErrorMessage = "Missing Authorization Header." };

            JwtSecurityToken token = new(authToken.ToString().Replace("Bearer ", "")); // remove Bearer prefix

            if (!int.TryParse(token.Claims.FirstOrDefault(claim => claim.Type == "id").Value, out int userId))
                throw new AppException("UserId Cannot be null");
            //return new TokenUserIdResponse { ErrorMessage = "UserId Cannot be null" };

            return userId;
            //return new TokenUserIdResponse { UserId = userId };
        }
    }
}