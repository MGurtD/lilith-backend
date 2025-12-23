using Application.Contracts;
using Domain.Entities.Production;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers.Production
{
    [ApiController]
    [Route("api/[controller]")]
    public class OperatorController(IUnitOfWork unitOfWork, ILocalizationService localizationService) : ControllerBase
    {
        [HttpPost]
        public async Task<IActionResult> Create(Operator request)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState.ValidationState);

            var exists = unitOfWork.Operators.Find(r => request.Code == r.Code).Any();
            if (!exists)
            {
                await unitOfWork.Operators.Add(request);
                var location = Url.Action(nameof(GetById), new { id = request.Id }) ?? $"/{request.Id}";
                return Created(location, request);
            }
            else
            {
                return Conflict(new GenericResponse(false, localizationService.GetLocalizedString("OperatorAlreadyExists", request.Name)));
            }
        }
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var entities = await unitOfWork.Operators.GetAll();

            return Ok(entities.OrderBy(w => w.Name));
        }

        [HttpGet("{id:guid}")]
        public async Task<IActionResult> GetById(Guid id)
        {
            var entity = await unitOfWork.Operators.Get(id);
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
        public async Task<IActionResult> Update(Guid Id, Operator request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState.ValidationState);
            if (Id != request.Id)
                return BadRequest();

            var exists = await unitOfWork.Operators.Exists(request.Id);
            if (!exists)
                return NotFound();

            await unitOfWork.Operators.Update(request);
            return Ok(request);
        }

        [HttpDelete("{id:guid}")]
        public async Task<IActionResult> Delete(Guid id)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState.ValidationState);

            var entity = unitOfWork.Operators.Find(e => e.Id == id).FirstOrDefault();
            if (entity is null)
                return NotFound();

            await unitOfWork.Operators.Remove(entity);
            return Ok(entity);
        }
    }
}
