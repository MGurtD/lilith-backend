using Application.Services;
using Domain.Entities.Sales;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;

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
        public IActionResult GetSalesOrders(DateTime startTime, DateTime endTime)
        {
            IEnumerable<SalesOrderHeader> salesOrderHeaders = new List<SalesOrderHeader>();
            salesOrderHeaders = _service.GetBetweenDates(startTime, endTime);
            if (salesOrderHeaders != null) return Ok(salesOrderHeaders.OrderBy(e => e.SalesOrderNumber));
            else return BadRequest();
        }

        [HttpPost]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> Create(SalesOrderHeader salesOrder)
        {
            var response = await _service.Create(salesOrder);

            if (response.Result)
                return Ok();
            else
                return BadRequest(response.Errors);
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
