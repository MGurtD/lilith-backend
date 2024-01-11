using Domain.Entities.Production;

namespace Domain.Entities.Sales
{
    public class DeliveryNoteReportResponse
    {
        public Customer Customer { get; set; }
        public Site Site { get; set; }
        public DeliveryNote DeliveryNote { get; set; }
        public IList<SalesOrderHeader> Orders { get; set; }
        public decimal Total { get; set; }
    }
}
