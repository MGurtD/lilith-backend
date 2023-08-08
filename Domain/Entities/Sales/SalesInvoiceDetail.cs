namespace Domain.Entities.Sales
{
    public class SalesInvoiceDetail : Entity
    {
        public Guid SalesInvoiceHeaderId { get; set; }
        public SalesInvoice? SalesInvoiceHeader { get; set; }
        public Guid ReferenceId { get; set; }
        public Reference? Reference { get; set; }
        public int Quantity { get; set; }
        public decimal UnitCost { get; set; } = decimal.Zero;
        public decimal UnitPrice { get; set; } = decimal.Zero;
        public decimal TotalCost { get; set; } = decimal.Zero;
        public decimal Amount { get; set; } = decimal.Zero;
        public DateTime EstimatedDeliveryDate { get; set; }

    }
}
