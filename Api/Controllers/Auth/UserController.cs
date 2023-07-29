using Application.Persistance;
using Domain.Entities.Auth;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers.Auth
{
    [ApiController]
    [Route("api/[controller]")]
    public class UserController : ControllerBase
    {
        private readonly IUnitOfWork _unitOfWork;

        public UserController(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        [HttpPost]
        public async Task<IActionResult> Create(User request)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState.ValidationState);

            var exists = _unitOfWork.Users.Find(r => request.Username == r.Username).Count() > 0;
            if (!exists)
            {
                await _unitOfWork.Users.Add(request);
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
            var users = await _unitOfWork.Users.GetAll();
            return Ok(users.OrderBy(e => e.Username));
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

        [HttpPut("{id:guid}")]
        public async Task<IActionResult> Update(Guid id, User request)
        {
            if (id != request.Id)
            {
                return BadRequest();
            }
            if (!ModelState.IsValid)
                return BadRequest(ModelState.ValidationState);

            await _unitOfWork.Users.Update(request);
            return Ok(request);
        }
    }
}
