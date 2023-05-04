using Microsoft.AspNetCore.Mvc;
using Application.Persistance;
using Domain.Entities;
using AutoMapper;
using Api.Mapping.Dtos;

namespace Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class RoleController : ControllerBase
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public RoleController(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        [HttpPost]
        public async Task<IActionResult> Create(RoleDto request)
        {
            // Validation the incoming request
            if (!ModelState.IsValid) return BadRequest(ModelState.ValidationState);


            // Validate existence of the unique user key
            var exists = _unitOfWork.Roles.Find(r => request.Name == r.Name).Count() > 0;
            if (!exists)
            {
                var role = _mapper.Map<Role>(request);
                await _unitOfWork.Roles.Add(role);

                return Ok(role);
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
            return Ok(roles);
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