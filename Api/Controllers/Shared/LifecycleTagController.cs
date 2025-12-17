using Application.Contracts;
using Application.Persistance;
using Application.Services;
using Domain.Entities.Shared;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers.Shared
{
    [ApiController]
    [Route("api/[controller]")]
    public class LifecycleTagController(IUnitOfWork unitOfWork, ILocalizationService localizationService) : ControllerBase
    {
        [HttpGet("Lifecycle/{lifecycleId:guid}")]
        public async Task<IActionResult> GetByLifecycleId(Guid lifecycleId)
        {
            var lifecycle = await unitOfWork.Lifecycles.Get(lifecycleId);
            if (lifecycle is null)
            {
                var message = localizationService.GetLocalizedString("LifecycleNotFound", lifecycleId);
                return NotFound(new GenericResponse(false, message));
            }

            var tags = await unitOfWork.LifecycleTags.GetByLifecycleId(lifecycleId);
            return Ok(tags);
        }

        [HttpGet("{id:guid}")]
        public async Task<IActionResult> GetById(Guid id)
        {
            var tag = await unitOfWork.LifecycleTags.Get(id);
            if (tag is null)
            {
                var message = localizationService.GetLocalizedString("LifecycleTagNotFound", id);
                return NotFound(new GenericResponse(false, message));
            }

            return Ok(tag);
        }

        [HttpPost("Lifecycle/{lifecycleId:guid}")]
        public async Task<IActionResult> Create(Guid lifecycleId, LifecycleTag request)
        {
            if (!ModelState.IsValid) 
                return BadRequest(ModelState.ValidationState);

            // Validate lifecycle exists
            var lifecycle = await unitOfWork.Lifecycles.Get(lifecycleId);
            if (lifecycle is null)
            {
                var message = localizationService.GetLocalizedString("LifecycleNotFound", lifecycleId);
                return NotFound(new GenericResponse(false, message));
            }

            // Validate name is provided
            if (string.IsNullOrWhiteSpace(request.Name))
            {
                var message = localizationService.GetLocalizedString("LifecycleTagNameRequired");
                return BadRequest(new GenericResponse(false, message));
            }

            // Check if name already exists in this lifecycle
            var exists = await unitOfWork.LifecycleTags.ExistsInLifecycle(request.Name, lifecycleId);
            if (exists)
            {
                var message = localizationService.GetLocalizedString("LifecycleTagAlreadyExists", request.Name);
                return Conflict(new GenericResponse(false, message));
            }

            // Set lifecycle ID from route parameter
            request.LifecycleId = lifecycleId;
            
            await unitOfWork.LifecycleTags.Add(request);

            var location = Url.Action(nameof(GetById), new { id = request.Id }) ?? $"/{request.Id}";
            return Created(location, request);
        }

        [HttpPut("{id:guid}")]
        public async Task<IActionResult> Update(Guid id, LifecycleTag request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState.ValidationState);
            
            if (id != request.Id)
                return BadRequest();

            var existingTag = await unitOfWork.LifecycleTags.Get(id);
            if (existingTag is null)
            {
                var message = localizationService.GetLocalizedString("LifecycleTagNotFound", id);
                return NotFound(new GenericResponse(false, message));
            }

            // Validate name is provided
            if (string.IsNullOrWhiteSpace(request.Name))
            {
                var message = localizationService.GetLocalizedString("LifecycleTagNameRequired");
                return BadRequest(new GenericResponse(false, message));
            }

            // Check if name already exists (excluding current tag)
            var nameExists = await unitOfWork.LifecycleTags.ExistsInLifecycle(request.Name, request.LifecycleId, id);
            if (nameExists)
            {
                var message = localizationService.GetLocalizedString("LifecycleTagAlreadyExists", request.Name);
                return Conflict(new GenericResponse(false, message));
            }

            await unitOfWork.LifecycleTags.Update(request);
            return Ok(request);
        }

        [HttpDelete("{id:guid}")]
        public async Task<IActionResult> Delete(Guid id)
        {
            var tag = await unitOfWork.LifecycleTags.Get(id);
            if (tag is null)
            {
                var message = localizationService.GetLocalizedString("LifecycleTagNotFound", id);
                return NotFound(new GenericResponse(false, message));
            }

            await unitOfWork.LifecycleTags.Remove(tag);
            return Ok(new GenericResponse(true));
        }

        [HttpGet("Status/{statusId:guid}")]
        public async Task<IActionResult> GetTagsByStatus(Guid statusId)
        {
            var status = await unitOfWork.Lifecycles.StatusRepository.Get(statusId);
            if (status is null)
            {
                var message = localizationService.GetLocalizedString("StatusNotFound", statusId);
                return NotFound(new GenericResponse(false, message));
            }

            var tags = await unitOfWork.LifecycleTags.GetTagsByStatus(statusId);
            return Ok(tags);
        }

        [HttpPost("Status/{statusId:guid}/AssignTag/{tagId:guid}")]
        public async Task<IActionResult> AssignTagToStatus(Guid statusId, Guid tagId)
        {
            var status = await unitOfWork.Lifecycles.StatusRepository.Get(statusId);
            if (status is null)
            {
                var message = localizationService.GetLocalizedString("StatusNotFound", statusId);
                return NotFound(new GenericResponse(false, message));
            }

            var tag = await unitOfWork.LifecycleTags.Get(tagId);
            if (tag is null)
            {
                var message = localizationService.GetLocalizedString("LifecycleTagNotFound", tagId);
                return NotFound(new GenericResponse(false, message));
            }

            // Validate both belong to the same lifecycle
            if (status.LifecycleId != tag.LifecycleId)
            {
                var message = localizationService.GetLocalizedString("TagNotInSameLifecycle");
                return BadRequest(new GenericResponse(false, message));
            }

            // Check if already assigned
            var alreadyAssigned = unitOfWork.StatusLifecycleTags
                .Find(slt => slt.StatusId == statusId && slt.LifecycleTagId == tagId && !slt.Disabled)
                .Any();

            if (alreadyAssigned)
            {
                var message = localizationService.GetLocalizedString("LifecycleTagAssigned");
                return Ok(new GenericResponse(true, message));
            }

            // Create the assignment
            var assignment = new StatusLifecycleTag
            {
                StatusId = statusId,
                LifecycleTagId = tagId
            };

            await unitOfWork.StatusLifecycleTags.Add(assignment);

            var successMessage = localizationService.GetLocalizedString("LifecycleTagAssigned");
            return Ok(new GenericResponse(true, successMessage));
        }

        [HttpDelete("Status/{statusId:guid}/RemoveTag/{tagId:guid}")]
        public async Task<IActionResult> RemoveTagFromStatus(Guid statusId, Guid tagId)
        {
            var status = await unitOfWork.Lifecycles.StatusRepository.Get(statusId);
            if (status is null)
            {
                var message = localizationService.GetLocalizedString("StatusNotFound", statusId);
                return NotFound(new GenericResponse(false, message));
            }

            var tag = await unitOfWork.LifecycleTags.Get(tagId);
            if (tag is null)
            {
                var message = localizationService.GetLocalizedString("LifecycleTagNotFound", tagId);
                return NotFound(new GenericResponse(false, message));
            }

            // Find the assignment
            var assignment = unitOfWork.StatusLifecycleTags
                .Find(slt => slt.StatusId == statusId && slt.LifecycleTagId == tagId && !slt.Disabled)
                .FirstOrDefault();

            if (assignment is null)
            {
                var message = localizationService.GetLocalizedString("LifecycleTagNotFound", tagId);
                return NotFound(new GenericResponse(false, message));
            }

            await unitOfWork.StatusLifecycleTags.Remove(assignment);

            var successMessage = localizationService.GetLocalizedString("LifecycleTagRemoved");
            return Ok(new GenericResponse(true, successMessage));
        }

        [HttpGet("Tag/{tagId:guid}/Statuses")]
        public async Task<IActionResult> GetStatusesByTag(Guid tagId)
        {
            var tag = await unitOfWork.LifecycleTags.Get(tagId);
            if (tag is null)
            {
                var message = localizationService.GetLocalizedString("LifecycleTagNotFound", tagId);
                return NotFound(new GenericResponse(false, message));
            }

            var statuses = await unitOfWork.LifecycleTags.GetStatusesByTag(tagId);
            return Ok(statuses);
        }
    }
}
