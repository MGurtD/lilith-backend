namespace Domain.Entities.Purchase
{
    public class Receipt : Entity
    {
        public string Number { get; set; } = string.Empty;
        public DateTime Date { get; set; }
        public Guid SupplierId { get; set; }
        public Supplier? Supplier { get; set; }
        public string SupplierNumber { get; set; } = string.Empty;

        public ICollection<ReceiptDetail>? Details { get; set; }
    }
}
