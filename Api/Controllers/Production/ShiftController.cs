using Application.Contracts;
using Application.Persistance;
using Application.Services;
using Domain.Entities.Production;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers.Production
{
    [ApiController]
    [Route("api/[controller]")]
    public class ShiftController : ControllerBase
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILocalizationService _localizationService;

        public ShiftController(IUnitOfWork unitOfWork, ILocalizationService localizationService)
        {
            _unitOfWork = unitOfWork;
            _localizationService = localizationService;
        }

        [HttpPost]
        public async Task<IActionResult> Create(Shift request)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState.ValidationState);

            var exists = _unitOfWork.Shifts.Find(r => request.Name == r.Name).Any();
            if (!exists)
            {
                await _unitOfWork.Shifts.Add(request);
                var location = Url.Action(nameof(GetById), new { id = request.Id }) ?? $"/{request.Id}";
                return Created(location, request);
            }
            else
            {
                return Conflict(new GenericResponse(false, _localizationService.GetLocalizedString("ShiftAlreadyExists", request.Name)));
            }
        }
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var entities = await _unitOfWork.Shifts.GetAll();

            return Ok(entities);
        }

        [HttpGet("{id:guid}")]
        public async Task<IActionResult> GetById(Guid id)
        {
            var entity = await _unitOfWork.Shifts.Get(id);
            if (entity is not null)
            {
                return Ok(entity);
            }
            else
            {
                return NotFound();
            }
        }
        [HttpPut("{id:guid}")]
        public async Task<IActionResult> Update(Guid Id, Shift request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState.ValidationState);
            if (Id != request.Id)
                return BadRequest();

            var exists = await _unitOfWork.Shifts.Exists(request.Id);
            if (!exists)
                return NotFound();

            await _unitOfWork.Shifts.Update(request);
            return Ok(request);
        }

        [HttpDelete("{id:guid}")]
        public async Task<IActionResult> Delete(Guid id)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState.ValidationState);

            var entity = _unitOfWork.Shifts.Find(e => e.Id == id).FirstOrDefault();
            if (entity is null)
                return NotFound();
            entity.Disabled = true;
            await _unitOfWork.Shifts.Update(entity);
            return Ok(entity);
        }

        #region Details
        [HttpPost("Detail")]
        public async Task<IActionResult> CreateDetail(ShiftDetail request)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState.ValidationState);

            await _unitOfWork.ShiftDetails.Add(request);
            var entity = await _unitOfWork.ShiftDetails.Get(request.Id);
            if (entity is not null)
            {
                return Ok(entity);
            }
            else
            {
                return NotFound();
            }

        }
        [HttpPut("Detail/{id:guid}")]
        public async Task<IActionResult> UpdateDetail(Guid Id, ShiftDetail request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState.ValidationState);
            if (Id != request.Id)
                return BadRequest();

            var exists = await _unitOfWork.ShiftDetails.Exists(request.Id);
            if (!exists)
                return NotFound();

            await _unitOfWork.ShiftDetails.Update(request);
            return Ok(request);
        }
        [HttpDelete("Detail/{id:guid}")]
        public async Task<IActionResult> DeleteDetail(Guid id)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState.ValidationState);

            var entity = _unitOfWork.ShiftDetails.Find(e => e.Id == id).FirstOrDefault();
            if (entity is null)
                return NotFound();

            await _unitOfWork.ShiftDetails.Remove(entity);
            return Ok(entity);
        }
        [HttpGet("Detail/{id:guid}")]
        public async Task<IActionResult> GetDetailsByShiftId(Guid id)
        {
            var shift = await _unitOfWork.Shifts.Get(id);
            if (shift is null)
                return NotFound();
            var details = _unitOfWork.ShiftDetails.Find(e => e.ShiftId == id).ToList();
            return Ok(details);
        }
        [HttpPost("Detail/ByIdBetweenHours")]
        public async Task<IActionResult> GetDetailsByShiftIdBetweenHours(Guid id, TimeOnly currentTime)
        {
            var shift = await _unitOfWork.Shifts.Get(id);
            if (shift is null)
                return NotFound();
            var detail = _unitOfWork.ShiftDetails.Find(e => e.ShiftId == id && e.StartTime <= currentTime && e.EndTime > currentTime).FirstOrDefault();
            return Ok(detail);
        }
        #endregion
    }
}
