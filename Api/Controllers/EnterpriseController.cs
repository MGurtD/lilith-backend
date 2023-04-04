using Microsoft.AspNetCore.Mvc;
using Application.Persistance;
using Domain.Entities;

namespace Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class EnterpriseController : ControllerBase
    {
        private readonly ILogger<EnterpriseController> _logger;
        private readonly IUnitOfWork _unitOfWork;

        public EnterpriseController(ILogger<EnterpriseController> logger, IUnitOfWork unitOfWork)
        {
            _logger = logger;
            _unitOfWork = unitOfWork;
        }

        [HttpPost]
        public async Task<IActionResult> Create(Enterprise requestEnterprise)
        {
            // Validation the incoming request
            if (!ModelState.IsValid) return BadRequest();

            // Validate existence of the unique user key
            var dbEnterprise = _unitOfWork.Enterprises.Find(e => e.Name == requestEnterprise.Name).SingleOrDefault();
            if (dbEnterprise is not null)
            {
                return BadRequest($"Enterprise with name {requestEnterprise.Name} already exists");
            }

            requestEnterprise.CreatedOn = DateTime.UtcNow;
            requestEnterprise.UpdatedOn = DateTime.UtcNow;

            await _unitOfWork.Enterprises.Add(requestEnterprise);
            return Ok();
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var enterprises = await _unitOfWork.Enterprises.GetAll();
            return Ok(enterprises);
        }

    }
}