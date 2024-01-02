using Application.Services.Warehouse;
using Domain.Entities.Warehouse;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers.Warehouse
{
    [ApiController]
    [Route("api/[controller]")]

    public class StockMovementController : ControllerBase
    {
        private readonly IStockMovementService _service;

        public StockMovementController(IStockMovementService service)
        {
            _service = service;
        }

        [HttpPost]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> Create(StockMovement request)
        {
            var response = await _service.Create(request);

            if (response.Result)
                return Ok(response);
            else
                return BadRequest(response);
        }

        [HttpGet]
        public async Task<IActionResult> GetMovement(DateTime startTime, DateTime endTime)
        {
            var stockMovements =  _service.GetBetweenDates(startTime, endTime);

            if (stockMovements != null) return Ok(stockMovements);
            else return BadRequest();
        }

        /*[HttpPut("{id:guid}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> Update(Guid id, [FromBody] Stock request)
        {
            if (id != request.Id) return BadRequest();

            var response = await _service.Update(request);

            if (response.Result) return Ok(response);
            else return BadRequest(response);
        }*/

    }
}