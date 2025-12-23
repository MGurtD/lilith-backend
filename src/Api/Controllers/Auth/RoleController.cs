using Microsoft.AspNetCore.Mvc;
using Application.Contracts;
using Domain.Entities.Auth;

namespace Api.Controllers.Auth
{
    [ApiController]
    [Route("api/[controller]")]
    public class RoleController(IUnitOfWork unitOfWork) : ControllerBase
    {
        [HttpPost]
        public async Task<IActionResult> Create(Role request)
        {
            // Validation the incoming request
            if (!ModelState.IsValid) return BadRequest(ModelState.ValidationState);


            // Validate existence of the unique user key
            var exists = unitOfWork.Roles.Find(r => request.Name == r.Name).Any();
            if (!exists)
            {
                await unitOfWork.Roles.Add(request);

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
            var roles = await unitOfWork.Roles.GetAll();
            return Ok(roles.OrderBy(e => e.Name));
        }

        [HttpGet("{id:guid}")]
        public async Task<IActionResult> GetById(Guid id)
        {
            var role = await unitOfWork.Roles.Get(id);
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
            var role = await unitOfWork.Roles.Get(id);
            if (role is not null)
            {
                await unitOfWork.Roles.Remove(role);
                return NoContent();
            }
            else
            {
                return NotFound();
            }
        }

    }
}