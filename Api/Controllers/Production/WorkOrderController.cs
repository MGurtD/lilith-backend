using Application.Contracts;
using Application.Contracts.Production;
using Application.Persistance;
using Application.Production;
using Application.Services;
using Domain.Entities.Production;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using Api.Constants;

namespace Api.Controllers.Production
{
    [ApiController]
    [Route("api/[controller]")]
    public class WorkOrderController(IUnitOfWork unitOfWork, IWorkOrderService workOrderService, ILocalizationService localizationService) : ControllerBase
    {
        [HttpPost("CreateFromWorkMaster")]
        public async Task<IActionResult> CreateFromWorkMaster([FromBody] CreateWorkOrderDto request)
        {
            var creationResult = await workOrderService.CreateFromWorkMaster(request);
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

            var existsReference = await unitOfWork.References.Exists(request.ReferenceId);
            if (!existsReference) return NotFound(new GenericResponse(false, localizationService.GetLocalizedString("ReferenceNotFound")));

            var exists = unitOfWork.WorkOrders.Find(w => w.Id == request.Id).Any();
            if (exists) return Conflict(new GenericResponse(false, localizationService.GetLocalizedString("WorkOrderAlreadyExists")));

            // Creació
            await unitOfWork.WorkOrders.Add(request);
            var location = Url.Action(nameof(GetById), new { id = request.Id }) ?? $"/{request.Id}";
            return Created(location, request);
        }

        [HttpGet]
        public IActionResult GetWorkOrders(DateTime startTime, DateTime endTime, Guid? statusId, Guid? referenceId, Guid? customerId)
        {
            IEnumerable<WorkOrder> workOrders = [];
            workOrders = workOrderService.GetBetweenDatesAndStatus(startTime, endTime, statusId);

            if (referenceId.HasValue)
                workOrders = workOrders.Where(e => e.ReferenceId == referenceId.Value);

            if (customerId.HasValue)
                workOrders = workOrders.Where(e => e.Reference!.CustomerId == customerId.Value);

            return Ok(workOrders.OrderBy(e => e.Code));
        }

        [HttpGet("SalesOrder/{id:guid}")]
        public async Task<IActionResult> GetBySalesOrderId(Guid id)
        {
            IEnumerable<WorkOrder> workOrders = await workOrderService.GetBySalesOrderId(id);
            return Ok(workOrders.OrderBy(e => e.Code));
        }

        [HttpGet("Workcenter/{workcenterId:guid}/Production")]
        [SwaggerOperation("GetByWorkcenterIdInProduction")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetByWorkcenterIdInProduction(Guid workcenterId)
        {
            var response = await workOrderService.GetByWorkcenterIdInProduction(workcenterId);
            
            if (!response.Result)
                return NotFound(response);
            
            return Ok(response.Content);
        }

        [HttpGet("WorkcenterType/{id:guid}")]
        [SwaggerOperation("GetWorkordersByWorkcenterTypeId")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetWorkordersByWorkcenterTypeId(Guid id)
        {
            var workorders = await workOrderService.GetWorkordersByWorkcenterTypeId(id);
            if (workorders.Any())
                return Ok(workorders);
            else
                return NotFound();
        }
        [HttpPut("UpdateOrders")]
        public async Task<IActionResult> UpdateOrders(List<UpdateWorkOrderOrderDTO> orders)
        {
            var response = await workOrderService.UpdateOrders(orders);
            if (response)
                return Ok();
            else
                return BadRequest();
        }

        [HttpGet("{id:guid}/Detailed")]
        public IActionResult GetDetailedWorkOrder(Guid id)
        {
            var details = workOrderService.GetWorkOrderDetails(id);
            return Ok(details);
        }

        [HttpGet("{id:guid}")]
        public async Task<IActionResult> GetById(Guid id)
        {
            var entity = await unitOfWork.WorkOrders.Get(id);
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

            var exists = await unitOfWork.WorkOrders.Exists(request.Id);
            if (!exists)
                return NotFound();

            await workOrderService.Update(request);
            return Ok(request);
        }

        [HttpDelete("{id:guid}")]
        public async Task<IActionResult> Delete(Guid id)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState.ValidationState);

            var response = await workOrderService.Delete(id);
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
            var WorkOrderPhase = await unitOfWork.WorkOrders.Phases.Get(id);
            if (WorkOrderPhase == null) return NotFound(new GenericResponse(false, localizationService.GetLocalizedString("WorkOrderPhaseNotFound")));

            return Ok(WorkOrderPhase);
        }        

