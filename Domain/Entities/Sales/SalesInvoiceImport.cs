namespace Domain.Entities.Sales
{
    public class SalesInvoiceImport : Entity
    {
        public decimal BaseAmount { get; set; }
        public decimal TaxAmount { get; set; }
        public decimal NetAmount { get; set; }

        public Guid SalesInvoiceId { get; set; }
        public SalesInvoice? SalesInvoice { get; set; }

        public Guid TaxId { get; set; }
        public Tax? Tax { get; set; }
    }
}
