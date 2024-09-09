using Application.Contracts.Sales;
using Application.Services.Sales;
using Domain.Entities.Sales;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;

namespace Api.Controllers.Sales
{
    [ApiController]
    [Route("api/[controller]")]
    public class BudgetController : ControllerBase
    {
        private readonly IBudgetService _service;
        public BudgetController(IBudgetService service)
        {
            _service = service;
        }

        [HttpGet("{id:guid}")]
        public async Task<IActionResult> GetById(Guid id)
        {
            var budget = await _service.GetById(id);
            if (budget == null)
            {
                return NotFound();
            }
            else { return Ok(budget); }
        }

        [HttpGet("Report/{id:guid}")]
        public async Task<IActionResult> GetSalesOrderForReport(Guid id)
        {
            var salesOrders = await _service.GetByIdForReporting(id);
            return Ok(salesOrders);
        }

        [HttpGet]
        public IActionResult GetByPeriodAndCustomer(DateTime startTime, DateTime endTime, Guid? customerId)
        {
            IEnumerable<Budget> salesOrderHeaders = new List<Budget>();
            if (customerId.HasValue)
                salesOrderHeaders = _service.GetBetweenDatesAndCustomer(startTime, endTime, customerId.Value);
            else
                salesOrderHeaders = _service.GetBetweenDates(startTime, endTime);         
            if (salesOrderHeaders != null) return Ok(salesOrderHeaders.OrderByDescending(e => e.Number));
            else return BadRequest();
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
        public async Task<IActionResult> Update(Guid id, [FromBody] Budget budget)
        {
            if (id != budget.Id) return BadRequest();

            var response = await _service.Update(budget);

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
        [SwaggerOperation("BudgetDetailCreate")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> AddDetail(BudgetDetail detail)
        {
            var response = await _service.AddDetail(detail);

            if (response.Result) return Ok();
            else return BadRequest(response.Errors);
        }

        [HttpPut("Detail/{id:guid}")]
        [SwaggerOperation("BudgetDetailUpdate")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> UpdateDetail(Guid id, [FromBody] BudgetDetail detail)
        {
            var response = await _service.UpdateDetail(detail);

            if (response.Result) return Ok();
            else return BadRequest(response.Errors);
        }

        [HttpDelete("Detail/{id:guid}")]
        [SwaggerOperation("BudgetDetailDelete")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> RemoveDetail(Guid id)
        {
            var response = await _service.RemoveDetail(id);

            if (response.Result) return Ok();
            else return BadRequest(response.Errors);
        }
    }
}
