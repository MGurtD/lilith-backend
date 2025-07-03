using Application.Contract;
using Application.Contracts.Sales;
using Application.Persistance;
using Application.Services.Sales;
using Domain.Entities.Production;
using Domain.Entities.Sales;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using Verifactu.Contracts;
using Verifactu;
using Microsoft.Extensions.Options;

namespace Api.Controllers.Sales
{
    [ApiController]
    [Route("api/[controller]")]
    public class VerifactuController(ISalesInvoiceService service, IOptions<AppSettings> settings) : ControllerBase
    {
        
        [HttpGet]
        public async Task<IActionResult> FindInvoices(int Month, int Year)
        {
            // Validate Month
            if (Month < 1 || Month > 12)
                return BadRequest("Month must be between 1 and 12.");

            // Validate Year
            int currentYear = DateTime.UtcNow.Year;
            if (Year < 2024 || Year > currentYear)
                return BadRequest($"Year must be between 2024 and {currentYear}.");

            // Construir la petició
            var request = new FindInvoicesRequest
            {
                EnterpriseName = "Tecniques de Mecanització del Ges S.L.",
                VatNumber = "B09680521",
                Month = Month,
                Year = Year,
            };

            // Enviar la factura a Verifactu
            var verifactuSettings = settings.Value.Verifactu!;
            var verifactuInvoiceService = new VerifactuInvoiceService(verifactuSettings.Url, verifactuSettings.Certificate.Path, verifactuSettings.Certificate.Password);
            var response = await verifactuInvoiceService.FindInvoices(request);
            return Ok(response);
        }

        #region Verifactu
        [HttpPost("{id:guid}/SendToVerifactu")]
        [SwaggerOperation("SalesInvoiceSendToVerifactu")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> SendToVerifactu(Guid id)
        {
            var response = await service.SendToVerifactu(id);
            if (response.Result) return Ok(response);
            else return BadRequest(response.Errors);
        }
                
        #endregion

    }
}
