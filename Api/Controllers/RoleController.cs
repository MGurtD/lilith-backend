using Microsoft.AspNetCore.Mvc;
using Application.Persistance;
using Domain.Entities;
using AutoMapper;
using Api.Mapping.Dtos;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class RoleController : ControllerBase
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly RoleManager<IdentityRole> _roleManager;

        public RoleController(IUnitOfWork unitOfWork, IMapper mapper, RoleManager<IdentityRole> roleManager)
        {
            _unitOfWork = unitOfWork;
            _roleManager = roleManager;
            _mapper = mapper;
        }

        [HttpPost]
        public async Task<IActionResult> Create(RoleDto request)
        {
            // Validation the incoming request
            if (!ModelState.IsValid) return BadRequest(ModelState.ValidationState);


            // Validate existence of the unique user key
            var roleDb = await _roleManager.FindByNameAsync(request.Name);
            if (roleDb is null)
            {
                var role = _mapper.Map<IdentityRole>(request);
                var result = await _roleManager.CreateAsync(role);

                if (result.Succeeded)
                {
                    return Ok(role);
                }
                else
                {
                    return StatusCode(StatusCodes.Status500InternalServerError);
                }
            }
            else
            {
                return BadRequest();
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var roles = await _roleManager.Roles.ToListAsync();
            return Ok(roles);
        }

        [HttpGet("{id:guid}")]
        public async Task<IActionResult> GetById(Guid id)
        {
            var role = await _roleManager.FindByIdAsync(id.ToString());
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
            var role = await _roleManager.FindByIdAsync(id.ToString());
            if (role is not null)
            {
                await _roleManager.DeleteAsync(role);
                return NoContent();
            }
            else
            {
                return NotFound();
            }
        }

    }
}