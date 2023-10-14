using Application.Services;
using Domain.Entities.Warehouse;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers.Warehouse
{
    [ApiController]
    [Route("api/[controller]")]

    public class StockController : ControllerBase
    {
        private readonly IStockService _service;

        public StockController(IStockService service)
        {
            _service = service;
        }

        [HttpPost]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> Create(Stock request)
        {
            var response = await _service.Create(request);

            if (response.Result)
                return Ok(response);
            else
                return BadRequest(response);
        }

        [HttpGet]
        public async Task<IActionResult> GetStock(Guid? locationId, Guid? referenceId)
        {
            IEnumerable<Stock> stock;
            if (locationId.HasValue)
            {
                stock = _service.GetByLocation(locationId.Value);
            }
            else if (referenceId.HasValue)
            {
                stock = _service.GetByReference(referenceId.Value);
            }
            else
            {
                stock = await _service.GetAll();
            }

            if (stock != null) return Ok(stock);
            else return BadRequest();
        }

        [HttpPut("{id:guid}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> Update(Guid id, [FromBody] Stock request)
        {
            if (id != request.Id) return BadRequest();

            var response = await _service.Update(request);

            if (response.Result) return Ok(response);
            else return BadRequest(response);
        }

    }
}