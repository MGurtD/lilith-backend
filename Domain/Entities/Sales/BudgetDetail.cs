using Domain.Entities.Shared;

namespace Domain.Entities.Sales
{
    public class BudgetDetail : Entity
    {
        public Guid BudgetId { get; set; }
        public Budget? Budget { get; set; }
        public Guid ReferenceId { get; set; }
        public Reference? Reference { get; set; }
        public string Description { get; set; } = string.Empty;
        public int Quantity { get; set; }
        public decimal UnitCost { get; set; } = decimal.Zero;
        public decimal UnitPrice { get; set; } = decimal.Zero;
        public decimal TotalCost { get; set; } = decimal.Zero;
        public decimal Amount { get; set; } = decimal.Zero;

        public void SetReference(Reference reference, int quantity)
        {
            Description = $"{reference.Code} - {reference.Description} (V.{reference.Version})";

            ReferenceId = reference.Id;
            Quantity = quantity;
            UnitCost = reference.Cost;
            UnitPrice = reference.Price;
            TotalCost = reference.Cost * quantity;
            Amount = reference.Price * quantity;
        }
    }
}