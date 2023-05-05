using Application.Dtos;
using Application.Persistance;
using AutoMapper;
using Domain.Entities;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UserController : ControllerBase
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public UserController(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        [HttpGet]
        public async Task<IActionResult> Create(UserDto request)
        {
            if(!ModelState.IsValid) return BadRequest(ModelState.ValidationState);

            var exists = _unitOfWork.Users.Find(r => request.Username == r.Username).Count()>0;
            if(!exists)
            {
                var user = _mapper.Map<User>(request);
                await _unitOfWork.Users.Add(user);
                return Ok(user);
            }
            else
            {
                return StatusCode(StatusCodes.Status500InternalServerError);
            }
        }
    }
}
