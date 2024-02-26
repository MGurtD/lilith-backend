using Application.Persistance;
using Domain.Entities.Production;
using Infrastructure.Migrations;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;

namespace Api.Controllers.Production
{
    [ApiController]
    [Route("api/[controller]")]
    public class ProductionPartController : ControllerBase
    {
        private readonly IUnitOfWork _unitOfWork;

        public ProductionPartController(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        [HttpGet("{id:guid}")]
        public async Task<IActionResult> GetById(Guid id)
        {
            var productionPart = await _unitOfWork.ProductionParts.Get(id);
            if (productionPart == null) return NotFound();
            else return Ok(productionPart);
        }

        [HttpGet("WorkOrder/{id:guid}")]
        public IActionResult GetByWorkOrderId(Guid id)
        {
            var productionParts = _unitOfWork.ProductionParts.Find(wo => wo.WorkOrderId == id);
            return Ok(productionParts);
        }

        [HttpGet]
        public IActionResult GetBetweenDates(DateTime startTime, DateTime endTime, Guid? workcenterId, Guid? operatorId)
        {
            IEnumerable<ProductionPart> productionParts = new List<ProductionPart>();
            productionParts = _unitOfWork.ProductionParts.Find(r => r.Date >= startTime && r.Date <= endTime);

            if (workcenterId != null)
                productionParts = productionParts.Where(r => r.WorkcenterId == workcenterId);
            if (operatorId != null)
                productionParts = productionParts.Where(r => r.OperatorId == operatorId);

            return Ok(productionParts);
        }

        [HttpPost]
        public async Task<IActionResult> Create(ProductionPart productionPart)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState.ValidationState);
            await _unitOfWork.ProductionParts.Add(productionPart);
            return Created("",productionPart);
        }
        [HttpPut("{id:guid}")]
        public async Task<IActionResult> Update(Guid Id, ProductionPart request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState.ValidationState);
            if (Id != request.Id)
                return BadRequest();

            var exists = await _unitOfWork.ProductionParts.Exists(request.Id);
            if (!exists)
                return NotFound();

            await _unitOfWork.ProductionParts.Update(request);
            return Ok(request);
        }

        [HttpDelete("{id:guid}")]
        public async Task<IActionResult> Delete(Guid id)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState.ValidationState);

            var entity = _unitOfWork.ProductionParts.Find(e => e.Id == id).FirstOrDefault();
            if (entity is null)
                return NotFound();

            await _unitOfWork.ProductionParts.Remove(entity);
            return Ok(entity);
        }
    }
}
