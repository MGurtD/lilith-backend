using Application.Persistance;
using Domain.Entities;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers
{
    [ApiController]
    [Route("test")]
    public class OperatorController : ControllerBase
    {
        private readonly IUnitOfWork unitOfWork;

        public OperatorController(IUnitOfWork unitOfWork)
        {
            this.unitOfWork = unitOfWork;
        }

        [HttpGet]
        public IActionResult GetAll()
        {
            var operators = unitOfWork.Operators.GetAll();
            return Ok(operators);
        }

    }
}
