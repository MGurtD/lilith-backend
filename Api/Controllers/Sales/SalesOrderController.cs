using Application.Services;
using Domain.Entities.Sales;
using Microsoft.AspNetCore.Mvc;

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
    }
}
