using Application.Contracts;
using Application.Services;
using Domain.Entities;
using Domain.Entities.Shared;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers.Shared
{
    [ApiController]
    [Route("api/[controller]")]
    public class LifecycleController(ILifecycleService lifecycleService) : ControllerBase
    {
        [HttpPost]
        public async Task<IActionResult> Create(Lifecycle request)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState.ValidationState);

            var response = await lifecycleService.CreateLifecycle(request);
            if (response.Result)
            {
                var location = Url.Action(nameof(GetById), new { id = request.Id }) ?? $"/{request.Id}";
                return Created(location, response.Content);
            }
            else
            {
                return Conflict(response);
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var entities = await lifecycleService.GetAllLifecycles();
            return Ok(entities);
        }

        [HttpGet("{id:guid}")]
        public async Task<IActionResult> GetById(Guid id)
        {
            var entity = await lifecycleService.GetLifecycleById(id);
            if (entity is not null)
            {
                return Ok(entity);
            }
            else
            {
                return NotFound();
            }
        }

        [HttpGet("Name/{name}")]
        public async Task<IActionResult> GetByName(string name)
        {
            var entity = await lifecycleService.GetLifecycleByName(name);
            if (entity is not null)
                return Ok(entity);
            else
                return NotFound();
        }

        [HttpPut("{id:guid}")]
        public async Task<IActionResult> Update(Guid Id, Lifecycle request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState.ValidationState);
            if (Id != request.Id)
                return BadRequest();

            var response = await lifecycleService.UpdateLifecycle(request);
            if (response.Result)
                return Ok(response.Content);
            else
                return NotFound(response);
        }

        [HttpDelete("{id:guid}")]
        public async Task<IActionResult> Delete(Guid id)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState.ValidationState);

            var response = await lifecycleService.RemoveLifecycle(id);
            if (response.Result)
                return Ok(response.Content);
            else
                return NotFound(response);
        }

        [Route("Status")]
        [HttpPost]
        public async Task<IActionResult> CreateStatus(Status request)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState.ValidationState);

            var response = await lifecycleService.CreateStatus(request);
            if (response.Result)
                return Ok(response.Content);
            else
                return BadRequest(response);
        }

        [Route("Status/{id:guid}")]
        [HttpPut]
        public async Task<IActionResult> UpdateStatus(Guid id, Status request)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState.ValidationState);

            var response = await lifecycleService.UpdateStatus(request);
            if (response.Result)
                return Ok(response.Content);
            else
                return NotFound(response);
        }

        [Route("Status/{id:guid}")]
        [HttpDelete]
        public async Task<IActionResult> RemoveContact(Guid id)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState.ValidationState);

            var response = await lifecycleService.RemoveStatus(id);
            if (response.Result)
                return Ok(response.Content);
            else
                return NotFound(response);
        }

        [Route("StatusTransition")]
        [HttpPost]
        public async Task<IActionResult> CreateStatusTransition(StatusTransition request)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState.ValidationState);

            var response = await lifecycleService.CreateStatusTransition(request);
            if (response.Result)
                return Ok(response.Content);
            else
                return BadRequest(response);
        }

        [Route("StatusTransition/{id:guid}")]
        [HttpPut]
        public async Task<IActionResult> UpdateStatusTransition(Guid id, StatusTransition request)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState.ValidationState);

            var response = await lifecycleService.UpdateStatusTransition(request);
            if (response.Result)
                return Ok(response.Content);
            else
                return NotFound(response);
        }

        [Route("StatusTransition/{id:guid}")]
        [HttpDelete]
        public async Task<IActionResult> RemoveStatusTransition(Guid id)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState.ValidationState);

            var response = await lifecycleService.RemoveStatusTransition(id);
            if (response.Result)
                return Ok(response.Content);
            else
                return NotFound(response);
        }
    }
}
