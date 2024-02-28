using Application.Contracts;
using Application.Contracts.Production;
using Application.Contracts.Purchase;
using Application.Persistance;
using Application.Production.Warehouse;
using Domain.Entities.Production;
using Domain.Entities.Sales;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;

namespace Api.Controllers.Production
{
    [ApiController]
    [Route("api/[controller]")]
    public class WorkOrderController : ControllerBase
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IWorkOrderService _workOrderService;

        public WorkOrderController(IUnitOfWork unitOfWork, IWorkOrderService workOrderService)
        {
            _unitOfWork = unitOfWork;
            _workOrderService = workOrderService;
        }

        [HttpPost("CreateFromWorkMaster")]
        public async Task<IActionResult> CreateFromWorkMaster([FromBody] CreateWorkOrderDto request)
        {
            var creationResult = await _workOrderService.CreateFromWorkMaster(request);
            if (creationResult.Result)
                return Ok(creationResult);
            else
                return BadRequest(creationResult);
        }

        [HttpPost]
        public async Task<IActionResult> Create(WorkOrder request)
        {
            // Validacions
            if (!ModelState.IsValid) return BadRequest(ModelState.ValidationState);

            var existsReference = await _unitOfWork.References.Exists(request.ReferenceId);
            if (!existsReference) return NotFound(new GenericResponse(false, $"Referencia inexistent"));

            var exists = _unitOfWork.WorkOrders.Find(w => w.Id == request.Id).Any();
            if (exists) return Conflict(new GenericResponse(false, $"ordre de fabricació existent"));

            // Creació
            await _unitOfWork.WorkOrders.Add(request);
            var location = Url.Action(nameof(GetById), new { id = request.Id }) ?? $"/{request.Id}";
            return Created(location, request);
        }

        [HttpGet]
        public IActionResult GetWorkOrders(DateTime startTime, DateTime endTime, Guid? statusId)
        {
            IEnumerable<WorkOrder> workOrders = new List<WorkOrder>();
            workOrders = _workOrderService.GetBetweenDatesAndStatus(startTime, endTime, statusId);
           
            return Ok(workOrders.OrderBy(e => e.Code));
        }

        [HttpGet("SalesOrder/{id:guid}")]
        public async Task<IActionResult> GetBySalesOrderId(Guid id)
        {
            IEnumerable<WorkOrder> workOrders = await _workOrderService.GetBySalesOrderId(id);
            return Ok(workOrders.OrderBy(e => e.Code));
        }

        [HttpGet("{id:guid}")]
        public async Task<IActionResult> GetById(Guid id)
        {
            var entity = await _unitOfWork.WorkOrders.Get(id);
            if (entity is not null)
                return Ok(entity);
            else
                return NotFound();
        }

        [HttpPut("{id:guid}")]
        public async Task<IActionResult> Update(Guid Id, WorkOrder request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState.ValidationState);
            if (Id != request.Id)
                return BadRequest();

            var exists = await _unitOfWork.WorkOrders.Exists(request.Id);
            if (!exists)
                return NotFound();

