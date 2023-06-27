using Application.Contracts.Purchase;
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

        [HttpGet("{id:guid}")]
        public async Task<IActionResult> GetById(Guid id)
        {
            var invoice = await _service.GetById(id);

            if (invoice == null) return BadRequest();
            else return Ok(invoice);
        }

        [HttpGet]
        public async Task<IActionResult> GetPurchaseInvoices(DateTime startTime, DateTime endTime, Guid? supplierId, Guid? statusId, Guid? excludeStatusId, Guid? exerciceId)
        {
            IEnumerable<PurchaseInvoice> purchaseInvoices = new List<PurchaseInvoice>();
            if (exerciceId.HasValue)
                purchaseInvoices = await _service.GetByExercise(exerciceId.Value);
            if (supplierId.HasValue)
                purchaseInvoices = _service.GetBetweenDatesAndSupplier(startTime, endTime, supplierId.Value);
            else if (statusId.HasValue)
                purchaseInvoices = _service.GetBetweenDatesAndStatus(startTime, endTime, statusId.Value);
            else if (excludeStatusId.HasValue)
                purchaseInvoices = _service.GetBetweenDatesAndExcludeStatus(startTime, endTime, excludeStatusId.Value);
            else
                purchaseInvoices = _service.GetBetweenDates(startTime, endTime);

            if (purchaseInvoices != null) return Ok(purchaseInvoices);
            else return BadRequest();
        }

        [HttpPost]
        [Route("DueDates")]
        public async Task<IActionResult> GetDueDates(PurchaseInvoice purchaseInvoice)
        {
            var dueDates = await _service.GetPurchaseInvoiceDueDates(purchaseInvoice);

            if (dueDates == null) return BadRequest();
            else return Ok(dueDates);
        }

        [HttpPost]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> Create(PurchaseInvoice purchaseInvoice)
        {
            var response = await _service.Create(purchaseInvoice);

            if (response.Result)
                return Ok();
            else
                return BadRequest(response.Errors);
        }

        [HttpPost]
        [Route("UpdateStatuses")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> UpdateStatuses(ChangeStatusOfPurchaseInvoicesRequest request)
        {
            if (!ModelState.IsValid) return BadRequest();

            var response = await _service.ChangeStatuses(request);
            if (response.Result)
                return Ok();
            else
                return BadRequest(response.Errors);
        }

        [HttpPost]
        [Route("RecreateDueDates")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> RecreateDueDates(PurchaseInvoice request)
        {
            if (!ModelState.IsValid) return BadRequest();

            var response = await _service.RecreateDueDates(request);
            if (response.Result)
                return Ok();
            else
                return BadRequest(response.Errors);

        }
        [Route("GetBetweenDates")]
        [HttpPost]
        public IActionResult GetBetweenDates(DateTime startDate, DateTime endDate)
        {
            var response =  _service.GetBetweenDates(startDate, endDate);
            if (response is not null)
            {
                return Ok(response);
            }
            else
            {
                return NotFound();
            }
        }


        [HttpPut("{id:guid}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> Update(Guid id, [FromBody] PurchaseInvoice purchaseInvoice)
        {
            if (id != purchaseInvoice.Id) return BadRequest();

            var response = await _service.Update(purchaseInvoice);

            if (response.Result) return Ok();
            else return BadRequest(response.Errors);
        }

        [HttpDelete("{id:guid}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> Remove(Guid id)
        {
            var response = await _service.Remove(id);

            if (response.Result) return Ok();
            else return BadRequest(response.Errors);
        }

    }
}
