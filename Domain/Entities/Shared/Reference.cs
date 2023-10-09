namespace Domain.Entities.Shared
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
        public Guid ReferenceTypeId { get; set;}
        public ReferenceType? ReferenceType {get; set;}
        public int FormatId {get; set; }
        public decimal LastPurchaseCost {get; set;}
        public decimal Density {get; set;}
        public bool Sales { get; set; }
        public bool Purchase { get; set; }
        public bool Production { get; set; }
    }
}
