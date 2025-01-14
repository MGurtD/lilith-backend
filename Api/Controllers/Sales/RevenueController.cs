using Microsoft.AspNetCore.Mvc;
using Application.Persistance;

namespace Api.Controllers.Sales
{
    [ApiController]
    [Route("api/[controller]")]
    public class RevenueController : ControllerBase
    {
        private readonly IUnitOfWork _unitOfWork;

        public RevenueController(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }
        [HttpGet]
        public IActionResult GetMonthlyRevenue(int year, int month)
        {
            var revenue = _unitOfWork.Revenues.Find(c => c.Year == year && c.Month == month).FirstOrDefault();
            return Ok(revenue);
        }
    }
}
