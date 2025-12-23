using Application.Contracts;
using Domain.Entities.Production;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers.Production
{
    [ApiController]
    [Route("api/[controller]")]
    public class ProductionPartController(IUnitOfWork unitOfWork, IWorkOrderService workOrderService, IMetricsService costsService) : ControllerBase
    {
        [HttpGet("{id:guid}")]
        public async Task<IActionResult> GetById(Guid id)
        {
            var productionPart = await unitOfWork.ProductionParts.Get(id);
            if (productionPart == null) return NotFound();
            else return Ok(productionPart);
        }

        [HttpGet("WorkOrder/{id:guid}")]
        public IActionResult GetByWorkOrderId(Guid id)
        {
            var productionParts = unitOfWork.ProductionParts.Find(wo => wo.WorkOrderId == id);
            return Ok(productionParts);
        }

        [HttpGet]
        public IActionResult GetBetweenDates(DateTime startTime, DateTime endTime, Guid? workcenterId, Guid? operatorId, Guid? workorderId)
        {
            IEnumerable<ProductionPart> productionParts = new List<ProductionPart>();
            productionParts = unitOfWork.ProductionParts.Find(r => r.Date >= startTime && r.Date <= endTime);

            if (workcenterId != null)
                productionParts = productionParts.Where(r => r.WorkcenterId == workcenterId);
            if (operatorId != null)
                productionParts = productionParts.Where(r => r.OperatorId == operatorId);
            if (workorderId != null)
                productionParts = productionParts.Where(r => r.WorkOrderId == workorderId);

            return Ok(productionParts);
        }

        [HttpPost]
        public async Task<IActionResult> Create(ProductionPart productionPart)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState.ValidationState);

            // Get current costs from master data
            var costsRequest = await costsService.GetProductionPartCosts(productionPart);
            if (costsRequest.Result && costsRequest.Content is ProductionMetrics costs)
            {
                productionPart.MachineHourCost = costs.MachineCost;
                productionPart.OperatorHourCost = costs.OperatorCost;
            }

            await unitOfWork.ProductionParts.Add(productionPart);
            await workOrderService.AddProductionPart(productionPart.WorkOrderId, productionPart);
            return Created("",productionPart);
        }
        [HttpPut("{id:guid}")]
        public async Task<IActionResult> Update(Guid Id, ProductionPart request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState.ValidationState);
            if (Id != request.Id)
                return BadRequest();

            var exists = await unitOfWork.ProductionParts.Exists(request.Id);
            if (!exists)
                return NotFound();

            await unitOfWork.ProductionParts.Update(request);
            return Ok(request);
        }

        [HttpDelete("{id:guid}")]
        public async Task<IActionResult> Delete(Guid id)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState.ValidationState);

            var entity = unitOfWork.ProductionParts.Find(e => e.Id == id).FirstOrDefault();
            if (entity is null)
                return NotFound();

            await workOrderService.RemoveProductionPart(entity.WorkOrderId, entity);
            await unitOfWork.ProductionParts.Remove(entity);
            
            return Ok(entity);
        }
    }
}
