using Domain.Entities.Production;

namespace Domain.Entities.Sales
{
    public class SalesOrderReportResponse
    {
        public Customer? Customer { get; set; }
        public Site? Site { get; set; }
        public SalesOrderHeader? Order { get; set; }
        public decimal Total { get; set; }
    }
}
