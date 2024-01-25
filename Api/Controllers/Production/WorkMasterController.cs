using Application.Contracts;
using Application.Persistance;
using Domain.Entities.Production;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using System.Net;

namespace Api.Controllers.Production
{
    [ApiController]
    [Route("api/[controller]")]
    public class WorkMasterController : ControllerBase
    {
        private readonly IUnitOfWork _unitOfWork;

        public WorkMasterController(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        [HttpPost]
        public async Task<IActionResult> Create(WorkMaster request)
        {
            // Validacions
            if (!ModelState.IsValid) return BadRequest(ModelState.ValidationState);

            var existsReference = await _unitOfWork.References.Exists(request.ReferenceId);
            if (!existsReference) return NotFound(new GenericResponse(false, $"Referencia inexistent"));

            var exists = _unitOfWork.WorkMasters.Find(w => w.Id == request.Id).Any();
            if (exists) return Conflict(new GenericResponse(false, $"Ruta de fabricació existent"));

            // Creació
            await _unitOfWork.WorkMasters.Add(request);
            var location = Url.Action(nameof(GetById), new { id = request.Id }) ?? $"/{request.Id}";
            return Created(location, request);
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var entities = await _unitOfWork.WorkMasters.GetAll();

            return Ok(entities.OrderBy(w => w.ReferenceId));
        }

        [HttpGet("{id:guid}")]
        public async Task<IActionResult> GetById(Guid id)
        {
            var entity = await _unitOfWork.WorkMasters.Get(id);
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

            var exists = await _unitOfWork.WorkMasters.Exists(request.Id);
            if (!exists)
                return NotFound();

            await _unitOfWork.WorkMasters.Update(request);
            return Ok(request);
        }

        [HttpDelete("{id:guid}")]
        public async Task<IActionResult> Delete(Guid id)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState.ValidationState);

            var entity = _unitOfWork.WorkMasters.Find(e => e.Id == id).FirstOrDefault();
            if (entity is null)
                return NotFound();

            await _unitOfWork.WorkMasters.Remove(entity);
            return Ok(entity);
        }

        [HttpPost("Copy")]
        public async Task<IActionResult> Copy(WorkMasterCopy request)
        {
            // Validacions
            if (request.ReferenceId.HasValue && request.ReferenceId != Guid.Empty)
            {
                var exists = _unitOfWork.WorkMasters.Find(w => w.ReferenceId == request.ReferenceId).Any();
                if (exists) return Conflict(new GenericResponse(false, $"Referencia de destí amb ruta de fabricació existent"));
            }
            

            // Creació            
            var result = await _unitOfWork.WorkMasters.Copy(request);            
            if (result) {
                return Ok();
            }
            else
            {
                return NotFound();
            };
            
        }

        #region Phases
        [HttpGet("Phase/{id:guid}")]
        [SwaggerOperation("GetWorkMasterPhaseById")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetWorkMasterPhaseById(Guid id)
        {
            var workmasterPhase = await _unitOfWork.WorkMasters.Phases.Get(id);
            if (workmasterPhase == null) return NotFound(new GenericResponse(false, $"Fase de la ruta de fabricació inexistent"));

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

            var exists = _unitOfWork.WorkMasters.Phases.Find(w => w.Id == request.Id).Any();
            if (exists) return Conflict(new GenericResponse(false, $"Fase de la ruta de fabricació existent"));

            var workmaster = await _unitOfWork.WorkMasters.Get(request.WorkMasterId);
            if (workmaster is null) return NotFound(new GenericResponse(false, $"Ruta de fabricació inexistent"));

            request.Code = $"{(workmaster.Phases.Count() + 1) * 10}";

            // Creació
            await _unitOfWork.WorkMasters.Phases.Add(request);
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

            var exists = await _unitOfWork.WorkMasters.Phases.Exists(request.Id);
            if (!exists)
                return NotFound();

            await _unitOfWork.WorkMasters.Phases.Update(request);
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

            var entity = _unitOfWork.WorkMasters.Phases.Find(e => e.Id == id).FirstOrDefault();
            if (entity is null)
                return NotFound();

            await _unitOfWork.WorkMasters.Phases.Remove(entity);
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
            var entities = await _unitOfWork.WorkMasters.Phases.Details.Get(id);
            if (entities == null) return NotFound(new GenericResponse(false, $"Fase de la ruta de fabricació inexistent"));

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

            var exists = _unitOfWork.WorkMasters.Phases.Details.Find(w => w.Id == request.Id).Any();
            if (exists) return Conflict(new GenericResponse(false, $"Pas de la fase de la ruta de fabricació existent"));

            // Creació
            await _unitOfWork.WorkMasters.Phases.Details.Add(request);
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

            var exists = await _unitOfWork.WorkMasters.Phases.Details.Exists(request.Id);
            if (!exists)
                return NotFound();

            await _unitOfWork.WorkMasters.Phases.Details.Update(request);
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

            var entity = _unitOfWork.WorkMasters.Phases.Details.Find(e => e.Id == id).FirstOrDefault();
            if (entity is null)
                return NotFound();

            await _unitOfWork.WorkMasters.Phases.Details.Remove(entity);
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
            var entities = await _unitOfWork.WorkMasters.Phases.BillOfMaterials.Get(id);
            if (entities == null) return NotFound(new GenericResponse(false, $"Fase de la ruta de fabricació inexistent"));

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

            var exists = _unitOfWork.WorkMasters.Phases.BillOfMaterials.Find(w => w.Id == request.Id).Any();
            if (exists) return Conflict(new GenericResponse(false, $"Pas de la fase de la ruta de fabricació existent"));

            // Creació
            await _unitOfWork.WorkMasters.Phases.BillOfMaterials.Add(request);
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

            var exists = await _unitOfWork.WorkMasters.Phases.BillOfMaterials.Exists(request.Id);
            if (!exists)
                return NotFound();

            await _unitOfWork.WorkMasters.Phases.BillOfMaterials.Update(request);
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

            var entity = _unitOfWork.WorkMasters.Phases.BillOfMaterials.Find(e => e.Id == id).FirstOrDefault();
            if (entity is null)
                return NotFound();

            await _unitOfWork.WorkMasters.Phases.BillOfMaterials.Remove(entity);
            return Ok(entity);
        }
        #endregion

    }
}
