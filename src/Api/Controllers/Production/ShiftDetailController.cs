using Application.Contracts;
using Domain.Entities.Production;
using Microsoft.AspNetCore.Mvc;
using System.Security.Policy;

namespace Api.Controllers.Production
{
    [ApiController]
    [Route("api/[controller]")]
    public class ShiftDetailDetailController : ControllerBase
    {
        private readonly IUnitOfWork _unitOfWork;

        public ShiftDetailDetailController(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }
        [HttpPost]
        public async Task<IActionResult> Create(ShiftDetail request)
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
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var entities = await _unitOfWork.ShiftDetails.GetAll();

            return Ok(entities);
        }

        [HttpGet("{id:guid}")]
        public async Task<IActionResult> GetById(Guid id)
        {
            var entity = await _unitOfWork.ShiftDetails.Get(id);
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
        public async Task<IActionResult> Update(Guid Id, ShiftDetail request)
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

        [HttpDelete("{id:guid}")]
        public async Task<IActionResult> Delete(Guid id)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState.ValidationState);

            var entity = _unitOfWork.ShiftDetails.Find(e => e.Id == id).FirstOrDefault();
            if (entity is null)
                return NotFound();

            await _unitOfWork.ShiftDetails.Remove(entity);
            return Ok(entity);
        }
    }
}

