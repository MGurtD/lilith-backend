using Application.Contracts;
using Domain.Entities.Production;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;

namespace Api.Controllers.Production
{
    [ApiController]
    [Route("api/[controller]")]
    public class WorkMasterController(IUnitOfWork unitOfWork, IMetricsService costsService, ILocalizationService localizationService) : ControllerBase
    {
        [HttpPost]
        public async Task<IActionResult> Create(WorkMaster request)
        {
            // Validacions
            if (!ModelState.IsValid) return BadRequest(ModelState.ValidationState);

            var existsReference = await unitOfWork.References.Exists(request.ReferenceId);
            if (!existsReference) return NotFound(new GenericResponse(false, localizationService.GetLocalizedString("ReferenceNotFound")));

            var exists = unitOfWork.WorkMasters.Find(w => w.Id == request.Id).Any();
            if (exists) return Conflict(new GenericResponse(false, localizationService.GetLocalizedString("WorkMasterAlreadyExists")));

            // Creació
            await unitOfWork.WorkMasters.Add(request);
            var location = Url.Action(nameof(GetById), new { id = request.Id }) ?? $"/{request.Id}";
            return Created(location, request);
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var entities = await unitOfWork.WorkMasters.GetAll();

            return Ok(entities.OrderBy(w => w.ReferenceId));
        }

        [HttpGet("{id:guid}")]
        public async Task<IActionResult> GetById(Guid id)
        {
            var entity = await unitOfWork.WorkMasters.Get(id);
            if (entity is not null)
                return Ok(entity);
            else
                return NotFound();
        }

        [HttpPut("{id:guid}")]
        public async Task<IActionResult> Update(Guid Id, WorkMaster request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState.ValidationState);
            if (Id != request.Id)
                return BadRequest();

            var exists = await unitOfWork.WorkMasters.Exists(request.Id);
            if (!exists)
                return NotFound();

            var resultCosts = await costsService.GetWorkmasterMetrics(request, request.BaseQuantity);
            if (resultCosts.Result && resultCosts.Content is ProductionMetrics workMasterMetrics)
            {
                request.operatorCost = workMasterMetrics.OperatorCost;
                request.machineCost = workMasterMetrics.MachineCost;
                request.externalCost = workMasterMetrics.ExternalServiceCost + workMasterMetrics.ExternalTransportCost;
                request.materialCost = workMasterMetrics.MaterialCost;
                request.totalWeight = workMasterMetrics.TotalWeight;
            }

            await unitOfWork.WorkMasters.Update(request);
            //get reference and update workmastercost

            var reference = await unitOfWork.References.Get(request.ReferenceId);
            if (reference != null)
            {
                reference.WorkMasterCost = request.operatorCost + request.machineCost + request.externalCost + request.materialCost;
                await unitOfWork.References.Update(reference);
            }
            return Ok(request);
        }

