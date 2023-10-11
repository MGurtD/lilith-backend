namespace Domain.Entities.Warehouse
{
    public class StockMovement : Entity
    {
        public Guid StockId {get; set;}
        public Stock? Stock {get; set;}
        public string MovementType {get; set;} = string.Empty;
        public int Quantity {get; set;}
        public decimal Width {get; set; }
        public decimal Length {get; set; }
        public decimal Height {get; set; }
        public DateTime MovementDate {get; set;}
        
    }
}