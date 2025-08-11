using Application.Contracts;
using Application.Services;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers.Shared
{
    [ApiController]
    [Route("api/[controller]")]
    public class LocalizationController : ControllerBase
    {
        private readonly ILocalizationService _localizationService;

        public LocalizationController(ILocalizationService localizationService)
        {
            _localizationService = localizationService;
        }

        [HttpGet("test")]
        public IActionResult TestLocalization()
        {
            return Ok(new
            {
                EntityNotFound = _localizationService.GetLocalizedString("EntityNotFound", "12345"),
                CustomerNotFound = _localizationService.GetLocalizedString("CustomerNotFound"),
                ReferenceNotFound = _localizationService.GetLocalizedString("ReferenceNotFound"),
                WorkOrderNotFound = _localizationService.GetLocalizedString("WorkOrderNotFound", "WO001"),
                StatusName_Created = _localizationService.GetLocalizedString("StatusNames.Created"),
                StatusName_Closed = _localizationService.GetLocalizedString("StatusNames.Closed")
            });
        }

        [HttpGet("test/{culture}")]
        public IActionResult TestLocalizationWithCulture(string culture)
        {
            return Ok(new
            {
                Culture = culture,
                EntityNotFound = _localizationService.GetLocalizedStringForCulture("EntityNotFound", culture, "12345"),
                CustomerNotFound = _localizationService.GetLocalizedStringForCulture("CustomerNotFound", culture),
                ReferenceNotFound = _localizationService.GetLocalizedStringForCulture("ReferenceNotFound", culture),
                WorkOrderNotFound = _localizationService.GetLocalizedStringForCulture("WorkOrderNotFound", culture, "WO001"),
                StatusName_Created = _localizationService.GetLocalizedStringForCulture("StatusNames.Created", culture),
                StatusName_Closed = _localizationService.GetLocalizedStringForCulture("StatusNames.Closed", culture)
            });
        }
    }
}