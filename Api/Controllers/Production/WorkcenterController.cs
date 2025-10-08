using Application.Contracts;
using Application.Persistance;
using Application.Services;
using Domain.Entities.Production;
using Domain.Entities.Sales;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers.Production
{
    [ApiController]
    [Route("api/[controller]")]
    public class WorkcenterController : ControllerBase
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILocalizationService _localizationService;

        public WorkcenterController(IUnitOfWork unitOfWork, ILocalizationService localizationService)
        {
            _unitOfWork = unitOfWork;
            _localizationService = localizationService;
        }

        [HttpPost]
        public async Task<IActionResult> Create(Workcenter request)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState.ValidationState);

            var exists = _unitOfWork.Workcenters.Find(r => request.Name == r.Name).Any();
            if (!exists)
            {
                await _unitOfWork.Workcenters.Add(request);
                var location = Url.Action(nameof(GetById), new { id = request.Id }) ?? $"/{request.Id}";
                return Created(location, request);
            }
            else
            {
                return Conflict(new GenericResponse(false, _localizationService.GetLocalizedString("WorkcenterAlreadyExists", request.Name)));
            }
        }
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var entities = await _unitOfWork.Workcenters.GetAll();

            return Ok(entities.OrderBy(w => w.Name));
        }

        [HttpGet("{id:guid}")]
        public async Task<IActionResult> GetById(Guid id)
        {
            var entity = await _unitOfWork.Workcenters.Get(id);
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

            var exists = await _unitOfWork.Workcenters.Exists(request.Id);
            if (!exists)
                return NotFound();

            await _unitOfWork.Workcenters.Update(request);
            return Ok(request);
        }

        [HttpDelete("{id:guid}")]
        public async Task<IActionResult> Delete(Guid id)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState.ValidationState);

            var entity = _unitOfWork.Workcenters.Find(e => e.Id == id).FirstOrDefault();
            if (entity is null)
                return NotFound();

            await _unitOfWork.Workcenters.Remove(entity);
            return Ok(entity);
        }
    }
}
