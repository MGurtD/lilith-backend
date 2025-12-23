using Application.Contracts;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers.Sales
{
    [ApiController]
    [Route("api/[controller]")]
    public class IncomeController(IUnitOfWork unitOfWork) : ControllerBase
    {
        [HttpGet]
        [Route("Consolidated")]
        public IActionResult GetConsolidatedBetweenDates(DateTime startTime, DateTime endTime)
        {
            IEnumerable<ConsolidatedIncomes> entities = Enumerable.Empty<ConsolidatedIncomes>();
            entities = unitOfWork.ConsolidatedIncomes.Find(c => c.Date >= startTime && c.Date <= endTime);
            return Ok(entities);
        }
    }
}
