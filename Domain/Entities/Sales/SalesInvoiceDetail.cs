namespace Domain.Entities.Sales
{
    public class SalesInvoiceDetail : Entity
    {
        public Guid SalesInvoiceId { get; set; }
        public SalesInvoice? SalesInvoice { get; set; }
        
        public Guid? SalesOrderDetailId { get; set; }
        public SalesOrderDetail? SalesOrderDetail { get; set; }
        public Guid TaxId { get; set; }
        public Tax? Tax { get; set; }

        public string Description { get; set; } = string.Empty;
        public int Quantity { get; set; }
        public decimal UnitCost { get; set; } = decimal.Zero;
        public decimal UnitPrice { get; set; } = decimal.Zero;
        public decimal TotalCost { get; set; } = decimal.Zero;
        public decimal Amount { get; set; } = decimal.Zero;
        public DateTime EstimatedDeliveryDate { get; set; }

        public void SetOrderDetail(SalesOrderDetail salesOrderDetail)
        {
            SalesOrderDetailId = salesOrderDetail.Id;
            Description = salesOrderDetail.Description;
            Quantity = salesOrderDetail.Quantity;
            UnitCost = salesOrderDetail.UnitCost;
            UnitPrice = salesOrderDetail.UnitPrice == 0 ? salesOrderDetail.UnitCost : salesOrderDetail.UnitPrice;
            TotalCost = UnitCost * UnitPrice;
            Amount = salesOrderDetail.Amount;

            if (salesOrderDetail.Reference != null && salesOrderDetail.Reference!.TaxId.HasValue)
                TaxId = salesOrderDetail.Reference!.TaxId!.Value;
        }

    }
}
