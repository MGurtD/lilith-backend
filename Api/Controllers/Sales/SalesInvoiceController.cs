using Application.Contracts.Sales;
using Application.Persistance;
using Application.Services;
using Domain.Entities.Sales;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;

namespace Api.Controllers.Sales
{
    [ApiController]
    [Route("api/[controller]")]
    public class SalesInvoiceController : ControllerBase
    {
        private readonly ISalesInvoiceService _service;
        private readonly IUnitOfWork _unitOfWork;

        public SalesInvoiceController(ISalesInvoiceService service, IUnitOfWork unitOfWork)
        {
            _service = service;
            _unitOfWork = unitOfWork;
        }        

        [HttpPost]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> Create(CreateInvoiceRequest createInvoiceRequest)
        {
            var response = await _service.Create(createInvoiceRequest);

            if (response.Result)
                return Ok(response);
            else
                return BadRequest(response);
        }

        [HttpGet("{id:guid}")]
        public async Task<IActionResult> GetById(Guid id)
        {
            var invoice = await _service.GetById(id);
            if (invoice == null)
                return NotFound();
            else 
                return Ok(invoice);
        }

        [HttpGet]
        public IActionResult GetInvoices(DateTime startTime, DateTime endTime, Guid? customerId, Guid? statusId, Guid? exerciceId)
        {
            IEnumerable<SalesInvoice> invoices = new List<SalesInvoice>();
            if (exerciceId.HasValue)
                invoices = _service.GetByExercise(exerciceId.Value);
            else if (customerId.HasValue)
                invoices = _service.GetBetweenDatesAndCustomer(startTime, endTime, customerId.Value);
            else if (statusId.HasValue)
                invoices = _service.GetBetweenDatesAndStatus(startTime, endTime, statusId.Value);
            else
                invoices = _service.GetBetweenDates(startTime, endTime);

            if (invoices != null) return Ok(invoices);
            else return BadRequest();
        }

        [HttpPut("{id:guid}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> Update(Guid id, [FromBody] SalesInvoice invoice)
        {
            var response = await _service.Update(invoice);

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

        #region Details
        [HttpPost("Detail")]
        [SwaggerOperation("AddSalesInvoiceDetail")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> AddSalesInvoiceDetail(SalesInvoiceDetail detail)
        {
            var response = await _service.AddDetail(detail);

            if (response.Result) return Ok();
            else return BadRequest(response.Errors);
        }

        [HttpPost("Detail/FromOrderDetails")]
        [SwaggerOperation("CreateInvoiceDetailsFromOrderDetails")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> AddDetailsFromOrderDetails(CreateInvoiceDetailsFromOrderDetailsRequest request)
        {
            var invoice = await _unitOfWork.SalesInvoices.Get(request.InvoiceId);
            if (invoice == null) return NotFound();

            var response = await _service.AddDetailsFromOrderDetails(invoice, request.OrderDetails);
            if (response.Result) return Ok();
            else return BadRequest(response.Errors);
        }

        [HttpPut("Detail/{id:guid}")]
        [SwaggerOperation("SalesInvoiceDetailUpdate")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> UpdateDetail(Guid id, [FromBody] SalesInvoiceDetail detail)
        {
            var response = await _service.UpdateDetail(detail);

            if (response.Result) return Ok();
            else return BadRequest(response.Errors);
        }

        [HttpDelete("Detail/{id:guid}")]
        [SwaggerOperation("SalesInvoiceDetailDelete")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> RemoveDetail(Guid id)
        {
            var response = await _service.RemoveDetail(id);

            if (response.Result) return Ok();
            else return BadRequest(response.Errors);
        }
        #endregion

    }
}
