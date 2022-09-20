using APIBank.ModelEntities;
using APIBank.Models.Accounts;
using APIBank.Services.Interfaces;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Primitives;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Principal;

namespace APIBank.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class AccountsController : CustomControllerBase
    {
        private readonly IAccountService _accountService;

        public AccountsController(IAccountService accountService)
        {
            _accountService = accountService;
        }


        [HttpPost("Create")]
        [ProducesResponseType(typeof(AccountRe), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(string), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(string), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(string), StatusCodes.Status500InternalServerError)]
        public IActionResult Create([FromBody] CreateAccountRequest model)
        {
            //Get userId from token in Header
            var userId = GetUserIdFromClaim();
            //if (!result.IsValid)
            //    throw new AppException(result.ErrorMessage);

            _accountService.Create(model, userId/*result.UserId.Value*/);
            return Created("", new { message = "Account created successfully" });
        }


        [HttpGet("GetAll")]
        [ProducesResponseType(typeof(Account), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(string), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(string), StatusCodes.Status500InternalServerError)]
        public IActionResult GetAllFromUser()
        {
            //Get userId from token in Header
            var userId = GetUserIdFromClaim();
            //if (!result.IsValid)
            //    throw new AppException(result.ErrorMessage);

            //Get all accounts from userId
            var accounts = _accountService.GetAll(userId/*result.UserId.Value*/);
            return Ok(accounts);
        }


        [HttpGet("GetById/{accountId}")]
        [ProducesResponseType(typeof(Account), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(string), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(string), StatusCodes.Status500InternalServerError)]
        public IActionResult GetById(int accountId)
        {
            //Get userId from token in Header
            var userId = GetUserIdFromClaim();
            //if (!result.IsValid)
            //    throw new AppException(result.ErrorMessage);

            // Get account by account id from user
            var account = _accountService.GetById(accountId, userId/*result.UserId.Value*/);
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
