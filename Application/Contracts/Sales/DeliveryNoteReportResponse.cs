using Domain.Entities.Production;

namespace Domain.Entities.Sales
{
    public class DeliveryNoteReportResponse
    {
        public Customer? Customer { get; set; }
        public Site? Site { get; set; }
        public DeliveryNote? DeliveryNote { get; set; }
        public IList<DeliveryNoteOrderReportDto>? Orders { get; set; }
        public decimal Total { get; set; }
    }

    public class DeliveryNoteOrderReportDto
    {
        public required string Number { get; set; }
        public required DateTime Date { get; set; }
        public required string CustomerNumber { get; set; }
        public required decimal Total { get; set; }
        public required List<SalesOrderDetail> Details { get; set; }
    }
}
