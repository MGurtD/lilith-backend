using Application.Persistance;
using Domain.Entities.Production;
using Microsoft.AspNetCore.Mvc;
using System.Security.Policy;

namespace Api.Controllers.Production
{
    [ApiController]
    [Route("api/[controller]")]
    public class OperatorCostController : ControllerBase
    {
        private readonly IUnitOfWork _unitOfWork;

        public OperatorCostController(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }
        [HttpPost]
        public async Task<IActionResult> Create(OperatorCost request)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState.ValidationState);

            var exists = _unitOfWork.OperatorCosts.Find(r => request.MachineStatusId == r.MachineStatusId && request.OperatorTypeId == r.OperatorTypeId).Any();
            if (!exists)
            {
                await _unitOfWork.OperatorCosts.Add(request);
                var location = Url.Action(nameof(GetById), new { id = request.Id }) ?? $"/{request.Id}";
                return Created(location, request);
            }
            else
            {
                return Conflict($"Cost existent");
            }
        }
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var entities = await _unitOfWork.OperatorCosts.GetAll();

            return Ok(entities);
        }

        [HttpGet("{id:guid}")]
        public async Task<IActionResult> GetById(Guid id)
        {
            var entity = await _unitOfWork.OperatorCosts.Get(id);
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
        public async Task<IActionResult> Update(Guid Id, OperatorCost request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState.ValidationState);
            if (Id != request.Id)
                return BadRequest();

            var exists = await _unitOfWork.OperatorCosts.Exists(request.Id);
            if (!exists)
                return NotFound();

            await _unitOfWork.OperatorCosts.Update(request);
            return Ok(request);
        }

        [HttpDelete("{id:guid}")]
        public async Task<IActionResult> Delete(Guid id)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState.ValidationState);

            var entity = _unitOfWork.OperatorCosts.Find(e => e.Id == id).FirstOrDefault();
            if (entity is null)
                return NotFound();

            await _unitOfWork.OperatorCosts.Remove(entity);
            return Ok(entity);
        }
    }
}
