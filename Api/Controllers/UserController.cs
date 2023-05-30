using Api.Mapping.Dtos;
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
            var usersDto = _mapper.Map<IEnumerable<UserDto>>(users);
            return Ok(usersDto);
        }

        [HttpGet("{id:guid}")]
        public async Task<IActionResult> GetById(Guid id)
        {
            var user = await _unitOfWork.Users.Get(id);
            if (user is not null)
            {
                var userDto = _mapper.Map<UserDto>(user);
                return Ok(userDto);
            }
            else
            {
                return NotFound();
            }
        }

        [HttpPut("{id:guid}")]
        public async Task<IActionResult> Update(Guid id, UserDto requestUser)
        {
            if (id != requestUser.Id) {
                return BadRequest();
            }
            if (!ModelState.IsValid)
                return BadRequest(ModelState.ValidationState);

            var userDb = _unitOfWork.Users.Find(e => e.Username == requestUser.Username).FirstOrDefault();
            if (userDb is null)
            {
                return NotFound();
            };

            userDb.FirstName = requestUser.FirstName;
            userDb.LastName = requestUser.LastName;
            userDb.Disabled = requestUser.Disabled;
            userDb.RoleId = requestUser.RoleId;

            await _unitOfWork.Users.Update(userDb);  
            return Ok(userDb);
        }
    }
}
