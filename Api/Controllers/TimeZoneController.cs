using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class TimeZoneController : ControllerBase
    {

        [HttpGet]
        public IActionResult Get()
        {
            return Ok($"DateTime.Now: {DateTime.Now}. TimeZoneInfo.Local: {TimeZoneInfo.Local}");
        }
        
    }
}
