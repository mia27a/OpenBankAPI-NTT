using APIBank.Services.Interfaces;

namespace APIBank.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class TransfersController : CustomControllerBase
    {
        private readonly ITransferService _transferService;
        private readonly IAccountService _accountService;

        public TransfersController(ITransferService transferService, IAccountService accountService)
        {
            _transferService = transferService;
            _accountService = accountService;
        }
        //get account by user id e by id

        [HttpPost("TransferRequest")]
        [ProducesResponseType(typeof(Transfer), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(string), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(string), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(string), StatusCodes.Status500InternalServerError)]
        public IActionResult Create(TransferRequest model)
        {
            var userId = GetUserIdFromClaim();
            /*var accountById = */_accountService.GetById(model.FromAccountId, userId);
            //if (accountById == null)
            //    return BadRequest("Account Id not valid for this user");

            _transferService.Create(model);
            return Created("", new { message = "Transfer was successful" });
        }

        //[HttpGet("GetAll")]
        //public IActionResult GetAll()
        //{
        //    var transfers = _transferService.GetAll();
        //    return Ok(transfers);
        //}


        #region Comentados
        //[HttpGet("GetById/{id}")]
        //public IActionResult GetById(int id)
        //{
        //    var user = _transferService.GetById(id);
        //    return Ok(user);
        //}

        //[HttpPut("Update/{id}")]
        //public IActionResult Update(int id, UpdateRequest model)
        //{
        //    _transferService.Update(id, model);
        //    return Ok(new { message = "User updated successfully" });
        //}

        //[HttpDelete("Delete/{id}")]
        //public IActionResult Delete(int id)
        //{
        //    _transferService.Delete(id);
        //    return Ok(new { message = "User deleted successfully" });
        //}
        #endregion
    }
}