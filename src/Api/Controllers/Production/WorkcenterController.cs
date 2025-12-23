using Application.Contracts;
using Domain.Entities.Production;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers.Production
{
    [ApiController]
    [Route("api/[controller]")]
    public class WorkcenterController(IUnitOfWork unitOfWork, ILocalizationService localizationService) : ControllerBase
    {
        [HttpPost]
        public async Task<IActionResult> Create(Workcenter request)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState.ValidationState);

            var exists = unitOfWork.Workcenters.Find(r => request.Name == r.Name).Any();
            if (!exists)
            {
                await unitOfWork.Workcenters.Add(request);
                var location = Url.Action(nameof(GetById), new { id = request.Id }) ?? $"/{request.Id}";
                return Created(location, request);
            }
            else
            {
                return Conflict(new GenericResponse(false, localizationService.GetLocalizedString("WorkcenterAlreadyExists", request.Name)));
            }
        }
        
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var entities = await unitOfWork.Workcenters.GetAll();

            return Ok(entities.OrderBy(w => w.Name));
        }

        [HttpGet("plant")]
        public async Task<IActionResult> GetVisibleInPlant()
        {
            var entities = await unitOfWork.Workcenters.GetVisibleInPlant();
            return Ok(entities);
        }

        [HttpGet("{id:guid}")]
        public async Task<IActionResult> GetById(Guid id)
        {
            var entity = await unitOfWork.Workcenters.Get(id);
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
        public async Task<IActionResult> Update(Guid Id, Workcenter request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState.ValidationState);
            if (Id != request.Id)
                return BadRequest();

            var exists = await unitOfWork.Workcenters.Exists(request.Id);
            if (!exists)
                return NotFound();

            await unitOfWork.Workcenters.Update(request);
            return Ok(request);
        }

        [HttpDelete("{id:guid}")]
        public async Task<IActionResult> Delete(Guid id)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState.ValidationState);

            var entity = unitOfWork.Workcenters.Find(e => e.Id == id).FirstOrDefault();
            if (entity is null)
                return NotFound();

            await unitOfWork.Workcenters.Remove(entity);
            return Ok(entity);
        }
    }
}
