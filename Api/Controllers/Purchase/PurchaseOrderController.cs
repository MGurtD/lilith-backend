using Application.Contracts;
using Application.Contracts.Purchase;
using Application.Persistance;
using Application.Services.Purchase;
using Domain.Entities.Purchase;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;

namespace Api.Controllers.Purchase
{
    [ApiController]
    [Route("api/[controller]")]
    public class PurchaseOrderController : ControllerBase
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IPurchaseOrderService _service;

        public PurchaseOrderController(IPurchaseOrderService service, IUnitOfWork unitOfWork)
        {
            _service = service;
            _unitOfWork = unitOfWork;
        }

        [HttpGet]
        public async Task<IActionResult> Get(DateTime startTime, DateTime endTime, Guid? supplierId, Guid? statusId)
        {
            var orders = await _service.GetBetweenDates(startTime, endTime, supplierId, statusId);

            if (orders != null) return Ok(orders.OrderBy(e => e.Number));
            else return BadRequest();
        }

        [HttpGet("{id:guid}")]
        public async Task<IActionResult> GetById(Guid id)
        {
            var receipt = await _unitOfWork.PurchaseOrders.Get(id);

            if (receipt == null) return BadRequest();
            else return Ok(receipt);
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
        public async Task<IActionResult> Update(Guid id, [FromBody] PurchaseOrder request)
        {
            if (id != request.Id) return BadRequest();
            var order = _unitOfWork.PurchaseOrders.Find(r => r.Id == request.Id).FirstOrDefault();
            if (order == null) return NotFound(new GenericResponse(false, $"Comanda de compra amb ID {request.Id} inexistent" ));
            var response = await _service.Update(request);

            if (response.Result) return Ok(response);
            else return BadRequest(response);
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
        [SwaggerOperation("PurchaseOrderDetailCreate")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> AddDetail(PurchaseOrderDetail detail)
        {
            var response = await _service.AddDetail(detail);

            if (response.Result) return Ok(response);
            else return BadRequest(response);
        }

        [HttpPut("Detail/{id:guid}")]
        [SwaggerOperation("PurchaseOrderDetailUpdate")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> UpdateDetail(Guid id, [FromBody] PurchaseOrderDetail detail)
        {
            var response = await _service.UpdateDetail(detail);

            if (response.Result) return Ok(response);
            else return BadRequest(response);
        }

        [HttpDelete("Detail/{id:guid}")]
        [SwaggerOperation("PurchaseOrderDetailDelete")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> RemoveDetail(Guid id)
        {
            var response = await _service.RemoveDetail(id);

            if (response.Result) return Ok(response);
            else return BadRequest(response);
        }
        #endregion

    }
}
