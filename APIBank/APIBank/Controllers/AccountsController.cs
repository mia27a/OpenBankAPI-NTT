using APIBank.Models.Responses;
using APIBank.Services.Interfaces;

namespace APIBank.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class AccountsController : CustomControllerBase
    {
        private readonly IAccountService _accountService;
        private readonly ILogger<AccountsController> _logger;
        private readonly IJwtUtils _jwtUtils;
        private readonly IUserService _userService;

        public AccountsController(IAccountService accountService, ILogger<AccountsController> logger, IJwtUtils jwtUtils, IUserService userService)
        {
            _accountService = accountService ?? throw new ArgumentNullException(nameof(accountService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _jwtUtils = jwtUtils ?? throw new ArgumentNullException(nameof(jwtUtils));
            _userService = userService ?? throw new ArgumentNullException(nameof(userService));
        }


        [HttpPost("Create")]
        [ProducesResponseType(typeof(AccountRequestResponse), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(string), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(string), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(string), StatusCodes.Status500InternalServerError)]
        public IActionResult Create([FromBody] AccountCreateRequest model)
        {
            //Get userId from token in Header
            var userId = GetUserIdFromClaim();
            if (userId == null || userId == 0)
            {
                return BadRequest();
            }
            RefreshToken refToken = _userService.GetRefreshTokenById(GetSessionIdFromClaim());
            if (!refToken.IsActive)
                return Unauthorized();

            var newAccount = _accountService.Create(model, (int)userId);
            _logger.LogInformation("Account Created");
            return Created("", /*new { message = "Account created successfully" }, */newAccount);
        }


        [HttpGet("GetAll")]
        [ProducesResponseType(typeof(List<AccountRequestResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(string), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(string), StatusCodes.Status500InternalServerError)]
        public IActionResult GetAllFromUser()
        {
            //Get userId from token in Header
            var userId = GetUserIdFromClaim();
            RefreshToken refToken = _userService.GetRefreshTokenById(GetSessionIdFromClaim());
            if (!refToken.IsActive)
                return Unauthorized();

            //Get all accounts from userId
            var accounts = _accountService.GetAll(userId);
            return Ok(accounts);
        }


        [HttpGet("GetById/{accountId}")]
        [ProducesResponseType(typeof(AccountMovims), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(string), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(string), StatusCodes.Status500InternalServerError)]
        public IActionResult GetById(int accountId)
        {
            //Get userId from token in Header
            var userId = GetUserIdFromClaim();
            RefreshToken refToken = _userService.GetRefreshTokenById(GetSessionIdFromClaim());
            if (!refToken.IsActive)
                return Unauthorized();

            // Get account by account id from user
            var account = _accountService.GetById(accountId, userId);
            return Ok(account);
        }

        #region Comentados
        /*[HttpPut("{id}")]
        public IActionResult Update(int id, UpdateRequest model)
        {
            _accountService.Update(id, model);
            return Ok(new { message = "Account updated successfully" });
        }

        [HttpDelete("{id}")]
        public IActionResult Delete(int id)
        {
            _accountService.Delete(id);
            return Ok(new { message = "Account deleted successfully" });
        }*/
        #endregion
    }
}
