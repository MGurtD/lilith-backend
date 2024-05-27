using Domain.Entities.Production;
using Domain.Entities.Shared;

namespace Domain.Entities.Sales
{
    public class SalesOrderDetail : Entity
    {
        public Guid SalesOrderHeaderId { get; set; }
        public SalesOrderHeader? SalesOrderHeader { get; set; }
        public Guid ReferenceId { get; set; }
        public Reference? Reference { get; set; }
        public Guid? WorkMasterId { get; set; }
        public WorkMaster? WorkMaster { get; set; }
        public Guid? WorkOrderId { get; set; }
        public WorkOrder? WorkOrder { get; set; }
        public string Description { get; set; }
        public int Quantity { get; set; }
        public decimal Profit { get; set; }
        public decimal Discount { get; set; }
        public decimal UnitCost { get; set; }
        public decimal TotalCost { get; set; }
        public decimal UnitPrice { get; set; }
        public decimal Amount { get; set; }
        public DateTime EstimatedDeliveryDate { get; set; }
        public bool IsDelivered { get; set; }
        // deprecated
        public decimal LastCost { get; set; }
        // deprecated
        public decimal WorkMasterCost { get; set; }

        public SalesOrderDetail() 
        {
            Description = string.Empty;
            Quantity = 0;
            Profit = decimal.Zero;
            Discount = decimal.Zero;
            LastCost = decimal.Zero;
            WorkMasterCost = decimal.Zero;
            UnitPrice = decimal.Zero;
            TotalCost = decimal.Zero;
            Amount = decimal.Zero;
            EstimatedDeliveryDate = DateTime.Now;
            Disabled = false;
            IsDelivered = false;
        }

        public SalesOrderDetail(BudgetDetail budgetDetail)
        {
            Id = Guid.NewGuid();
            CreatedOn = DateTime.Now;
            UpdatedOn = DateTime.Now;
            Description = budgetDetail.Description;
            Quantity = budgetDetail.Quantity;
            Profit = budgetDetail.Profit;
            Discount = budgetDetail.Discount;
            UnitCost = budgetDetail.UnitCost;
            TotalCost = budgetDetail.TotalCost;
            UnitPrice = budgetDetail.UnitPrice;
            Amount = budgetDetail.Amount;
            EstimatedDeliveryDate = DateTime.Now;
            Disabled = false;
            IsDelivered = false;
        }

        public void SetReference(Reference reference, int quantity)
        {
            Description = $"{reference.Code} - {reference.Description} (V.{reference.Version})";

            ReferenceId = reference.Id;
            Quantity = quantity;
            Amount = reference.Price * quantity;
        }

    }
}