        [HttpDelete("{id:guid}")]
        public async Task<IActionResult> Delete(Guid id)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState.ValidationState);

            var entity = unitOfWork.WorkMasters.Find(e => e.Id == id).FirstOrDefault();
            if (entity is null)
                return NotFound();

            await unitOfWork.WorkMasters.Remove(entity);
            return Ok(entity);
        }

        [HttpPost("Copy")]
        public async Task<IActionResult> Copy(WorkMasterCopy request)
        {
            // Validacions
            if (request.ReferenceId.HasValue && request.ReferenceId != Guid.Empty)
            {
                var exists = unitOfWork.WorkMasters.Find(w => w.ReferenceId == request.ReferenceId && w.Mode == request.Mode).Any();
                if (exists) return Conflict(new GenericResponse(false, localizationService.GetLocalizedString("ReferenceAlreadyExists")));
            }
            

            // Creació            
            var result = await unitOfWork.WorkMasters.Copy(request);            
            if (result) {
                return Ok();
            }
            else
            {
                return NotFound();
            };
            
        }

        [HttpGet("reference/{id:guid}")]
        public IActionResult GetWorkMasterByReferenceId(Guid id)
        {
            var entities = unitOfWork.WorkMasters.Find(w => w.ReferenceId == id && w.Disabled == false);
            return Ok(entities.OrderBy(w => w.BaseQuantity));
        }

        [HttpGet("Cost/{id:guid}")]
        public async Task<IActionResult> GetWorkMasterCostById(Guid id, int? quantity)
        {
            var workMaster = await unitOfWork.WorkMasters.Get(id);
            if (workMaster == null) return NotFound();

            var result = await costsService.GetWorkmasterMetrics(workMaster, quantity);

            if (result.Result && result.Content is ProductionMetrics workMasterCosts)
                return Ok(new GenericResponse(true, workMasterCosts.TotalCost()));
            else
                return NotFound(result);

        }
        [HttpGet("Costs/{id:guid}")]
        public async Task<IActionResult> GetWorkMasterCostsById(Guid id, int? quantity)
        {
            var workMaster = await unitOfWork.WorkMasters.Get(id);
            if (workMaster == null) return NotFound();

            var result = await costsService.GetWorkmasterMetrics(workMaster, quantity);

            if (result.Result && result.Content is ProductionMetrics workMasterCosts)
                return Ok(new GenericResponse(true, workMasterCosts));
            else
                return NotFound(result);

        }

        #region Phases
        [HttpGet("Phase/{id:guid}")]
        [SwaggerOperation("GetWorkMasterPhaseById")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetWorkMasterPhaseById(Guid id)
        {
            var workmasterPhase = await unitOfWork.WorkMasters.Phases.Get(id);
            if (workmasterPhase == null) return NotFound(new GenericResponse(false, localizationService.GetLocalizedString("WorkMasterPhaseNotFound")));

            return Ok(workmasterPhase);
        }

        [HttpPost("Phase")]
        [SwaggerOperation("CreateWorkMasterPhase")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> CreatePhase(WorkMasterPhase request)
        {
            // Validacions
            if (!ModelState.IsValid) return BadRequest(ModelState.ValidationState);

            var exists = unitOfWork.WorkMasters.Phases.Find(w => w.Id == request.Id).Any();
            if (exists) return Conflict(new GenericResponse(false, localizationService.GetLocalizedString("WorkMasterPhaseAlreadyExists")));

            var workmaster = await unitOfWork.WorkMasters.Get(request.WorkMasterId);
            if (workmaster is null) return NotFound(new GenericResponse(false, localizationService.GetLocalizedString("WorkMasterNotFound")));

            // Creació
            await unitOfWork.WorkMasters.Phases.Add(request);
            return Ok(new GenericResponse(true, request));
        }

        [HttpPut("Phase/{id:guid}")]
        [SwaggerOperation("UpdateWorkMasterPhase")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> UpdatePhase(Guid Id, WorkMasterPhase request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState.ValidationState);
            if (Id != request.Id)
                return BadRequest();

            var exists = await unitOfWork.WorkMasters.Phases.Exists(request.Id);
            if (!exists)
                return NotFound();

            await unitOfWork.WorkMasters.Phases.Update(request);
            return Ok(request);
        }

        [HttpDelete("Phase/{id:guid}")]
        [SwaggerOperation("DeleteWorkMasterPhase")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> DeletePhase(Guid id)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState.ValidationState);

            var entity = unitOfWork.WorkMasters.Phases.Find(e => e.Id == id).FirstOrDefault();
            if (entity is null)
                return NotFound();

            await unitOfWork.WorkMasters.Phases.Remove(entity);
            return Ok(entity);
        }
        #endregion

        #region PhaseDetails
        [HttpGet("Phase/Detail/{id:guid}")]
        [SwaggerOperation("GetWorkMasterPhaseDetailById")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetWorkMasterPhaseDetailById(Guid id)
        {
            var entities = await unitOfWork.WorkMasters.Phases.Details.Get(id);
            if (entities == null) return NotFound(new GenericResponse(false, localizationService.GetLocalizedString("WorkMasterPhaseDetailNotFound")));

            return Ok(entities);
        }

        [HttpPost("Phase/Detail")]
        [SwaggerOperation("CreateWorkMasterPhaseDetail")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> CreatePhaseDetail(WorkMasterPhaseDetail request)
        {
            // Validacions
            if (!ModelState.IsValid) return BadRequest(ModelState.ValidationState);

            var exists = unitOfWork.WorkMasters.Phases.Details.Find(w => w.Id == request.Id).Any();
            if (exists) return Conflict(new GenericResponse(false, localizationService.GetLocalizedString("WorkMasterPhaseDetailAlreadyExists")));

            // Creació
            await unitOfWork.WorkMasters.Phases.Details.Add(request);
            return Ok(new GenericResponse(true, request));
        }

        [HttpPut("Phase/Detail/{id:guid}")]
        [SwaggerOperation("UpdateWorkMasterPhaseDetail")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> UpdatePhaseDetail(Guid Id, WorkMasterPhaseDetail request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState.ValidationState);
            if (Id != request.Id)
                return BadRequest();

            var exists = await unitOfWork.WorkMasters.Phases.Details.Exists(request.Id);
            if (!exists)
                return NotFound();

            await unitOfWork.WorkMasters.Phases.Details.Update(request);
            return Ok(request);
        }

        [HttpDelete("Phase/Detail/{id:guid}")]
        [SwaggerOperation("DeleteWorkMasterPhaseDetail")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> DeletePhaseDetail(Guid id)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState.ValidationState);

            var entity = unitOfWork.WorkMasters.Phases.Details.Find(e => e.Id == id).FirstOrDefault();
            if (entity is null)
                return NotFound();

            await unitOfWork.WorkMasters.Phases.Details.Remove(entity);
            return Ok(entity);
        }
        #endregion

        #region PhaseBillOfMaterials
        [HttpGet("Phase/BillOfMaterials/{id:guid}")]
        [SwaggerOperation("GetWorkMasterPhaseDetailById")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetWorkMasterPhaseBillOfMaterialsItemById(Guid id)
        {
            var entities = await unitOfWork.WorkMasters.Phases.BillOfMaterials.Get(id);
            if (entities == null) return NotFound(new GenericResponse(false, localizationService.GetLocalizedString("WorkMasterPhaseDetailNotFound")));

            return Ok(entities);
        }

        [HttpPost("Phase/BillOfMaterials")]
        [SwaggerOperation("CreateWorkMasterPhaseBillOfMaterials")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> CreatePhaseDetail(WorkMasterPhaseBillOfMaterials request)
        {
            // Validacions
            if (!ModelState.IsValid) return BadRequest(ModelState.ValidationState);

            var exists = unitOfWork.WorkMasters.Phases.BillOfMaterials.Find(w => w.Id == request.Id).Any();
            if (exists) return Conflict(new GenericResponse(false, localizationService.GetLocalizedString("WorkMasterPhaseDetailAlreadyExists")));

            // Creació
            await unitOfWork.WorkMasters.Phases.BillOfMaterials.Add(request);
            return Ok(new GenericResponse(true, request));
        }

        [HttpPut("Phase/BillOfMaterials/{id:guid}")]
        [SwaggerOperation("UpdateWorkMasterPhaseBillOfMaterials")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> UpdatePhaseDetail(Guid Id, WorkMasterPhaseBillOfMaterials request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState.ValidationState);
            if (Id != request.Id)
                return BadRequest();

            var exists = await unitOfWork.WorkMasters.Phases.BillOfMaterials.Exists(request.Id);
            if (!exists)
                return NotFound();

            await unitOfWork.WorkMasters.Phases.BillOfMaterials.Update(request);
            return Ok(request);
        }

        [HttpDelete("Phase/BillOfMaterials/{id:guid}")]
        [SwaggerOperation("DeleteWorkMasterPhaseBillOfMaterials")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> DeletePhaseBillOfMaterials(Guid id)
        {
            if (!ModelState.IsValid)

                return BadRequest(ModelState.ValidationState);

            var entity = unitOfWork.WorkMasters.Phases.BillOfMaterials.Find(e => e.Id == id).FirstOrDefault();
            if (entity is null)
                return NotFound();

            await unitOfWork.WorkMasters.Phases.BillOfMaterials.Remove(entity);
            return Ok(entity);
        }
        #endregion

    }
}
