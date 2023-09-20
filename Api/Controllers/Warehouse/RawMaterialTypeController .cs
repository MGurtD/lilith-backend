using Application.Persistance;
using Domain.Entities.Warehouse;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers.Warehouse
{
    [ApiController]
    [Route("api/[controller]")]
    public class RawMaterialTypeController : ControllerBase
    {
        private readonly IUnitOfWork _unitOfWork;

        public RawMaterialTypeController(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        [HttpPost]
        public async Task<IActionResult> Create(RawMaterialType request)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState.ValidationState);

            var exists = _unitOfWork.RawMaterialTypes.Find(r => request.Name == r.Name).Any();
            if (!exists)
            {
                await _unitOfWork.RawMaterialTypes.Add(request);
                var location = Url.Action(nameof(GetById), new { id = request.Id }) ?? $"/{request.Id}";
                return Created(location, request);
            }
            else
            {
                return Conflict($"Area {request.Name} existent");
            }
        }
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var entities = await _unitOfWork.RawMaterialTypes.GetAll();

            return Ok(entities);
        }

        [HttpGet("{id:guid}")]
        public async Task<IActionResult> GetById(Guid id)
        {
            var entity = await _unitOfWork.RawMaterialTypes.Get(id);
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
        public async Task<IActionResult> Update(Guid Id, RawMaterialType request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState.ValidationState);
            if (Id != request.Id)
                return BadRequest();

            var exists = await _unitOfWork.RawMaterialTypes.Exists(request.Id);
            if (!exists)
                return NotFound();

            await _unitOfWork.RawMaterialTypes.Update(request);
            return Ok(request);
        }

        [HttpDelete("{id:guid}")]
        public async Task<IActionResult> Delete(Guid id)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState.ValidationState);

            var entity = _unitOfWork.RawMaterialTypes.Find(e => e.Id == id).FirstOrDefault();
            if (entity is null)
                return NotFound();

            await _unitOfWork.RawMaterialTypes.Remove(entity);
            return Ok(entity);
        }
    }
}
