namespace Application.Contracts.Sales
{
    public class SalesOrderDetail : Contract
    {
        public Guid SalesOrderId { get; set; }
        public int SalesOrderNumber { get; set; }
        public DateTime SalesOrderDate { get; set; }
        public Guid CustomerId { get; set; }
        public Guid StatusId { get; set; }
        public string StatusName { get; set; } = null!;
        public Guid ReferenceId { get; set; }
        public string ReferenceCode { get; set; } = null!;
        public string ReferenceDescription { get; set; } = null!;
        public string ReferenceVersion { get; set; } = null!;
        public int Quantity { get; set; }
        public string Description { get; set; } = null!;
        public decimal Amount { get; set; }
        public bool IsInvoiced { get; set; }
        public bool IsServed { get; set; }
    }
}

