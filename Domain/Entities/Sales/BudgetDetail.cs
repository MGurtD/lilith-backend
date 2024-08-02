using Domain.Entities.Production;
using Domain.Entities.Shared;

namespace Domain.Entities.Sales;

public class BudgetDetail : Entity
{
    public Guid BudgetId { get; set; }
    public Budget? Budget { get; set; }
    public Guid ReferenceId { get; set; }
    public Reference? Reference { get; set; }
    public Guid? WorkMasterId { get; set; }
    public WorkMaster? WorkMaster { get; set; }
    public string Description { get; set; }
    public int Quantity { get; set; }
    public decimal Profit { get; set; }
    public decimal Discount { get; set; }
    public decimal UnitCost { get; set; }
    public decimal TransportCost { get; set; }
    public decimal ServiceCost { get; set; }
    public decimal TotalCost { get; set; } 
    public decimal UnitPrice { get; set; }
    public decimal Amount { get; set; }         

    public BudgetDetail()
    {
        Description = string.Empty;
        Quantity = 0;
        Profit = decimal.Zero;
        Discount = decimal.Zero;
        TransportCost = decimal.Zero;
        ServiceCost = decimal.Zero;
        UnitCost = decimal.Zero;
        TotalCost = decimal.Zero;
        Amount = decimal.Zero;
        Disabled = false;
    }

    public void SetReference(Reference reference, int quantity)
    {
        Description = $"{reference.Code} - {reference.Description} (V.{reference.Version})";

        ReferenceId = reference.Id;
        Quantity = quantity;
        Amount = reference.Price * quantity;
    }
}
