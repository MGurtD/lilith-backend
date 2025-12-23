namespace Domain.Entities.Purchase
{
    public class PurchaseInvoiceStatusTransition : Entity
    {

        public PurchaseInvoiceStatus? FromStatus { get; set; }
        public Guid FromStatusId { get; set; }

        public PurchaseInvoiceStatus? ToStatus { get; set; }
        public Guid ToStatusId { get; set; }
    }
}
