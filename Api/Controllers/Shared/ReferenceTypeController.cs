using Application.Persistance;
using Domain.Entities.Shared;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers.Shared
{
    [ApiController]
    [Route("api/[controller]")]
    public class ReferenceTypeController : ControllerBase
    {
        private readonly IUnitOfWork _unitOfWork;

        public ReferenceTypeController(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        [HttpPost]
        public async Task<IActionResult> Create(ReferenceType request)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState.ValidationState);

            var exists = _unitOfWork.ReferenceTypes.Find(r => request.Name == r.Name).Any();
            if (!exists)
            {
                await _unitOfWork.ReferenceTypes.Add(request);
                var location = Url.Action(nameof(GetById), new { id = request.Id }) ?? $"/{request.Id}";
                return Created(location, request);
            }
            else
            {
                return Conflict($"ReferenceType {request.Name} existent");
            }
        }
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var entities = await _unitOfWork.ReferenceTypes.GetAll();

            return Ok(entities);
        }

        [HttpGet("{id:guid}")]
        public async Task<IActionResult> GetById(Guid id)
        {
            var entity = await _unitOfWork.ReferenceTypes.Get(id);
            if (entity is not null)
            {
                return Ok(entity);
            }
            else
            {
                return NotFound();
            }
        }
        [HttpPut("{id:guid}")]
        public async Task<IActionResult> Update(Guid Id, ReferenceType request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState.ValidationState);
            if (Id != request.Id)
                return BadRequest();

            var exists = await _unitOfWork.ReferenceTypes.Exists(request.Id);
            if (!exists)
                return NotFound();

            await _unitOfWork.ReferenceTypes.Update(request);
            return Ok(request);
        }

        [HttpDelete("{id:guid}")]
        public async Task<IActionResult> Delete(Guid id)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState.ValidationState);

            var entity = _unitOfWork.ReferenceTypes.Find(e => e.Id == id).FirstOrDefault();
            if (entity is null)
                return NotFound();

            await _unitOfWork.ReferenceTypes.Remove(entity);
            return Ok(entity);
        }
    }
}