            await _unitOfWork.WorkOrders.Update(request);
            return Ok(request);
        }

        [HttpDelete("{id:guid}")]
        public async Task<IActionResult> Delete(Guid id)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState.ValidationState);

            var response = await _workOrderService.Delete(id);            
            if (response.Result)
                return Ok();
            else
                return BadRequest(response);
        }

        #region Phases
        [HttpGet("Phase/{id:guid}")]
        [SwaggerOperation("GetWorkOrderPhaseById")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetWorkOrderPhaseById(Guid id)
        {
            var WorkOrderPhase = await _unitOfWork.WorkOrders.Phases.Get(id);
            if (WorkOrderPhase == null) return NotFound(new GenericResponse(false, $"Fase de la ordre de fabricació inexistent"));

            return Ok(WorkOrderPhase);
        }

        [HttpPost("Phase")]
        [SwaggerOperation("CreateWorkOrderPhase")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> CreatePhase(WorkOrderPhase request)
        {
            // Validacions
            if (!ModelState.IsValid) return BadRequest(ModelState.ValidationState);

            var exists = _unitOfWork.WorkOrders.Phases.Find(w => w.Id == request.Id).Any();
            if (exists) return Conflict(new GenericResponse(false, $"Fase de la ordre de fabricació existent"));

            var WorkOrder = await _unitOfWork.WorkOrders.Get(request.WorkOrderId);
            if (WorkOrder is null) return NotFound(new GenericResponse(false, $"ordre de fabricació inexistent"));

            request.Code = $"{(WorkOrder.Phases.Count() + 1) * 10}";

            // Creació
            await _unitOfWork.WorkOrders.Phases.Add(request);
            return Ok(new GenericResponse(true, request));
        }

        [HttpPut("Phase/{id:guid}")]
        [SwaggerOperation("UpdateWorkOrderPhase")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> UpdatePhase(Guid Id, WorkOrderPhase request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState.ValidationState);
            if (Id != request.Id)
                return BadRequest();

            var exists = await _unitOfWork.WorkOrders.Phases.Exists(request.Id);
            if (!exists)
                return NotFound();

            await _unitOfWork.WorkOrders.Phases.Update(request);
            return Ok(request);
        }

        [HttpDelete("Phase/{id:guid}")]
        [SwaggerOperation("DeleteWorkOrderPhase")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> DeletePhase(Guid id)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState.ValidationState);

            var entity = _unitOfWork.WorkOrders.Phases.Find(e => e.Id == id).FirstOrDefault();
            if (entity is null)
                return NotFound();

            await _unitOfWork.WorkOrders.Phases.Remove(entity);
            return Ok(entity);
        }
        #endregion

        #region PhaseDetails
        [HttpGet("Phase/Detail/{id:guid}")]
        [SwaggerOperation("GetWorkOrderPhaseDetailById")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetWorkOrderPhaseDetailById(Guid id)
        {
            var entities = await _unitOfWork.WorkOrders.Phases.Details.Get(id);
            if (entities == null) return NotFound(new GenericResponse(false, $"Fase de la ordre de fabricació inexistent"));

            return Ok(entities);
        }

        [HttpPost("Phase/Detail")]
        [SwaggerOperation("CreateWorkOrderPhaseDetail")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> CreatePhaseDetail(WorkOrderPhaseDetail request)
        {
            // Validacions
            if (!ModelState.IsValid) return BadRequest(ModelState.ValidationState);

            var exists = _unitOfWork.WorkOrders.Phases.Details.Find(w => w.Id == request.Id).Any();
            if (exists) return Conflict(new GenericResponse(false, $"Pas de la fase de la ordre de fabricació existent"));

            // Creació
            await _unitOfWork.WorkOrders.Phases.Details.Add(request);
            return Ok(new GenericResponse(true, request));
        }

        [HttpPut("Phase/Detail/{id:guid}")]
        [SwaggerOperation("UpdateWorkOrderPhaseDetail")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> UpdatePhaseDetail(Guid Id, WorkOrderPhaseDetail request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState.ValidationState);
            if (Id != request.Id)
                return BadRequest();

            var exists = await _unitOfWork.WorkOrders.Phases.Details.Exists(request.Id);
            if (!exists)
                return NotFound();

            await _unitOfWork.WorkOrders.Phases.Details.Update(request);
            return Ok(request);
        }

        [HttpDelete("Phase/Detail/{id:guid}")]
        [SwaggerOperation("DeleteWorkOrderPhaseDetail")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> DeletePhaseDetail(Guid id)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState.ValidationState);

            var entity = _unitOfWork.WorkOrders.Phases.Details.Find(e => e.Id == id).FirstOrDefault();
            if (entity is null)
                return NotFound();

            await _unitOfWork.WorkOrders.Phases.Details.Remove(entity);
            return Ok(entity);
        }
        #endregion

        #region PhaseBillOfMaterials
        [HttpGet("Phase/BillOfMaterials/{id:guid}")]
        [SwaggerOperation("GetWorkOrderPhaseDetailById")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetWorkOrderPhaseBillOfMaterialsItemById(Guid id)
        {
            var entities = await _unitOfWork.WorkOrders.Phases.BillOfMaterials.Get(id);
            if (entities == null) return NotFound(new GenericResponse(false, $"Fase de la ordre de fabricació inexistent"));

            return Ok(entities);
        }

        [HttpPost("Phase/BillOfMaterials")]
        [SwaggerOperation("CreateWorkOrderPhaseBillOfMaterials")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> CreatePhaseDetail(WorkOrderPhaseBillOfMaterials request)
        {
            // Validacions
            if (!ModelState.IsValid) return BadRequest(ModelState.ValidationState);

            var exists = _unitOfWork.WorkOrders.Phases.BillOfMaterials.Find(w => w.Id == request.Id).Any();
            if (exists) return Conflict(new GenericResponse(false, $"Pas de la fase de la ordre de fabricació existent"));

            // Creació
            await _unitOfWork.WorkOrders.Phases.BillOfMaterials.Add(request);
            return Ok(new GenericResponse(true, request));
        }

        [HttpPut("Phase/BillOfMaterials/{id:guid}")]
        [SwaggerOperation("UpdateWorkOrderPhaseBillOfMaterials")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> UpdatePhaseDetail(Guid Id, WorkOrderPhaseBillOfMaterials request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState.ValidationState);
            if (Id != request.Id)
                return BadRequest();

            var exists = await _unitOfWork.WorkOrders.Phases.BillOfMaterials.Exists(request.Id);
            if (!exists)
                return NotFound();

            await _unitOfWork.WorkOrders.Phases.BillOfMaterials.Update(request);
            return Ok(request);
        }

        [HttpDelete("Phase/BillOfMaterials/{id:guid}")]
        [SwaggerOperation("DeleteWorkOrderPhaseBillOfMaterials")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> DeletePhaseBillOfMaterials(Guid id)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState.ValidationState);

            var entity = _unitOfWork.WorkOrders.Phases.BillOfMaterials.Find(e => e.Id == id).FirstOrDefault();
            if (entity is null)
                return NotFound();

            await _unitOfWork.WorkOrders.Phases.BillOfMaterials.Remove(entity);
            return Ok(entity);
        }
        #endregion

        #region DetailedWorkOrder
        [HttpGet("Detailed/ByWorkcenter/{id:guid}")]        
        public IActionResult GetDetailedWorkOrderByWorkcenterId(Guid id)
        {
            IEnumerable<DetailedWorkOrder> entities = Enumerable.Empty<DetailedWorkOrder>();
            entities = _unitOfWork.DetailedWorkOrders.Find(d => d.WorkcenterId == id);
            return Ok(entities);
        }

        [HttpGet("Detailed/ByWorkOrder/{id:guid}")]
        public IActionResult GetDetailedWorkOrderByWorkOrderId(Guid id)
        {
            IEnumerable<DetailedWorkOrder> entities = Enumerable.Empty<DetailedWorkOrder>();
            entities = _unitOfWork.DetailedWorkOrders.Find(d => d.WorkOrderId == id);
            return Ok(entities);
        }

        [HttpGet("Detailed/ByWorkOrderPhase/{id:guid}")]
        public IActionResult GetDetailedWorkOrderByWorkOrderPhaseId(Guid id)
        {
            IEnumerable<DetailedWorkOrder> entities = Enumerable.Empty<DetailedWorkOrder>();
            entities = _unitOfWork.DetailedWorkOrders.Find(d => d.WorkOrderPhaseId == id);
            return Ok(entities);
        }
        #endregion
    }
}
