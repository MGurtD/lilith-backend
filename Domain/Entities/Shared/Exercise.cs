namespace Domain.Entities
{
    public class Exercise : Entity
    {
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }

        public int PurchaseInvoiceCounter { get; set; }
        public int ReceiptCounter { get; set; }
        public int SalesOrderCounter { get; set; }
        public int SalesInvoiceCounter { get; set; }

    }
}
