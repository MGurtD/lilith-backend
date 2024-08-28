using Application.Persistance;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers.Production
{
    [ApiController]
    [Route("api/[controller]")]
    public class ProductionCostController : ControllerBase
    {
        private readonly IUnitOfWork _unitOfWork;

        public ProductionCostController(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }
        [HttpGet]
        public async Task<IActionResult> Get()
        {
            var entities = await _unitOfWork.ProductionCosts.GetAll();
            return Ok(entities);
        }

        [HttpGet]
        [Route("GroupedByMonthAndWorkcenterType")]
        public async Task<IActionResult> GetByMonthAndWorkcenterType(DateTime startTime, DateTime endTime)
        {
            var entities = await _unitOfWork.ProductionCosts.GetAll();

            var groupedData = entities
                .Where(x => x.Date >= startTime && x.Date < endTime)
                .GroupBy(x => new { x.WorkcenterTypeName, x.Year, x.Month})
                .Select(g => new
                {
                    WorkcenterTypeName = g.Key.WorkcenterTypeName,
                    Year = g.Key.Year,
                    Month = g.Key.Month,
                    TotalTime = (g.Sum(x => x.WorkcenterTime))/60,
                    TotalCost = g.Sum(x => x.PartWorkcenterCost)
                })                
                .ToList();
            return Ok(groupedData);
        }

        [HttpGet]
        [Route("GroupedByMonthAndWorkcenter")]
        public async Task<IActionResult> GetByMonthAndWorkcenter(DateTime startTime, DateTime endTime)
        {
            var entities = await _unitOfWork.ProductionCosts.GetAll();

            var groupedData = entities
                .Where(x => x.Date >= startTime && x.Date < endTime)
                .GroupBy(x => new { x.WorkcenterName, x.WorkcenterTypeName, x.Year, x.Month })
                .Select(g => new
                {
                    WorkcenterName = g.Key.WorkcenterName,
                    WorkcenterTypeName = g.Key.WorkcenterTypeName,
                    Year = g.Key.Year,
                    Month = g.Key.Month,
                    TotalTime = (g.Sum(x => x.WorkcenterTime)) / 60,
                    TotalCost = g.Sum(x => x.PartWorkcenterCost)
                })
                .ToList();
            return Ok(groupedData);
        }

    }
}
