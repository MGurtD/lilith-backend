using Application.Contracts.Sales;
using Application.Persistance;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers.Sales
{
    [ApiController]
    [Route("api/[controller]")]
    public class IncomeController : ControllerBase
    {
        private readonly IUnitOfWork _unitOfWork;
        public IncomeController(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }
        [HttpGet]
        [Route("Consolidated")]
        public IActionResult GetConsolidatedBetweenDates(DateTime startTime, DateTime endTime)
        {
            IEnumerable<ConsolidatedIncomes> entities = Enumerable.Empty<ConsolidatedIncomes>();
            entities = _unitOfWork.ConsolidatedIncomes.Find(c => c.Date >= startTime && c.Date <= endTime);
            return Ok(entities);
        }
    }
}
