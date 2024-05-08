namespace Domain.Entities.Production
{
    public class ProductionCosts
    {
        public decimal OperatorCost { get; set; }
        public decimal MachineCost { get; set; }
        public decimal MaterialCost { get; set; }
        public decimal ExternalCost { get; set; }

        public ProductionCosts(decimal operatorCost, decimal machineCost, decimal materialCost, decimal externalCost)
        {
            OperatorCost = operatorCost;
            MachineCost = machineCost;
            MaterialCost = materialCost;
            ExternalCost = externalCost;
        }
    }
}
