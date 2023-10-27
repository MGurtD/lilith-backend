using Application.Persistance;
using Domain.Entities.Production;
using Domain.Entities.Sales;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers.Production
{
    [ApiController]
    [Route("api/[controller]")]
    public class WorkcenterTypeController : ControllerBase
    {
        private readonly IUnitOfWork _unitOfWork;

        public WorkcenterTypeController(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        [HttpPost]
        public async Task<IActionResult> Create(WorkcenterType request)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState.ValidationState);

            var exists = _unitOfWork.WorkcenterTypes.Find(r => request.Name == r.Name).Any();
            if (!exists)
            {
                await _unitOfWork.WorkcenterTypes.Add(request);
                var location = Url.Action(nameof(GetById), new { id = request.Id }) ?? $"/{request.Id}";
                return Created(location, request);
            }
            else
            {
                return Conflict($"WorkcenterType {request.Name} existent");
            }
        }
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var entities = await _unitOfWork.WorkcenterTypes.GetAll();

            return Ok(entities.OrderBy(w => w.Name));
        }

        [HttpGet("{id:guid}")]
        public async Task<IActionResult> GetById(Guid id)
        {
            var entity = await _unitOfWork.WorkcenterTypes.Get(id);
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
        public async Task<IActionResult> Update(Guid Id, WorkcenterType request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState.ValidationState);
            if (Id != request.Id)
                return BadRequest();

            var exists = await _unitOfWork.WorkcenterTypes.Exists(request.Id);
            if (!exists)
                return NotFound();

            await _unitOfWork.WorkcenterTypes.Update(request);
            return Ok(request);
        }

        [HttpDelete("{id:guid}")]
        public async Task<IActionResult> Delete(Guid id)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState.ValidationState);

            var entity = _unitOfWork.WorkcenterTypes.Find(e => e.Id == id).FirstOrDefault();
            if (entity is null)
                return NotFound();

            await _unitOfWork.WorkcenterTypes.Remove(entity);
            return Ok(entity);
        }
    }
}
