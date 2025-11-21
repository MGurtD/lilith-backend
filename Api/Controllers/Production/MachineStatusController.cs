using Application.Contracts;
using Application.Persistance;
using Application.Services;
using Domain.Entities.Production;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;

namespace Api.Controllers.Production
{
    [ApiController]
    [Route("api/[controller]")]
    public class MachineStatusController(IUnitOfWork unitOfWork, ILocalizationService localizationService) : ControllerBase
    {
        [HttpPost]
        public async Task<IActionResult> Create(MachineStatus request)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState.ValidationState);

            var exists = unitOfWork.MachineStatuses.Find(r => request.Name == r.Name).Any();
            if (!exists)
            {
                if (request.Default)
                    DisableCurrentDefault(request.Id);

                await unitOfWork.MachineStatuses.Add(request);
                var location = Url.Action(nameof(GetById), new { id = request.Id }) ?? $"/{request.Id}";
                return Created(location, request);
            }
            else
            {
                return Conflict(new GenericResponse(false, localizationService.GetLocalizedString("MachineStatusAlreadyExists", request.Name)));
            }
        }
        [HttpGet]
        public async Task<IActionResult> GetAll([FromQuery] bool includeReasons = false)
        {
            IEnumerable<MachineStatus> entities;
            
            if (includeReasons)
            {
                entities = await unitOfWork.MachineStatuses.GetAllWithReasons();
            }
            else
            {
                entities = await unitOfWork.MachineStatuses.GetAll();
            }

            return Ok(entities.OrderBy(w => w.Name));
        }

        [HttpGet("{id:guid}")]
        public async Task<IActionResult> GetById(Guid id)
        {
            var entity = await unitOfWork.MachineStatuses.Get(id);
            if (entity is not null)
            {
                return Ok(entity);
            }
            else
            {
                return NotFound();
            }
        }

        private void DisableCurrentDefault(Guid currentDefaultId)
        {
            var currentDefault = unitOfWork.MachineStatuses.Find(e => e.Id != currentDefaultId && e.Default).FirstOrDefault();
            if (currentDefault is not null)
            {
                currentDefault.Default = false;
                unitOfWork.MachineStatuses.UpdateWithoutSave(currentDefault);
            }
        }

        [HttpPut("{id:guid}")]
        public async Task<IActionResult> Update(Guid Id, MachineStatus request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState.ValidationState);
            if (Id != request.Id)
                return BadRequest();

            var exists = await unitOfWork.MachineStatuses.Exists(request.Id);
            if (!exists)
                return NotFound();

            if (request.Default)
                DisableCurrentDefault(Id);

            unitOfWork.MachineStatuses.UpdateWithoutSave(request);
            await unitOfWork.CompleteAsync();
            return Ok(request);
        }

        [HttpDelete("{id:guid}")]
        public async Task<IActionResult> Delete(Guid id)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState.ValidationState);

            var entity = unitOfWork.MachineStatuses.Find(e => e.Id == id).FirstOrDefault();
            if (entity is null)
                return NotFound();

            await unitOfWork.MachineStatuses.Remove(entity);
            return Ok(entity);
        }

        #region Reasons
        [HttpGet("Reason/{id:guid}")]
        [SwaggerOperation("GetMachineStatusReasonById")]
        public async Task<IActionResult> GetReasonById(Guid id)
        {
            var entity = await unitOfWork.MachineStatuses.Reasons.Get(id);
            if (entity is not null)
            {
                return Ok(entity);
            }
            else
            {
                return NotFound(new GenericResponse(false, localizationService.GetLocalizedString("MachineStatusReasonNotFound", id)));
            }
        }

        [HttpPost("Reason")]
        [SwaggerOperation("CreateMachineStatusReason")]
        public async Task<IActionResult> CreateReason(MachineStatusReason request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState.ValidationState);

            var codeExists = unitOfWork.MachineStatuses.Reasons.Find(r => 
                r.Code == request.Code && 
                r.MachineStatusId == request.MachineStatusId && 
                r.Id != request.Id
            ).Any();
            
            if (codeExists)
                return Conflict(new GenericResponse(false, localizationService.GetLocalizedString("MachineStatusReasonCodeDuplicate", request.Code)));

            await unitOfWork.MachineStatuses.Reasons.Add(request);
            return Ok(new GenericResponse(true, request));
        }

        [HttpPut("Reason/{id:guid}")]
        [SwaggerOperation("UpdateMachineStatusReason")]
        public async Task<IActionResult> UpdateReason(Guid id, MachineStatusReason request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState.ValidationState);
            if (id != request.Id)
                return BadRequest();

            var exists = await unitOfWork.MachineStatuses.Reasons.Exists(request.Id);
            if (!exists)
                return NotFound(new GenericResponse(false, localizationService.GetLocalizedString("MachineStatusReasonNotFound", id)));

            var codeExists = unitOfWork.MachineStatuses.Reasons.Find(r => 
                r.Code == request.Code && 
                r.MachineStatusId == request.MachineStatusId && 
                r.Id != request.Id
            ).Any();
            
            if (codeExists)
                return Conflict(new GenericResponse(false, localizationService.GetLocalizedString("MachineStatusReasonCodeDuplicate", request.Code)));

            unitOfWork.MachineStatuses.Reasons.UpdateWithoutSave(request);
            await unitOfWork.CompleteAsync();
            return Ok(new GenericResponse(true, request));
        }

        [HttpDelete("Reason/{id:guid}")]
        [SwaggerOperation("DeleteMachineStatusReason")]
        public async Task<IActionResult> DeleteReason(Guid id)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState.ValidationState);

            var entity = unitOfWork.MachineStatuses.Reasons.Find(e => e.Id == id).FirstOrDefault();
            if (entity is null)
                return NotFound(new GenericResponse(false, localizationService.GetLocalizedString("MachineStatusReasonNotFound", id)));

            await unitOfWork.MachineStatuses.Reasons.Remove(entity);
            return Ok(new GenericResponse(true, entity));
        }
        #endregion
    }
}
