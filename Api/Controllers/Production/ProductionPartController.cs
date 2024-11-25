using Api.Services.Production;
using Application.Persistance;
using Application.Production.Warehouse;
using Application.Services.Production;
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
        private readonly IWorkOrderService _workOrderService;
        private readonly IMetricsService _costsService;

        public ProductionPartController(IUnitOfWork unitOfWork, IWorkOrderService workOrderService, IMetricsService costsService)
        {
            _unitOfWork = unitOfWork;
            _workOrderService = workOrderService;
            _costsService = costsService;
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
        public IActionResult GetBetweenDates(DateTime startTime, DateTime endTime, Guid? workcenterId, Guid? operatorId, Guid? workorderId)
        {
            IEnumerable<ProductionPart> productionParts = new List<ProductionPart>();
            productionParts = _unitOfWork.ProductionParts.Find(r => r.Date >= startTime && r.Date <= endTime);

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
            var costsRequest = await _costsService.GetProductionPartCosts(productionPart);
            if (costsRequest.Result && costsRequest.Content is ProductionMetrics costs)
            {
                productionPart.MachineHourCost = costs.MachineCost;
                productionPart.OperatorHourCost = costs.OperatorCost;
            }

            await _unitOfWork.ProductionParts.Add(productionPart);
            await _workOrderService.AddProductionPart(productionPart.WorkOrderId, productionPart);
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

            await _workOrderService.RemoveProductionPart(entity.WorkOrderId, entity);
            await _unitOfWork.ProductionParts.Remove(entity);
            
            return Ok(entity);
        }
    }
}
