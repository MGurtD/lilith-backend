namespace Domain.Entities.Production
{
    public class ProductionCosts
    {
        public decimal OperatorCost { get; set; }
        public decimal MachineCost { get; set; }
        public decimal MaterialCost { get; set; }
        public decimal ExternalServiceCost { get; set; }
        public decimal ExternalTransportCost { get; set; }


        public ProductionCosts(decimal operatorCost, decimal machineCost, decimal materialCost, decimal externalServiceCost, decimal externalTransportCost)
        {
            OperatorCost = operatorCost;
            MachineCost = machineCost;
            MaterialCost = materialCost;
            ExternalServiceCost = externalServiceCost;
            ExternalTransportCost = externalTransportCost;
        }

        public decimal TotalCost()
        {
            return OperatorCost + MachineCost + MaterialCost + ExternalServiceCost + ExternalTransportCost;
        }
    }
}
