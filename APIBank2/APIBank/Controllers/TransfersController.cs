namespace APIBank.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class TransfersController : ControllerBase
    {
        private ITransferService _transferService;
        private IMapper _mapper;

        public TransfersController(ITransferService transferService, IMapper mapper)
        {
            _transferService = transferService;
            _mapper = mapper;
        }

        [HttpPost("TransferRequest")]
        public IActionResult Create(TransferRequest model)
        {
            _transferService.Create(model);
            return Ok(new { message = "Transfer created successfully" });
        }

        [HttpGet("GetAll")]
        public IActionResult GetAll()
        {
            var transfers = _transferService.GetAll();
            return Ok(transfers);
        }



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
    }
}