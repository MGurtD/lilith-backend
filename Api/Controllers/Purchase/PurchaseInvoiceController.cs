using Api.Services;
using Application.Contract;
using Application.Contracts;
using Application.Contracts.Purchase;
using Application.Persistance;
using Application.Services;
using Application.Services.Purchase;
using Domain.Entities.Purchase;
using Domain.Entities.Sales;
using Infrastructure.Persistance;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;

namespace Api.Controllers.Purchase
{
    [ApiController]
    [Route("api/[controller]")]
    public class PurchaseInvoiceController : ControllerBase
    {
        private readonly IPurchaseInvoiceService _service;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IDueDateService _dueDateService;
        private readonly ILocalizationService _localizationService;

        public PurchaseInvoiceController(IPurchaseInvoiceService service, IUnitOfWork unitOfWork, IDueDateService dueDateService, ILocalizationService localizationService)
        {
            _service = service;
            _unitOfWork = unitOfWork;
            _dueDateService = dueDateService;
            _localizationService = localizationService;
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
            else if (supplierId.HasValue)
                purchaseInvoices = _service.GetBetweenDatesAndSupplier(startTime, endTime, supplierId.Value);
            else if (statusId.HasValue)
                purchaseInvoices = _service.GetBetweenDatesAndStatus(startTime, endTime, statusId.Value);
            else if (excludeStatusId.HasValue)
                purchaseInvoices = _service.GetBetweenDatesAndExcludeStatus(startTime, endTime, excludeStatusId.Value);
            else
                purchaseInvoices = _service.GetBetweenDates(startTime, endTime);

            if (purchaseInvoices != null) return Ok(purchaseInvoices.OrderByDescending(e => e.Number));
            else return BadRequest();
        }

        [HttpPost]
        [Route("DueDates")]
        public async Task<IActionResult> GetDueDates(PurchaseInvoice invoice)
        {
            // Recuperar metode de pagament
            var paymentMethod = await _unitOfWork.PaymentMethods.Get(invoice.PaymentMethodId);
            if (paymentMethod == null || paymentMethod.Disabled)
            {
                return NotFound(new GenericResponse(false, _localizationService.GetLocalizedString("PaymentMethodNotFoundOrDisabled", invoice.PaymentMethodId)));
            }

            var invoiceDueDates = new List<PurchaseInvoiceDueDate>();
            _dueDateService.GenerateDueDates(paymentMethod, invoice.PurchaseInvoiceDate, invoice.NetAmount)
                .ForEach(dueDate => invoiceDueDates.Add(new PurchaseInvoiceDueDate()
                {
                    DueDate = dueDate.Date,
                    Amount = dueDate.Amount,
                    PurchaseInvoiceId = invoice.Id
                }));
            return Ok(invoiceDueDates);
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
        public async Task<IActionResult> UpdateStatuses(ChangeStatusOfInvoicesRequest request)
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

        #region Imports
        [HttpPost("Import")]
        [SwaggerOperation("PurchaseInvoiceImportCreate")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> AddImport(PurchaseInvoiceImport import)
        {
            var response = await _service.AddImport(import);

            if (response.Result) return Ok();
            else return BadRequest(response.Errors);
        }

        [HttpPut("Import/{id:guid}")]
        [SwaggerOperation("PurchaseInvoiceImportUpdate")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> UpdateImport(Guid id, [FromBody] PurchaseInvoiceImport import)
        {
            var response = await _service.UpdateImport(import);

            if (response.Result) return Ok();
            else return BadRequest(response.Errors);
        }

        [HttpDelete("Import/{id:guid}")]
        [SwaggerOperation("PurchaseInvoiceImportDelete")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> RemoveImport(Guid id)
        {
            var response = await _service.RemoveImport(id);

            if (response.Result) return Ok();
            else return BadRequest(response.Errors);
        }
        #endregion

        #region DueDates
        [HttpPost("DueDate")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> AddDueDates([FromBody] IEnumerable<PurchaseInvoiceDueDate> dueDates)
        {
            var response = await _service.AddDueDates(dueDates);
            if (response.Result) return Ok();
            else return BadRequest(response.Errors);
        }

        [HttpDelete("DueDate")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> RemoveDueDates([FromQuery] IEnumerable<Guid> ids)
        {
            var response = await _service.RemoveDueDates(ids);
            if (response.Result) return Ok();
            else return BadRequest(response.Errors);
        }
        #endregion

    }
}
