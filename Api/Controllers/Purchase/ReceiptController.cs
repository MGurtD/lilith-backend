using Application.Contracts;
using Application.Contracts.Purchase;
using Application.Persistance;
using Application.Services.Purchase;
using Domain.Entities.Purchase;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using Application.Services;

namespace Api.Controllers.Purchase
{
    [ApiController]
    [Route("api/[controller]")]
    public class ReceiptController : ControllerBase
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IReceiptService _service;
        private readonly IReferenceService _referenceService;

        public ReceiptController(IReceiptService service, IUnitOfWork unitOfWork, IReferenceService referenceService)
        {
            _service = service;
            _unitOfWork = unitOfWork;
            _referenceService = referenceService;
        }

        [HttpGet]
        public IActionResult GetReceipts(DateTime startTime, DateTime endTime, Guid? supplierId, Guid? statusId)
        {
            IEnumerable<Receipt> receipts = new List<Receipt>();
            if (supplierId.HasValue)
                receipts = _service.GetBetweenDatesAndSupplier(startTime, endTime, supplierId.Value);
            else if (statusId.HasValue)
                receipts = _service.GetBetweenDatesAndStatus(startTime, endTime, statusId.Value);
            else
                receipts = _service.GetBetweenDates(startTime, endTime);

            if (receipts != null) return Ok(receipts.OrderByDescending(e => e.Number));
            else return BadRequest();
        }

        [HttpGet("{id:guid}")]
        public async Task<IActionResult> GetById(Guid id)
        {
            var receipt = await _unitOfWork.Receipts.Get(id);

            if (receipt == null) return BadRequest();
            else return Ok(receipt);
        }

        [HttpGet("ByReferenceId/{id:guid}")]
        public async Task<IActionResult> GetByReferenceId(Guid id)
        {
            var receipts = await _unitOfWork.Receipts.GetReceiptsByReferenceId(id);

            if (receipts == null) return BadRequest();
            else return Ok(receipts);
        }

        [HttpGet("ToInvoice/{supplierId:guid}")]
        public IActionResult GetSupplierInvoiceableReceipts(Guid supplierId)
        {
            IEnumerable<Receipt> receipts = _service.GetBySupplier(supplierId, true);

            if (receipts != null) return Ok(receipts.OrderBy(e => e.Number));
            else return BadRequest();
        }

        [HttpGet("Invoice/{id:guid}")]
        public IActionResult GetReceiptsByInvoiceId(Guid id)
        {
            IEnumerable<Receipt> receipts = _service.GetByInvoice(id);

            if (receipts != null) return Ok(receipts.OrderBy(e => e.Number));
            else return BadRequest();
        }

        [HttpPost]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> Create(CreatePurchaseDocumentRequest createRequest)
        {
            var response = await _service.Create(createRequest);

            if (response.Result)
                return Ok(response);
            else
                return BadRequest(response);
        }

        [HttpPut("{id:guid}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> Update(Guid id, [FromBody] Receipt request)
        {
            if (id != request.Id) return BadRequest();
            var receipt = _unitOfWork.Receipts.Find(r => r.Id == request.Id).FirstOrDefault();
            if (receipt == null) return NotFound(new GenericResponse(false, $"Albará amb ID {request.Id} inexistent" ));

            var moveToWarehouseStatus = _unitOfWork.Lifecycles.StatusRepository.Find(s => s.Name == "Recepcionat").FirstOrDefault();
            if (moveToWarehouseStatus == null) return NotFound(new GenericResponse(false, $"Estat recepcionat inexistent"));

            var warehouseResponse = new GenericResponse(true);
            if (receipt.StatusId != moveToWarehouseStatus.Id && request.StatusId == moveToWarehouseStatus.Id)
                warehouseResponse = await _service.MoveToWarehose(request);                  

            if (receipt.StatusId == moveToWarehouseStatus.Id && request.StatusId != moveToWarehouseStatus.Id)
                warehouseResponse = await _service.RetriveFromWarehose(request);

            var globalResponse = new GenericResponse(true);
            if (warehouseResponse.Result)
                globalResponse = await _service.Update(request);
                request.Details = await _unitOfWork.Receipts.Details.FindAsync(r => r.ReceiptId == request.Id);
                await _referenceService.UpdatePriceFromReceipt(request);

            if (globalResponse.Result && warehouseResponse.Result) return Ok(globalResponse);
            else return BadRequest(globalResponse);
        }

        [HttpDelete("{id:guid}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> Remove(Guid id)
        {
            var response = await _service.Remove(id);

            if (response.Result) return Ok();
            else return BadRequest(response);
        }

        #region Details
        [HttpPost("Detail")]
        [SwaggerOperation("ReceiptDetailCreate")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> AddDetail(ReceiptDetail detail)
        {
            var response = await _service.AddDetail(detail);

            if (response.Result) return Ok(response);
            else return BadRequest(response);
        }

        [HttpPut("Detail/{id:guid}")]
        [SwaggerOperation("ReceiptDetailUpdate")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> UpdateDetail(Guid id, [FromBody] ReceiptDetail detail)
        {
            var response = await _service.UpdateDetail(detail);

            if (response.Result) return Ok(response);
            else return BadRequest(response);
        }

        [HttpDelete("Detail/{id:guid}")]
        [SwaggerOperation("ReceiptDetailDelete")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> RemoveDetail(Guid id)
        {
            var response = await _service.RemoveDetail(id);

            if (response.Result) return Ok(response);
            else return BadRequest(response);
        }

        [HttpPost("Detail/Calculate")]
        [SwaggerOperation("ReceiptDetailCalculate")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> CalculateDetailPriceAndWeight(ReceiptDetail detail)
        {
            var response = await _service.CalculateDetailWeightAndPrice(detail);

            if (response.Result) return Ok(response);
            else return BadRequest(response);
        }
        #endregion

        #region Receiptions

        [HttpGet("{id:guid}/Receptions")]
        [SwaggerOperation("PurchaseOrderReceiptsFromReceipt")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> GetReceptions(Guid id)
        {
            if (id == Guid.Empty) return BadRequest();
            var receipt = await _unitOfWork.Receipts.Get(id);
            if (receipt == null) return NotFound();

            var receptions = await _service.GetReceptions(id);
            return Ok(receptions);
        }

        [HttpPost("AddReceptions")]
        [SwaggerOperation("AddPurchaseOrderReceipts")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> AddReceptions(AddReceptionsRequest request)
        {
            var response = await _service.AddReceptions(request.ReceiptId, request.Receptions);

            if (response.Result) return Ok(response);
            else return BadRequest(response);
        }

        #endregion

    }
}
