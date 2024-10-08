﻿using Application.Contracts;
using Application.Contracts.Sales;
using Application.Persistance;
using Application.Services;
using Domain.Entities.Sales;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;

namespace Api.Controllers.Purchase
{
    [ApiController]
    [Route("api/[controller]")]
    public class DeliveryNoteController : ControllerBase
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IDeliveryNoteService _service;

        public DeliveryNoteController(IDeliveryNoteService service, IUnitOfWork unitOfWork)
        {
            _service = service;
            _unitOfWork = unitOfWork;
        }

        [HttpGet("{id:guid}")]
        public async Task<IActionResult> GetById(Guid id)
        {
            var receipt = await _unitOfWork.DeliveryNotes.Get(id);

            if (receipt == null) return BadRequest();
            else return Ok(receipt);
        }

        [HttpGet("Report/{id:guid}")]
        public async Task<IActionResult> GetDeliveryNoteForReport(Guid id)
        {
            var deliveryNote = await _service.GetDtoForReportingById(id);
            return Ok(deliveryNote);
        }

        [HttpGet]
        public IActionResult GetDeliveryNotes(DateTime startTime, DateTime endTime, Guid? customerId, Guid? statusId)
        {
            IEnumerable<DeliveryNote> receipts = new List<DeliveryNote>();
            if (customerId.HasValue)
                receipts = _service.GetBetweenDatesAndCustomer(startTime, endTime, customerId.Value);
            else if (statusId.HasValue)
                receipts = _service.GetBetweenDatesAndStatus(startTime, endTime, statusId.Value);
            else
                receipts = _service.GetBetweenDates(startTime, endTime);

            if (receipts != null) return Ok(receipts.OrderByDescending(e => e.Number));
            else return BadRequest();
        }

        [HttpGet("Invoice/{id:guid}")]
        public IActionResult GetDeliveryNotesByInvoiceId(Guid id)
        {
            var salesOrders = _service.GetBySalesInvoice(id);
            return Ok(salesOrders);
        }

        [HttpGet("ToInvoice")]
        public IActionResult GetDeliveryNotesToInvoice(Guid customerId)
        {
            var salesOrderHeaders = _service.GetDeliveryNotesToInvoice(customerId);
            return Ok(salesOrderHeaders.OrderBy(e => e.Number));
        }

        [HttpPost]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> Create(CreateHeaderRequest createRequest)
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
        public async Task<IActionResult> Update(Guid id, [FromBody] DeliveryNote request)
        {
            if (id != request.Id) return BadRequest();
            var deliveryNote = _unitOfWork.DeliveryNotes.Find(r => r.Id == request.Id).FirstOrDefault();
            if (deliveryNote == null) return NotFound(new GenericResponse(false, new List<string>() { $"Albará amb ID {request.Id} inexistent" }));

            var deliveredStatus = _unitOfWork.Lifecycles.StatusRepository.Find(s => s.Name == "Entregat").FirstOrDefault();
            if (deliveredStatus == null) return NotFound(new GenericResponse(false, $"Estat 'Entregat' inexistent" ));

            var warehouseResponse = new GenericResponse(true);
            if (deliveryNote.StatusId != deliveredStatus.Id && request.StatusId == deliveredStatus.Id)
                warehouseResponse = await _service.Deliver(request);
            if (deliveryNote.StatusId == deliveredStatus.Id && request.StatusId != deliveredStatus.Id)
                warehouseResponse = await _service.UnDeliver(request);

            var globalResponse = new GenericResponse(true);
            if (warehouseResponse.Result)
                globalResponse = await _service.Update(request);

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
        [HttpPost("{id:guid}/AddOrder")]
        [SwaggerOperation("DeliveryNoteAddOrder")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> AddOrder(Guid id, [FromBody] SalesOrderHeader order)
        {
            var response = await _service.AddOrder(id, order);

            if (response.Result) return Ok(response);
            else return BadRequest(response);
        }

        [HttpPost("{id:guid}/RemoveOrder")]
        [SwaggerOperation("DeliveryNoteRemoveOrder")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> RemoveOrder(Guid id, [FromBody] SalesOrderHeader order)
        {
            var response = await _service.RemoveOrder(id, order);
            if (response.Result) return Ok(response);
            else return BadRequest(response);
        }
        #endregion

    }
}
