using Application.Services;
using Domain.Entities.Shared;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers.Shared
{
    [ApiController]
    [Route("api/[controller]")]
    public class LanguageController : ControllerBase
    {
        private readonly ILanguageService _languageService;
        private readonly ILocalizationService _localizationService;

        public LanguageController(ILanguageService languageService, ILocalizationService localizationService)
        {
            _languageService = languageService;
            _localizationService = localizationService;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var response = await _languageService.GetAllLanguagesAsync();
            return response.Result ? Ok(response) : BadRequest(response);
        }

        [HttpGet("{code}")]
        public async Task<IActionResult> GetByCode(string code)
        {
            var response = await _languageService.GetLanguageByCodeAsync(code);
            return response.Result ? Ok(response) : NotFound(response);
        }

        [HttpGet("default")]
        public async Task<IActionResult> GetDefault()
        {
            var response = await _languageService.GetDefaultLanguageAsync();
            return response.Result ? Ok(response) : NotFound(response);
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] Language language)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var response = await _languageService.CreateLanguageAsync(language);
            return response.Result ? CreatedAtAction(nameof(GetByCode), new { code = language.Code }, response) : Conflict(response);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(Guid id, [FromBody] Language language)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var response = await _languageService.UpdateLanguageAsync(id, language);
            return response.Result ? Ok(response) : NotFound(response);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(Guid id)
        {
            var response = await _languageService.DeleteLanguageAsync(id);
            return response.Result ? Ok(response) : NotFound(response);
        }
    }
}