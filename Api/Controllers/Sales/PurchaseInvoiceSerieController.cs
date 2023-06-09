using Application.Persistance;
using Domain.Entities.Purchase;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers.Sales
{
    [ApiController]
    [Route("api/[controller]")]
    public class PurchaseInvoiceSerieController : ControllerBase
    {
        private readonly IUnitOfWork _unitOfWork;

        public PurchaseInvoiceSerieController(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        [HttpPost]
        public async Task<IActionResult> Create(PurchaseInvoiceSerie request)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState.ValidationState);

            var exists = _unitOfWork.PurchaseInvoiceSeries.Find(r => request.Name == r.Name).Any();
            if (!exists)
            {
                await _unitOfWork.PurchaseInvoiceSeries.Add(request);
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
            var entities = await _unitOfWork.PurchaseInvoiceSeries.GetAll();
            return Ok(entities);
        }

        [HttpGet("{id:guid}")]
        public async Task<IActionResult> GetById(Guid id)
        {
            var entity = await _unitOfWork.PurchaseInvoiceSeries.Get(id);
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
        public async Task<IActionResult> Update(Guid Id, PurchaseInvoiceSerie request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState.ValidationState);
            if (Id != request.Id)
                return BadRequest();

            var exists = await _unitOfWork.PurchaseInvoiceSeries.Exists(request.Id);
            if (!exists)
                return NotFound();

            await _unitOfWork.PurchaseInvoiceSeries.Update(request);
            return Ok(request);
        }

        [HttpDelete("{id:guid}")]
        public async Task<IActionResult> Delete(Guid id)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState.ValidationState);

            var entity = _unitOfWork.PurchaseInvoiceSeries.Find(e => e.Id == id).FirstOrDefault();
            if (entity is null)
                return NotFound();

            await _unitOfWork.PurchaseInvoiceSeries.Remove(entity);
            return Ok(entity);
        }
    }
}
