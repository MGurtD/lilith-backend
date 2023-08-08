namespace Domain.Entities.Sales
{
    public class SalesInvoiceDueDate : Entity
    {
        public DateTime DueDate { get; set; }
        public decimal Amount { get; set; }

        public Guid SalesInvoiceId { get; set; }
        public SalesInvoice? SalesInvoiceHeader { get; set; }
    }
}
