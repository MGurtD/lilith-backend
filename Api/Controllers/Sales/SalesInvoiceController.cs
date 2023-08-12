using Application.Persistance;
using Application.Persistance.Repositories.Sales;
using Application.Services;
using Domain.Entities.Purchase;
using Domain.Entities.Sales;
using Infrastructure.Persistance;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers.Sales
{
    [ApiController]
    [Route("api/[controller]")]
    public class SalesInvoiceController : ControllerBase
    {
        private readonly ISalesInvoiceService _service;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IDueDateService _dueDateService;

        public SalesInvoiceController(ISalesInvoiceService service, IUnitOfWork unitOfWork, IDueDateService dueDateService)
        {
            _service = service;
            _unitOfWork = unitOfWork;
            _dueDateService = dueDateService;
        }        

        [HttpPost]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> Create(SalesInvoice invoice)
        {
            var response = await _service.Create(invoice);

            if (response.Result)
                return Ok();
            else
                return BadRequest(response.Errors);
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

        [HttpPost]
        [Route("DueDates")]
        public async Task<IActionResult> GetDueDates(SalesInvoice invoice)
        {
            // Recuperar metode de pagament
            var paymentMethod = await _unitOfWork.PaymentMethods.Get(invoice.PaymentMethodId);
            if (paymentMethod == null || paymentMethod.Disabled)
            {
                return NotFound($"El métode de pagament amb ID {invoice.PaymentMethodId} no existeix o está desactivat");
            }

            var invoiceDueDates = new List<SalesInvoiceDueDate>();
            _dueDateService.GenerateDueDates(paymentMethod, invoice.InvoiceDate, invoice.NetAmount)
                .ForEach(dueDate => invoiceDueDates.Add(new SalesInvoiceDueDate()
                {
                    DueDate = dueDate.Date,
                    Amount = dueDate.Amount,
                    SalesInvoiceId = invoice.Id
                }));
            return Ok(invoiceDueDates);
        }

    }
}
