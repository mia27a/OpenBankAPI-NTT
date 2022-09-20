using APIBank.Models.Users.Requests;
using APIBank.Models.Users.Responses;
using APIBank.Services.Interfaces;
using NuGet.Protocol.Plugins;
using System.Security.Claims;

namespace APIBank.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class UsersController : CustomControllerBase
    {
        private readonly IUserService _userService;

        public UsersController(IUserService userService)
        {
            _userService = userService;
        }

        [AllowAnonymous]
        [HttpPost("Login")]
        [ProducesResponseType(typeof(LoginResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(string), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(string), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(string), StatusCodes.Status500InternalServerError)]
        public IActionResult Login(LoginRequest model)
        {
            var response = _userService.Login(model, IpAddress());
            SetTokenCookie(response.RefreshToken);
            return Ok(response);
        }


        [AllowAnonymous]
        [HttpPost("Create")]
        [ProducesResponseType(typeof(CreateUserResponse), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(string), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(string), StatusCodes.Status500InternalServerError)]
        public IActionResult Create(CreateUserRequest model)
        {
            _userService.Create(model);
            return Created("", new { message = "User created successfully" });
        }

        [AllowAnonymous]
        [HttpPost("refreshToken")]
        [ProducesResponseType(typeof(LoginResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(string), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(string), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(string), StatusCodes.Status500InternalServerError)]
        public IActionResult RefreshToken()
        {
            var refreshToken = Request.Cookies["refreshToken"];
            var response = _userService.RefreshToken(refreshToken, IpAddress());
            SetTokenCookie(response.RefreshToken);
            return Ok(response);
        }

        //[HttpGet("GetAll")]
        //public IActionResult GetAll()
        //{
        //    var users = _userService.GetAll();
        //    return Ok(users);
        //}


        //[HttpGet("GetById/{id}")]
        //public IActionResult GetById(int id)
        //{
        //    var user = _userService.GetById(id);
        //    return Ok(user);
        //}


        //[HttpPut("Update/{id}")]
        //public IActionResult Update(int id, UpdateRequest model)
        //{
        //    _userService.Update(id, model);
        //    return Ok(new { message = "User updated successfully" });
        //}


        //[HttpDelete("Delete/{id}")]
        //public IActionResult Delete(int id)
        //{
        //    _userService.Delete(id);
        //    return Ok(new { message = "User deleted successfully" });
        //}


        //[HttpGet("{id}/refresh-tokens")]
        //public IActionResult GetRefreshTokens(int id)
        //{
        //    var userRefreshTokens = _userService.GetAllUserRefreshTokens(id);
        //    return Ok(userRefreshTokens);
        //    #region OldVersion
        //    //var user = _userService.GetById(id);
        //    //return Ok(user.RefreshTokenCollection);
        //    #endregion
        //}


        #region Future Implementations: RevokeToken
        //[HttpPost("revoke-token")]
        //public IActionResult RevokeToken(RevokeTokenRequest model)
        //{
        //    // accept refresh token in request body or cookie
        //    var token = model.Token != null ? model.Token : Request.Cookies["refreshToken"];

        //    if (string.IsNullOrEmpty(token))
        //    {     //throw new AppException? instead of BadRequest
        //        return BadRequest(new
        //        {
        //            message = "Token is required"
        //        });
        //    }

        //    _userService.RevokeToken(token, IpAddress());
        //    return Ok(new { message = "Token revoked" });
        //}
        #endregion


        // helper methods
        private void SetTokenCookie(string token)
        {
            // append cookie with refresh token to the http response
            CookieOptions cookieOptions = new()
            {
                HttpOnly = true,
                Expires = DateTime.UtcNow.AddDays(7)
            };
            Response.Cookies.Append("refreshToken", token, cookieOptions);
        }

        private string IpAddress()
        {
            // get source ip address for the current request
            if (Request.Headers.ContainsKey("X-Forwarded-For"))
                return Request.Headers["X-Forwarded-For"];
            else
                return HttpContext.Connection.RemoteIpAddress.MapToIPv4().ToString();
        }
    }
}