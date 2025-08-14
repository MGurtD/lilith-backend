using Microsoft.AspNetCore.Mvc;
using Application.Services; // For ILocalizationService & ILanguageCatalog
using Application.Contracts; // GenericResponse
using Application.Contracts.Shared; // LanguageDto

namespace Api.Controllers.Shared
{
    [ApiController]
    [Route("api/[controller]")]
    public class LanguageController : ControllerBase
    {
        private readonly ILanguageCatalog _catalog;
        private readonly ILocalizationService _localizationService;

        public LanguageController(ILanguageCatalog catalog, ILocalizationService localizationService)
        {
            _catalog = catalog;
            _localizationService = localizationService;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var items = await _catalog.GetAllAsync();
            return Ok(new GenericResponse(true, items));
        }

        [HttpGet("{code}")]
        public async Task<IActionResult> GetByCode(string code)
        {
            var item = await _catalog.GetByCodeAsync(code);
            if (item == null)
            {
                var message = _localizationService.GetLocalizedString("LanguageNotFound", code);
                return NotFound(new GenericResponse(false, message));
            }
            return Ok(new GenericResponse(true, item));
        }

        [HttpGet("default")]
        public async Task<IActionResult> GetDefault()
        {
            var item = await _catalog.GetDefaultAsync();
            if (item == null)
            {
                var message = _localizationService.GetLocalizedString("DefaultLanguageNotFound");
                return NotFound(new GenericResponse(false, message));
            }
            return Ok(new GenericResponse(true, item));
        }
    }
}