using Application.Persistance;
using Application.Services;
using Domain.Entities.Purchase;
using Infrastructure.Persistance;
using Microsoft.AspNetCore.Mvc;
using System.Net;

namespace Api.Controllers.Sales
{
    [ApiController]
    [Route("api/[controller]")]
    public class PurchaseInvoiceController : ControllerBase
    {
        private readonly IPurchaseInvoiceService _service;

        public PurchaseInvoiceController(IPurchaseInvoiceService service)
        {
            _service = service;
        }

        [HttpPost]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> Create(PurchaseInvoice purchaseInvoice)
        {
            var response = await _service.Create(purchaseInvoice);

            if (response.Result)
            {
                return Ok();
            }
            else
            {
                return BadRequest(response.Errors);
            }

        }

    }
}
