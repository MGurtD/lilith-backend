using Application.Services;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers.Shared
{
    [ApiController]
    [Route("api/[controller]")]
    public class LocalizationController(ILocalizationService localizationService) : ControllerBase
    {
        [HttpGet("test")]
        public IActionResult TestLocalization()
        {
            return Ok(new
            {
                EntityNotFound = localizationService.GetLocalizedString("EntityNotFound", "12345"),
                CustomerNotFound = localizationService.GetLocalizedString("CustomerNotFound"),
                ReferenceNotFound = localizationService.GetLocalizedString("ReferenceNotFound"),
                WorkOrderNotFound = localizationService.GetLocalizedString("WorkOrderNotFound", "WO001"),
                StatusName_Created = localizationService.GetLocalizedString("StatusNames.Created"),
                StatusName_Closed = localizationService.GetLocalizedString("StatusNames.Closed")
            });
        }

        [HttpGet("test/{culture}")]
        public IActionResult TestLocalizationWithCulture(string culture)
        {
            return Ok(new
            {
                Culture = culture,
                EntityNotFound = localizationService.GetLocalizedStringForCulture("EntityNotFound", culture, "12345"),
                CustomerNotFound = localizationService.GetLocalizedStringForCulture("CustomerNotFound", culture),
                ReferenceNotFound = localizationService.GetLocalizedStringForCulture("ReferenceNotFound", culture),
                WorkOrderNotFound = localizationService.GetLocalizedStringForCulture("WorkOrderNotFound", culture, "WO001"),
                StatusName_Created = localizationService.GetLocalizedStringForCulture("StatusNames.Created", culture),
                StatusName_Closed = localizationService.GetLocalizedStringForCulture("StatusNames.Closed", culture)
            });
        }
    }
}