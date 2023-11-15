﻿using Application.Contracts.Sales;
using Application.Services;
using Domain.Entities.Sales;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using SalesOrderDetail = Domain.Entities.Sales.SalesOrderDetail;

namespace Api.Controllers.Sales
{
    [ApiController]
    [Route("api/[controller]")]
    public class SalesOrderController : ControllerBase
    {
        private readonly ISalesOrderService _service;
        public SalesOrderController(ISalesOrderService service)
        {
            _service = service;
        }
        [HttpGet("{id:guid}")]
        public async Task<IActionResult> GetById(Guid id)
        {
            var salesOrder = await _service.GetById(id);
            if (salesOrder == null)
            {
                return NotFound();
            }
            else { return Ok(salesOrder); }
        }

        [HttpGet]
        public IActionResult GetSalesOrders(DateTime startTime, DateTime endTime, Guid? customerId)
        {
            IEnumerable<SalesOrderHeader> salesOrderHeaders = new List<SalesOrderHeader>();
            if (customerId.HasValue)
                salesOrderHeaders = _service.GetBetweenDatesAndCustomer(startTime, endTime, customerId.Value);
            else
                salesOrderHeaders = _service.GetBetweenDates(startTime, endTime);         
            if (salesOrderHeaders != null) return Ok(salesOrderHeaders.OrderBy(e => e.SalesOrderNumber));
            else return BadRequest();
        }

        [HttpGet("DeliveryNote/{id:guid}")]
        public IActionResult GetDeliveryNoteOrders(Guid id)
        {
            var salesOrders = _service.GetByDeliveryNoteId(id);
            return Ok(salesOrders);
        }

            [HttpGet("ToDeliver")]
        public IActionResult GetOrdersToDeliver(Guid customerId)
        {
            var salesOrderHeaders = _service.GetOrdersToDeliver(customerId);
            return Ok(salesOrderHeaders.OrderBy(e => e.SalesOrderNumber));
        }

        [HttpPost]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> Create(CreateHeaderRequest salesOrder)
        {
            var response = await _service.Create(salesOrder);

            if (response.Result)
                return Ok(response.Content);
            else
                return BadRequest(response);
        }

        [HttpPut("{id:guid}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> Update(Guid id, [FromBody] SalesOrderHeader salesOrder)
        {
            if (id != salesOrder.Id) return BadRequest();

            var response = await _service.Update(salesOrder);

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

        [HttpPost("Detail")]
        [SwaggerOperation("SalesOrderDetailCreate")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> AddDetail(SalesOrderDetail detail)
        {
            var response = await _service.AddDetail(detail);

            if (response.Result) return Ok();
            else return BadRequest(response.Errors);
        }

        [HttpPut("Detail/{id:guid}")]
        [SwaggerOperation("SalesOrderDetailUpdate")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> UpdateDetail(Guid id, [FromBody] SalesOrderDetail detail)
        {
            var response = await _service.UpdateDetail(detail);

            if (response.Result) return Ok();
            else return BadRequest(response.Errors);
        }

        [HttpDelete("Detail/{id:guid}")]
        [SwaggerOperation("SalesOrderDetailDelete")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> RemoveImport(Guid id)
        {
            var response = await _service.RemoveDetail(id);

            if (response.Result) return Ok();
            else return BadRequest(response.Errors);
        }
    }
}
