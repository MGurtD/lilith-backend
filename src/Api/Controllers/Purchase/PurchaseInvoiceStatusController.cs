using Application.Contracts;
using Domain.Entities.Purchase;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers.Purchase
{
    [ApiController]
    [Route("api/[controller]")]
    public class PurchaseInvoiceStatusController(IUnitOfWork unitOfWork) : ControllerBase
    {
        [HttpPost]
        public async Task<IActionResult> Create(PurchaseInvoiceStatus request)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState.ValidationState);

            var exists = unitOfWork.PurchaseInvoiceStatuses.Find(r => request.Name == r.Name).Any();
            if (!exists)
            {
                await unitOfWork.PurchaseInvoiceStatuses.Add(request);
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
            var entities = await unitOfWork.PurchaseInvoiceStatuses.GetAll();
            return Ok(entities.OrderBy(e => e.Name));
        }

        [HttpGet("{id:guid}")]
        public async Task<IActionResult> GetById(Guid id)
        {
            var entity = await unitOfWork.PurchaseInvoiceStatuses.Get(id);
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

            var exists = await unitOfWork.PurchaseInvoiceStatuses.Exists(request.Id);
            if (!exists)
                return NotFound();

            await unitOfWork.PurchaseInvoiceStatuses.Update(request);
            return Ok(request);
        }

        [HttpDelete("{id:guid}")]
        public async Task<IActionResult> Delete(Guid id)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState.ValidationState);

            var entity = unitOfWork.PurchaseInvoiceStatuses.Find(e => e.Id == id).FirstOrDefault();
            if (entity is null)
                return NotFound();

            await unitOfWork.PurchaseInvoiceStatuses.Remove(entity);
            return Ok(entity);
        }

        [Route("Transition")]
        [HttpPost]
        public async Task<IActionResult> CreateTransition(PurchaseInvoiceStatusTransition request)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState.ValidationState);
            if (request.FromStatusId == request.ToStatusId) return BadRequest();

            var statusFrom = await unitOfWork.PurchaseInvoiceStatuses.Get(request.FromStatusId);
            var statusId = await unitOfWork.PurchaseInvoiceStatuses.Get(request.ToStatusId);
            if (statusFrom is not null && statusId is not null)
            {
                await unitOfWork.PurchaseInvoiceStatuses.AddTransition(request);
                return Ok(request);
            }
            else
            {
                return NotFound();
            }
        }

        [Route("Transition/{fromStatusId:guid}/{toStatusId:guid}")]
        [HttpDelete]
        public async Task<IActionResult> RemoveTransition(Guid fromStatusId, Guid toStatusId)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState.ValidationState);

            var deleted = await unitOfWork.PurchaseInvoiceStatuses.RemoveTransition(fromStatusId, toStatusId);
            return deleted ? Ok() : NotFound();
        }

    }
}
