using Microsoft.AspNetCore.Mvc;
using Application.Contracts;
using Domain.Entities;

namespace Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class PaymentMethodController : ControllerBase
    {
        private readonly IUnitOfWork _unitOfWork;

        public PaymentMethodController(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        [HttpPost]
        public async Task<IActionResult> Create(PaymentMethod request)
        {
            // Validation the incoming request
            if (!ModelState.IsValid) return BadRequest(ModelState.ValidationState);


            // Validate existence of the unique user key
            var exists = _unitOfWork.PaymentMethods.Find(r => request.Name == r.Name).Count() > 0;
            if (!exists)
            {
                await _unitOfWork.PaymentMethods.Add(request);

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
            var paymentMethods = await _unitOfWork.PaymentMethods.GetAll();
            return Ok(paymentMethods.OrderBy(e => e.Name));
        }

        [HttpGet("{id:guid}")]
        public async Task<IActionResult> GetById(Guid id)
        {
            var paymentMethod = await _unitOfWork.PaymentMethods.Get(id);
            if (paymentMethod is not null)
            {
                return Ok(paymentMethod);
            } 
            else
            {
                return NotFound();
            }
        }

        [HttpPut("{id:guid}")]
        public async Task<IActionResult> Update(Guid Id, PaymentMethod request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState.ValidationState);
            if (Id != request.Id)
                return BadRequest();

            var exists = await _unitOfWork.PaymentMethods.Exists(request.Id);
            if (!exists)
                return NotFound();

            await _unitOfWork.PaymentMethods.Update(request);
            return Ok(request);
        }

        [HttpDelete("{id:guid}")]
        public async Task<IActionResult> Delete(Guid id)
        {
            var payementMethod = await _unitOfWork.PaymentMethods.Get(id);
            if (payementMethod is not null)
            {
                await _unitOfWork.PaymentMethods.Remove(payementMethod);
                return NoContent();
            }
            else
            {
                return NotFound();
            }
        }

    }
}