using Application.Contracts;
using Application.Persistance;
using Domain.Entities.Production;
using Domain.Entities.Sales;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers.Production
{
    [ApiController]
    [Route("api/[controller]")]
    public class WorkMasterController : ControllerBase
    {
        private readonly IUnitOfWork _unitOfWork;

        public WorkMasterController(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        [HttpPost]
        public async Task<IActionResult> Create(WorkMaster request)
        {
            // Validacions
            if (!ModelState.IsValid) return BadRequest(ModelState.ValidationState);

            var existsReference = await _unitOfWork.References.Exists(request.ReferenceId);
            if (!existsReference) return NotFound(new GenericResponse(false, $"Referencia inexistent"));

            var exists = _unitOfWork.WorkMasters.Find(w => w.Id == request.Id).Any();
            if (exists) return Conflict(new GenericResponse(false, $"Ruta de fabricació existent"));

            // Creació
            await _unitOfWork.WorkMasters.Add(request);
            var location = Url.Action(nameof(GetById), new { id = request.Id }) ?? $"/{request.Id}";
            return Created(location, request);
        }
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var entities = await _unitOfWork.WorkMasters.GetAll();

            return Ok(entities.OrderBy(w => w.ReferenceId));
        }

        [HttpGet("{id:guid}")]
        public async Task<IActionResult> GetById(Guid id)
        {
            var entity = await _unitOfWork.WorkMasters.Get(id);
            if (entity is not null)
                return Ok(entity);
            else
                return NotFound();
        }

        [HttpPut("{id:guid}")]
        public async Task<IActionResult> Update(Guid Id, WorkMaster request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState.ValidationState);
            if (Id != request.Id)
                return BadRequest();

            var exists = await _unitOfWork.WorkMasters.Exists(request.Id);
            if (!exists)
                return NotFound();

            await _unitOfWork.WorkMasters.Update(request);
            return Ok(request);
        }

        [HttpDelete("{id:guid}")]
        public async Task<IActionResult> Delete(Guid id)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState.ValidationState);

            var entity = _unitOfWork.WorkMasters.Find(e => e.Id == id).FirstOrDefault();
            if (entity is null)
                return NotFound();

            await _unitOfWork.WorkMasters.Remove(entity);
            return Ok(entity);
        }
    }
}
