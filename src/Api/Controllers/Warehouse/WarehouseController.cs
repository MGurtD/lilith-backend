using Application.Contracts;
using Domain.Entities.Warehouse;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers.Warehouse
{
    [ApiController]
    [Route("api/[controller]")]
    public class WarehouseController(IUnitOfWork unitOfWork, ILocalizationService localizationService) : ControllerBase
    {
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var entities = await unitOfWork.Warehouses.GetAll();

            return Ok(entities);
        }

        [HttpGet("Site/{id:guid}")]
        public async Task<IActionResult> GetBySiteId(Guid id)
        {
            var entities = await unitOfWork.Warehouses.GetBySiteId(id);
            return Ok(entities);
        }

        [HttpGet("WithLocations")]
        public async Task<IActionResult> GetAllWithLocations()
        {
            var entities = await unitOfWork.Warehouses.GetAllWithLocations();
            return Ok(entities);
        }

        [HttpGet("{id:guid}")]
        public async Task<IActionResult> GetById(Guid id)
        {
            var entity = await unitOfWork.Warehouses.Get(id);
            if (entity is null) return NotFound();

            return Ok(entity);
        }

        [HttpPost]
        public async Task<IActionResult> Create(Domain.Entities.Warehouse.Warehouse request)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState.ValidationState);

            var exists = unitOfWork.Warehouses.Find(r => request.Name == r.Name).Any();
            if (!exists)
            {
                await unitOfWork.Warehouses.Add(request);
                var location = Url.Action(nameof(GetById), new { id = request.Id }) ?? $"/{request.Id}";
                return Created(location, request);
            }
            else
            {
                return Conflict(new GenericResponse(false, localizationService.GetLocalizedString("WarehouseAlreadyExists", request.Name)));
            }
        }

        [HttpPut("{id:guid}")]
        public async Task<IActionResult> Update(Guid Id, Domain.Entities.Warehouse.Warehouse request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState.ValidationState);
            if (Id != request.Id)
                return BadRequest();

            var exists = await unitOfWork.Warehouses.Exists(request.Id);
            if (!exists)
                return NotFound();

            await unitOfWork.Warehouses.Update(request);
            return Ok(request);
        }

        [HttpDelete("{id:guid}")]
        public async Task<IActionResult> Delete(Guid id)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState.ValidationState);

            var entity = unitOfWork.Warehouses.Find(e => e.Id == id).FirstOrDefault();
            if (entity is null)
                return NotFound();

            await unitOfWork.Warehouses.Remove(entity);
            return Ok(entity);
        }

        [HttpPost("Location")]
        public async Task<IActionResult> CreateLocation(Location request)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState.ValidationState);

            var exists = unitOfWork.Warehouses.Locations.Find(r => request.Name == r.Name && request.WarehouseId == r.WarehouseId).Any();
            if (!exists)
            {
                await unitOfWork.Warehouses.Locations.Add(request);
                return Ok(new GenericResponse(true, request));
            }
            else
            {
                return Conflict(new GenericResponse(false, localizationService.GetLocalizedString("LocationAlreadyExists", request.Name)));
            }
        }

        [HttpPut("Location/{id:guid}")]
        public async Task<IActionResult> UpdateLocation(Guid id, [FromBody] Location request)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState.ValidationState);
            if (id != request.Id) return BadRequest();

            var exists = unitOfWork.Warehouses.Locations.Find(r => request.Id == r.Id).Any();
            if (exists)
            {
                await unitOfWork.Warehouses.Locations.Update(request);
                return Ok(new GenericResponse(true, request));
            }
            else
            {
                return NotFound(new GenericResponse(false, localizationService.GetLocalizedString("LocationNotFound", request.Id)));
            }
        }

        [HttpDelete("Location/{id:guid}")]
        public async Task<IActionResult> DeleteLocation(Guid id)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState.ValidationState);

            var location = unitOfWork.Warehouses.Locations.Find(r => id == r.Id).FirstOrDefault();
            if (location is not null)
            {
                await unitOfWork.Warehouses.Locations.Remove(location);
                return Ok(new GenericResponse(true, location));
            }
            else
            {
                return NotFound(new GenericResponse(false, localizationService.GetLocalizedString("LocationNotFound", id)));
            }
        }
    }
}
