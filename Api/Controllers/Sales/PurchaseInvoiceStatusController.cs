using Application.Persistance;
using Domain.Entities.Purchase;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers.Sales
{
    [ApiController]
    [Route("api/[controller]")]
    public class PurchaseInvoiceStatusController : ControllerBase
    {
        private readonly IUnitOfWork _unitOfWork;

        public PurchaseInvoiceStatusController(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        [HttpPost]
        public async Task<IActionResult> Create(PurchaseInvoiceStatus request)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState.ValidationState);

            var exists = _unitOfWork.PurchaseInvoiceStatuses.Find(r => request.Name == r.Name).Any();
            if (!exists)
            {
                await _unitOfWork.PurchaseInvoiceStatuses.Add(request);
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
            var entities = await _unitOfWork.PurchaseInvoiceStatuses.GetAll();
            return Ok(entities);
        }

        [HttpGet("{id:guid}")]
        public async Task<IActionResult> GetById(Guid id)
        {
            var entity = await _unitOfWork.PurchaseInvoiceStatuses.Get(id);
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
        public async Task<IActionResult> Update(Guid Id, PurchaseInvoiceStatus request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState.ValidationState);
            if (Id != request.Id)
                return BadRequest();

            var exists = await _unitOfWork.PurchaseInvoiceStatuses.Exists(request.Id);
            if (!exists)
                return NotFound();

            await _unitOfWork.PurchaseInvoiceStatuses.Update(request);
            return Ok(request);
        }

        [HttpDelete("{id:guid}")]
        public async Task<IActionResult> Delete(Guid id)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState.ValidationState);

            var entity = _unitOfWork.PurchaseInvoiceStatuses.Find(e => e.Id == id).FirstOrDefault();
            if (entity is null)
                return NotFound();

            await _unitOfWork.PurchaseInvoiceStatuses.Remove(entity);
            return Ok(entity);
        }

        // TODO: Get, Update & Delete Transitions

    }
}
