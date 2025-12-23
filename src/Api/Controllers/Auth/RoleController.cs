using Microsoft.AspNetCore.Mvc;
using Application.Contracts;
using Domain.Entities.Auth;

namespace Api.Controllers.Auth
{
    [ApiController]
    [Route("api/[controller]")]
    public class RoleController : ControllerBase
    {
        private readonly IUnitOfWork _unitOfWork;

        public RoleController(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        [HttpPost]
        public async Task<IActionResult> Create(Role request)
        {
            // Validation the incoming request
            if (!ModelState.IsValid) return BadRequest(ModelState.ValidationState);


            // Validate existence of the unique user key
            var exists = _unitOfWork.Roles.Find(r => request.Name == r.Name).Count() > 0;
            if (!exists)
            {
                await _unitOfWork.Roles.Add(request);

                return Ok(request);
            }
            else
            {
                return StatusCode(StatusCodes.Status500InternalServerError);
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var roles = await _unitOfWork.Roles.GetAll();
            return Ok(roles.OrderBy(e => e.Name));
        }

        [HttpGet("{id:guid}")]
        public async Task<IActionResult> GetById(Guid id)
        {
            var role = await _unitOfWork.Roles.Get(id);
            if (role is not null)
            {
                return Ok(role);
            }
            else
            {
                return NotFound();
            }
        }

        [HttpDelete("{id:guid}")]
        public async Task<IActionResult> Delete(Guid id)
        {
            var role = await _unitOfWork.Roles.Get(id);
            if (role is not null)
            {
                await _unitOfWork.Roles.Remove(role);
                return NoContent();
            }
            else
            {
                return NotFound();
            }
        }

    }
}