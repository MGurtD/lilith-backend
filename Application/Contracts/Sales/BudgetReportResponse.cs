using Domain.Entities.Production;

namespace Domain.Entities.Sales
{
    public class BudgetReportResponse
    {
        public Customer? Customer { get; set; }
        public Site? Site { get; set; }
        public Budget? Budget { get; set; }
        public decimal Total { get; set; }
    }
}
