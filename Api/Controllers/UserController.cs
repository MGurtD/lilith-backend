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

        [HttpPost]
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

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var users = await _unitOfWork.Users.GetAll();
            return Ok(users);
        }

        [HttpGet("{id:guid}")]
        public async Task<IActionResult> GetById(Guid id)
        {
            var user = await _unitOfWork.Users.Get(id);
            if (user is not null)
            {
                return Ok(user);
            }
            else
            {
                return NotFound();
            }
        }

        [HttpPut]
        public async Task<IActionResult> Update(UserDto requestUser)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState.ValidationState);

            var userDb = _unitOfWork.Users.Find(e => e.Username == requestUser.Username).FirstOrDefault();
            if (userDb is null)
            {
                return NotFound();
            }

            var user = _mapper.Map<User>(requestUser);
            await _unitOfWork.Users.Update(user);  
            return Ok(user);
        }
    }
}
