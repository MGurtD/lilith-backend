namespace Domain.Entities.Purchase
{
    public class PurchaseInvoice : Entity
    {
        public int Number { get; set; }
        public string SupplierNumber { get; set; } = string.Empty;
        public DateTime PurchaseInvoiceDate { get; set; }

        public decimal BaseAmount { get; set; }
        public decimal TransportAmount { get; set; }
        public decimal Subtotal { get; set; }
        public decimal TaxAmount { get; set; }
        public decimal GrossAmount { get; set; }
        public decimal NetAmount { get; set; }
        public decimal DiscountPercentage { get; set; }
        public decimal DiscountAmount { get; set; }

        public Supplier? Supplier { get; set; }
        public Guid SupplierId { get; set; }

        public Tax? Tax { get; set; }
        public Guid? TaxId { get; set; }

        public Exercise? Exercice { get; set; }
        public Guid? ExerciceId { get; set; }

        public PurchaseInvoiceSerie? PurchaseInvoiceSerie { get; set; }
        public Guid? PurchaseInvoiceSerieId { get; set; }

        public PaymentMethod? PaymentMethod { get; set; }
        public Guid PaymentMethodId { get; set; }

        public PurchaseInvoiceStatus? PurchaseInvoiceStatus { get; set; }
        public Guid PurchaseInvoiceStatusId { get; set; }
    }

}
