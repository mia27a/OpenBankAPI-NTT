namespace APIBank.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class AccountsController : ControllerBase
    {
        private IAccountService _accountService;
        private IMapper _mapper;

        public AccountsController(IAccountService accountService, IMapper mapper)
        {
            _accountService = accountService;
            _mapper = mapper;
        }

        [HttpPost("Create")]
        public IActionResult Create([FromBody] CreateAccountRequest model)
        {
            _accountService.Create(model);
            return Ok(new { message = "Account created successfully" });
        }

        [HttpGet("GetAll")]
        public IActionResult GetAll(int userId)
        {
            var accounts = _accountService.GetAll(userId);
            return Ok(accounts);
        }

        [HttpGet("GetById/{accountId}")]
        public IActionResult GetById(int accountId, int userId)
        {
            var account = _accountService.GetById(accountId, userId);
            return Ok(account);
        }


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
    }
}