using Microsoft.AspNetCore.Mvc;
using Application.Persistance;
using Domain.Entities;

namespace Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class TaxController : ControllerBase
    {
        private readonly IUnitOfWork _unitOfWork;

        public TaxController(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        [HttpPost]
        public async Task<IActionResult> Create(Tax request)
        {
            // Validation the incoming request
            if (!ModelState.IsValid) return BadRequest(ModelState.ValidationState);


            // Validate existence of the unique user key
            var exists = _unitOfWork.Taxes.Find(r => request.Name == r.Name).Count() > 0;
            if (!exists)
            {
                await _unitOfWork.Taxes.Add(request);
                return Ok(request);
            }
            else
            {
                return StatusCode(StatusCodes.Status500InternalServerError);
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var taxes = await _unitOfWork.Taxes.GetAll();
            return Ok(taxes.OrderBy(e => e.Name));
        }

        [HttpGet("{id:guid}")]
        public async Task<IActionResult> GetById(Guid id)
        {
            var Tax = await _unitOfWork.Taxes.Get(id);
            if (Tax is not null)
            {
                return Ok(Tax);
            } 
            else
            {
                return NotFound();
            }
        }

        [HttpPut("{id:guid}")]
        public async Task<IActionResult> Update(Guid Id, Tax request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState.ValidationState);
            if (Id != request.Id)
                return BadRequest();

            var exists = await _unitOfWork.Taxes.Exists(request.Id);
            if (!exists)
                return NotFound();

            await _unitOfWork.Taxes.Update(request);
            return Ok(request);
        }

        [HttpDelete("{id:guid}")]
        public async Task<IActionResult> Delete(Guid id)
        {
            var payementMethod = await _unitOfWork.Taxes.Get(id);
            if (payementMethod is not null)
            {
                await _unitOfWork.Taxes.Remove(payementMethod);
                return NoContent();
            }
            else
            {
                return NotFound();
            }
        }

    }
}