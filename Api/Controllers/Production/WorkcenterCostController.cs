using Application.Contracts;
using Application.Persistance;
using Application.Services;
using Domain.Entities.Production;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers.Production
{
    [ApiController]
    [Route("api/[controller]")]
    public class WorkcenterCostController : ControllerBase
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILocalizationService _localizationService;

        public WorkcenterCostController(IUnitOfWork unitOfWork, ILocalizationService localizationService)
        {
            _unitOfWork = unitOfWork;
            _localizationService = localizationService;
        }

        [HttpPost]
        public async Task<IActionResult> Create(WorkcenterCost request)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState.ValidationState);

            var exists = _unitOfWork.WorkcenterCosts.Find(r => request.MachineStatusId == r.MachineStatusId && request.WorkcenterId == r.WorkcenterId).Any();
            if (!exists)
            {
                await _unitOfWork.WorkcenterCosts.Add(request);
                var location = Url.Action(nameof(GetById), new { id = request.Id }) ?? $"/{request.Id}";
                return Created(location, request);
            }
            else
            {
                return Conflict(new GenericResponse(false, _localizationService.GetLocalizedString("WorkcenterCostAlreadyExists")));
            }
        }
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var entities = await _unitOfWork.WorkcenterCosts.GetAll();

            return Ok(entities);
        }

        [HttpGet("{id:guid}")]
        public async Task<IActionResult> GetById(Guid id)
        {
            var entity = await _unitOfWork.WorkcenterCosts.Get(id);
            if (entity is not null)
            {
                return Ok(entity);
            }
            else
            {
                return NotFound();
            }
        }
        [HttpGet("workcenter/{workcenterId:guid}/status/{statusId:guid}")]
        public async Task<IActionResult> GetByWorkcenterAndStatusId(Guid workcenterId, Guid statusId)
        {
            var workcentercost = (await _unitOfWork.WorkcenterCosts.FindAsync(r => r.WorkcenterId == workcenterId && r.MachineStatusId == statusId)).FirstOrDefault();
            if(workcentercost == null)
            {
                return NotFound();
            }
            return Ok(workcentercost);
        }
        [HttpPut("{id:guid}")]
        public async Task<IActionResult> Update(Guid Id, WorkcenterCost request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState.ValidationState);
            if (Id != request.Id)
                return BadRequest();

            var exists = await _unitOfWork.WorkcenterCosts.Exists(request.Id);
            if (!exists)
                return NotFound();

            await _unitOfWork.WorkcenterCosts.Update(request);
            return Ok(request);
        }

        [HttpDelete("{id:guid}")]
        public async Task<IActionResult> Delete(Guid id)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState.ValidationState);

            var entity = _unitOfWork.WorkcenterCosts.Find(e => e.Id == id).FirstOrDefault();
            if (entity is null)
                return NotFound();

            await _unitOfWork.WorkcenterCosts.Remove(entity);
            return Ok(entity);
        }

    }
}
