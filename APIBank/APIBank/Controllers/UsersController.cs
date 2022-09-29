using APIBank.Models.MyRequestResponses;
using APIBank.Services.Interfaces;

namespace APIBank.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class UsersController : CustomControllerBase
    {
        private readonly IUserService _userService;

        public UsersController(IUserService userService) { _userService = userService; }

        [AllowAnonymous]
        [HttpPost("Login")]
        [ProducesResponseType(typeof(LoginResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(string), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(string), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(string), StatusCodes.Status500InternalServerError)]
        public IActionResult Login(UserLoginRequest model)
        {
            var response = _userService.Login(model, IpAddress());
            SetTokenCookie(response.RefreshToken);
            return Ok(response);
        }


        [AllowAnonymous]
        [HttpPost("Create")]
        [ProducesResponseType(typeof(string), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(string), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(string), StatusCodes.Status500InternalServerError)]
        public IActionResult Create(UserCreateRequest model)
        {
            _userService.Create(model);
            return Created(string.Empty, new { message = "User created successfully" });
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


        [HttpGet("{id}/refresh-tokens")]
        public IActionResult GetRefreshTokens(int id)
        {
            RefreshToken refToken = _userService.GetRefreshTokenById(GetSessionIdFromClaim());
            if (!refToken.IsActive)
                return Unauthorized();

            var userRefreshTokens = _userService.GetAllUserRefreshTokens(id);
            return Ok(userRefreshTokens);
            #region OldVersion
            //var user = _userService.GetById(id);
            //return Ok(user.RefreshTokenCollection);
            #endregion
        }

        [HttpPost("Logout")]
        public IActionResult Logout(/*UserRevokeTokenRequest model*/)
        {
            RefreshToken refToken = _userService.GetRefreshTokenById(GetSessionIdFromClaim());
            if (!refToken.IsActive)
                return Unauthorized();

            // accept refresh token in request body or cookie
            var token = /*!model.RefToken.IsNullOrEmpty() ? model.RefToken :*/ Request.Cookies["refreshToken"];

            if (string.IsNullOrEmpty(token))
                return BadRequest(new { message = "Token is required" });

            _userService.RevokeToken(token, IpAddress());
            return Ok(new { message = "Logout Successful" });
        }

        // helper methods
        private void SetTokenCookie(string refToken)
        {
            // append cookie with refresh token to the http response
            CookieOptions cookieOptions = new() { HttpOnly = true, Expires = DateTime.UtcNow.AddDays(7) };
            Response.Cookies.Append("refreshToken", refToken, cookieOptions);
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