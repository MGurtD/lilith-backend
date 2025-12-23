using Application.Contracts;
using Domain.Entities.Auth;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers.Auth
{
    [ApiController]
    [Route("api/[controller]")]
    public class UserFilterController : ControllerBase
    {
        private readonly IUnitOfWork _unitOfWork;

        public UserFilterController(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        [HttpGet("{userId:guid}")]
        public IActionResult Get(Guid userId)
        {
            var userFilter = _unitOfWork.UserFilters.Find(r => userId == r.UserId);
            if (userFilter == null)
                return NotFound();

            return Ok(userFilter);
        }

        [HttpGet("{userId:guid}/{page}/{key}")]
        public IActionResult Get(Guid userId, string page, string key)
        {
            var userFilter = _unitOfWork.UserFilters.Find(r => userId == r.UserId && page == r.Page && key == r.Key);
            if (userFilter == null)
                return NotFound();

            return Ok(userFilter);
        }


        [HttpPost]
        public async Task<IActionResult> CreateOrUpdate(UserFilter request)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState.ValidationState);

            var userFilter = _unitOfWork.UserFilters.Find(r => request.UserId == r.UserId && request.Page == r.Page && request.Key == r.Key).FirstOrDefault();
            if (userFilter == null)
            {
                await _unitOfWork.UserFilters.Add(request);
                return Ok(request);
            }
            else
            {
                userFilter.Filter = request.Filter;
                await _unitOfWork.UserFilters.Update(userFilter);
                return Ok(userFilter);
            }
        }

        [HttpDelete("{id:guid}")]
        public async Task<IActionResult> Delete(Guid id)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState.ValidationState);

            var userFilter = await _unitOfWork.UserFilters.Get(id);
            if (userFilter == null)
                return NotFound();

            await _unitOfWork.UserFilters.Remove(userFilter);
            return Ok();
        }
    }
}
