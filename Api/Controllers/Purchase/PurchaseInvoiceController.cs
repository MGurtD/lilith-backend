using Application.Services;
using Domain.Entities.Purchase;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers.Purchase
{
    [ApiController]
    [Route("api/[controller]")]
    public class PurchaseInvoiceController : ControllerBase
    {
        private readonly IPurchaseInvoiceService _service;

        public PurchaseInvoiceController(IPurchaseInvoiceService service)
        {
            _service = service;
        }

        [HttpPost]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> Create(PurchaseInvoice purchaseInvoice)
        {
            var response = await _service.Create(purchaseInvoice);

            if (response.Result)
            {
                return Ok();
            }
            else
            {
                return BadRequest(response.Errors);
            }

        }

        [HttpPost]
        [Route("DueDates")]
        public async Task<IActionResult> GetDueDates(PurchaseInvoice purchaseInvoice)
        {
            var dueDates = await _service.GetPurchaseInvoiceDueDates(purchaseInvoice);

            if (dueDates == null) return BadRequest();
            else return Ok(dueDates); 
        }

        [HttpGet]
        public async Task<IActionResult> GetPurchaseInvoices(DateTime startTime, DateTime endTime, Guid? supplierId, Guid? statusId, Guid? exerciceId)
        {
            IEnumerable<PurchaseInvoice> purchaseInvoices = new List<PurchaseInvoice>();
            if (exerciceId.HasValue)
                purchaseInvoices = await _service.GetByExercise(exerciceId.Value);
            if (supplierId.HasValue)
                purchaseInvoices = _service.GetBetweenDatesAndSupplier(startTime, endTime, supplierId.Value);
            if (statusId.HasValue) 
                purchaseInvoices = _service.GetBetweenDatesAndSupplier(startTime, endTime, statusId.Value);

            purchaseInvoices = _service.GetBetweenDates(startTime, endTime);

            if (purchaseInvoices != null) return Ok(purchaseInvoices);
            else return BadRequest();
        }

    }
}
