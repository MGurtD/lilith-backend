using Domain.Entities.Warehouse;

namespace Domain.Entities.Sales
{
    public class Reference : Entity
    {
        public string Code { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public decimal Cost { get; set; } = decimal.Zero;
        public decimal Price { get; set; } = decimal.Zero;
        public string Version { get; set; } = string.Empty;
        public Guid TaxId { get; set; }
        public Tax? Tax { get; set; }
    }
}
