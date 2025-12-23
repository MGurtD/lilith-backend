using Application.Contracts;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers.Production
{
    [ApiController]
    [Route("api/[controller]")]
    public class ProductionCostController(IUnitOfWork unitOfWork) : ControllerBase
    {
        [HttpGet]
        public async Task<IActionResult> Get()
        {
            var entities = await unitOfWork.ProductionCosts.GetAll();
            return Ok(entities);
        }

        [HttpGet]
        [Route("GroupedByMonthAndWorkcenterType")]
        public async Task<IActionResult> GetByMonthAndWorkcenterType(DateTime startTime, DateTime endTime)
        {
            var entities = await unitOfWork.ProductionCosts.FindAsync(p => p.Date >= startTime && p.Date < endTime);
            if (entities.Count == 0)
            {
                return Ok(entities);
            }

            var groupedData = entities
                .GroupBy(x => new { x.WorkcenterTypeName, x.Year, x.Month })
                .Select(g => new
                {
                    g.Key.WorkcenterTypeName,
                    g.Key.Year,
                    g.Key.Month,
                    TotalTime = (g.Sum(x => x.WorkcenterTime)) / 60,
                    TotalCost = g.Sum(x => x.PartWorkcenterCost)
                });

            return Ok(groupedData);
        }

        [HttpGet]
        [Route("GroupedByMonthAndWorkcenter")]
        public async Task<IActionResult> GetByMonthAndWorkcenter(DateTime startTime, DateTime endTime)
        {
            var entities = await unitOfWork.ProductionCosts.FindAsync(p => p.Date >= startTime && p.Date < endTime);
            if (entities.Count == 0)
            {
                return Ok(entities);
            }

            var groupedData = entities
                .GroupBy(x => new { x.WorkcenterName, x.WorkcenterTypeName, x.Year, x.Month })
                .Select(g => new
                {
                    g.Key.WorkcenterName,
                    g.Key.WorkcenterTypeName,
                    g.Key.Year,
                    g.Key.Month,
                    TotalTime = g.Sum(x => x.WorkcenterTime) / 60,
                    TotalCost = g.Sum(x => x.PartWorkcenterCost)
                });
            return Ok(groupedData);
        }

        [HttpGet]
        [Route("GroupedByMonthAndOperator")]
        public async Task<IActionResult> GroupedByMonthAndOperator(DateTime startTime, DateTime endTime)
        {
            var entities = await unitOfWork.ProductionCosts.FindAsync(p => p.Date >= startTime && p.Date < endTime);
            if (entities.Count == 0)
            {
                return Ok(entities);
            }

            var groupedData = entities
                .GroupBy(x => new { x.OperatorCode, x.OperatorName, x.Year, x.Month })
                .Select(g => new
                {
                    g.Key.OperatorCode,
                    g.Key.OperatorName,
                    g.Key.Year,
                    g.Key.Month,
                    TotalTime = g.Sum(x => x.OperatorTime) / 60,
                    TotalCost = g.Sum(x => x.PartOperatorCost)
                });
            return Ok(groupedData);
        }

    }
}
