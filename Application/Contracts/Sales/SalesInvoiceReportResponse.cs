using Domain.Entities.Production;

namespace Domain.Entities.Sales
{
    public record SalesInvoiceReportResponse(
        Customer Customer, Site Site, SalesInvoice Invoice, PaymentMethod PaymentMethod, List<InvoiceDetailGroup> DetailGroups, decimal Total, DateTime DueDate
    );


    public class InvoiceDetailGroup
    {
        public string Key { get; set; }
        public List<SalesInvoiceDetail> Details { get; set; }
        public decimal Total { get; set; }

        public InvoiceDetailGroup()
        {
            Key = string.Empty;
            Details = new List<SalesInvoiceDetail>();
            Total = 0;
        }
    }

}