        [HttpGet("Phase/External")]
        [SwaggerOperation("GetExternalWorkOrderPhase")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetExternalWorkOrderPhase(DateTime startTime, DateTime endTime)
        {
            var status = await unitOfWork.Lifecycles.GetStatusByName(StatusConstants.Lifecycles.WorkOrder, StatusConstants.Statuses.Production);
            if (status == null)
            {
                return NotFound(new GenericResponse(false, localizationService.GetLocalizedString("WorkOrderStatusNotFound")));
            }

            // Asíncron per trobar les ordres de treball
            var workOrders = await unitOfWork.WorkOrders.FindAsync(w =>
                                w.StatusId == status.Id &&
                                w.PlannedDate >= startTime &&
                                w.PlannedDate < endTime);

            // Asíncron per trobar les fases externes
            var workOrderPhases = await unitOfWork.WorkOrders.Phases.FindAsync(p =>
                                p.IsExternalWork == true &&
                                p.ServiceReferenceId != null &&
                                p.PurchaseOrderId == null);

            // Uneix les fases amb les ordres de treball
            var workOrderPhaseJoin = from w in workOrders
                                     join p in workOrderPhases
                                     on w.Id equals p.WorkOrderId
                                     select new
                                     {
                                         WorkOrder = w,
                                         Phase = p
                                     };

            if (workOrderPhaseJoin == null || !workOrderPhaseJoin.Any())
                return NotFound(new GenericResponse(false, localizationService.GetLocalizedString("WorkOrderPhaseNotFound")));

            return Ok(workOrderPhaseJoin);
        }


        [HttpPost("Phase")]
        [SwaggerOperation("CreateWorkOrderPhase")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> CreatePhase(WorkOrderPhase request)
        {
            // Validacions
            if (!ModelState.IsValid) return BadRequest(ModelState.ValidationState);

            var exists = unitOfWork.WorkOrders.Phases.Find(w => w.Id == request.Id).Any();
            if (exists) return Conflict(new GenericResponse(false, localizationService.GetLocalizedString("WorkOrderPhaseAlreadyExists")));

            var WorkOrder = await unitOfWork.WorkOrders.Get(request.WorkOrderId);
            if (WorkOrder is null) return NotFound(new GenericResponse(false, localizationService.GetLocalizedString("WorkOrderNotFound", request.WorkOrderId)));

            // Creació
            await unitOfWork.WorkOrders.Phases.Add(request);
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

            var exists = await unitOfWork.WorkOrders.Phases.Exists(request.Id);
            if (!exists)
                return NotFound();

            await unitOfWork.WorkOrders.Phases.Update(request);
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

            var entity = unitOfWork.WorkOrders.Phases.Find(e => e.Id == id).FirstOrDefault();
            if (entity is null)
                return NotFound();

            await unitOfWork.WorkOrders.Phases.Remove(entity);
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
            var entities = await unitOfWork.WorkOrders.Phases.Details.Get(id);
            if (entities == null) return NotFound(new GenericResponse(false, localizationService.GetLocalizedString("WorkOrderPhaseDetailNotFound")));

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

            var exists = unitOfWork.WorkOrders.Phases.Details.Find(w => w.Id == request.Id).Any();
            if (exists) return Conflict(new GenericResponse(false, localizationService.GetLocalizedString("WorkOrderPhaseDetailAlreadyExists")));

            // Creació
            await unitOfWork.WorkOrders.Phases.Details.Add(request);
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

            var exists = await unitOfWork.WorkOrders.Phases.Details.Exists(request.Id);
            if (!exists)
                return NotFound();

            await unitOfWork.WorkOrders.Phases.Details.Update(request);
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

            var entity = unitOfWork.WorkOrders.Phases.Details.Find(e => e.Id == id).FirstOrDefault();
            if (entity is null)
                return NotFound();

            await unitOfWork.WorkOrders.Phases.Details.Remove(entity);
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
            var entities = await unitOfWork.WorkOrders.Phases.BillOfMaterials.Get(id);
            if (entities == null) return NotFound(new GenericResponse(false, localizationService.GetLocalizedString("WorkOrderPhaseDetailNotFound")));

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

            var exists = unitOfWork.WorkOrders.Phases.BillOfMaterials.Find(w => w.Id == request.Id).Any();
            if (exists) return Conflict(new GenericResponse(false, localizationService.GetLocalizedString("WorkOrderPhaseDetailAlreadyExists")));

            // Creació
            await unitOfWork.WorkOrders.Phases.BillOfMaterials.Add(request);
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

            var exists = await unitOfWork.WorkOrders.Phases.BillOfMaterials.Exists(request.Id);
            if (!exists)
                return NotFound();

            await unitOfWork.WorkOrders.Phases.BillOfMaterials.Update(request);
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

            var entity = unitOfWork.WorkOrders.Phases.BillOfMaterials.Find(e => e.Id == id).FirstOrDefault();
            if (entity is null)
                return NotFound();

            await unitOfWork.WorkOrders.Phases.BillOfMaterials.Remove(entity);
            return Ok(entity);
        }
        #endregion

        #region DetailedWorkOrder
        [HttpGet("Detailed/ByWorkcenter/{id:guid}")]
        public IActionResult GetDetailedWorkOrderByWorkcenterId(Guid id)
        {
            IEnumerable<DetailedWorkOrder> entities = Enumerable.Empty<DetailedWorkOrder>();
            entities = unitOfWork.DetailedWorkOrders.Find(d => d.WorkcenterId == id);
            return Ok(entities);
        }

        [HttpGet("Detailed/ByWorkOrder/{id:guid}")]
        public IActionResult GetDetailedWorkOrderByWorkOrderId(Guid id)
        {
            IEnumerable<DetailedWorkOrder> entities = Enumerable.Empty<DetailedWorkOrder>();
            entities = unitOfWork.DetailedWorkOrders.Find(d => d.WorkOrderId == id);
            return Ok(entities);
        }

        [HttpGet("Detailed/ByWorkOrderPhase/{id:guid}")]
        public IActionResult GetDetailedWorkOrderByWorkOrderPhaseId(Guid id)
        {
            IEnumerable<DetailedWorkOrder> entities = Enumerable.Empty<DetailedWorkOrder>();
            entities = unitOfWork.DetailedWorkOrders.Find(d => d.WorkOrderPhaseId == id);
            return Ok(entities);
        }
        #endregion
    }
}
