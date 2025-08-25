using Application.Contracts;
using Application.Services.Sales;
using Application.Services;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;

namespace Api.Controllers.Verifactu;

[ApiController]
[Route("api/[controller]")]
public class VerifactuController(IVerifactuIntegrationService service, ILocalizationService localizationService) : ControllerBase
{

    [HttpGet("PendingIntegration")]
    [SwaggerOperation("GetInvoicesToIntegrateWithVerifactu")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> GetPendingIntegration()
    {
        var invoices = await service.GetInvoicesToIntegrateWithVerifactu();
        return Ok(invoices);
    }

    [HttpGet("{id:guid}/Requests")]
    [SwaggerOperation("GetInvoiceRequests")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> GetInvoiceRequests(Guid id)
    {
        var response = await service.GetInvoiceRequests(id);
        if (response != null) return Ok(response);
        else return NotFound(localizationService.GetLocalizedString("VerifactuNoRequestsFound"));
    }

    [HttpGet]
    [SwaggerOperation("FindInvoicesInVerifactu")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> FindInvoices(int Month, int Year)
    {
        // Validate Month
        if (Month < 1 || Month > 12)
            return BadRequest(localizationService.GetLocalizedString("VerifactuInvalidMonth"));

        // Validate Year
        int currentYear = DateTime.UtcNow.Year;
        if (Year < 2024 || Year > currentYear)
            return BadRequest(localizationService.GetLocalizedString("VerifactuInvalidYear", currentYear));

        // Llamar al servicio para buscar las facturas
        var response = await service.FindInvoicesInVerifactu(Month, Year);
        if (response.Result) return Ok(response.Content);
        else return BadRequest(response.Errors);
    }

    [HttpPost("{id:guid}/SendToVerifactu")]
    [SwaggerOperation("SalesInvoiceSendToVerifactu")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> SendToVerifactu(Guid id)
    {
        var hasBeenIntegrated = await service.HasInvoiceBeenIntegrated(id);
        if (hasBeenIntegrated)
            return BadRequest(new GenericResponse(false, localizationService.GetLocalizedString("VerifactuInvoiceAlreadyIntegrated")));

        var response = await service.SendInvoiceToVerifactu(id);
        if (response.Result) return Ok(response);
        else return BadRequest(response.Errors);
    }

    [HttpPost("{id:guid}/RemoveFromVerifactu")]
    [SwaggerOperation("SalesInvoiceRemoveFromVerifactu")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> RemoveFromVerifactu(Guid id)
    {
        var hasBeenIntegrated = await service.HasInvoiceBeenIntegrated(id);
        if (!hasBeenIntegrated)
        {
            return BadRequest(new GenericResponse(false, localizationService.GetLocalizedString("VerifactuInvoiceNotIntegrated")));
        }

        var response = await service.RemoveInvoiceFromVerifactu(id);
        if (response.Result) return Ok(response);
        else return BadRequest(response.Errors);
    }

}
