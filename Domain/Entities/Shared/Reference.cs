using Domain.Entities.Purchase;
using Domain.Entities.Sales;

namespace Domain.Entities.Shared
{
    public class Reference : Entity
    {
        public string Code { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string Version { get; set; } = string.Empty;
        public Guid? TaxId { get; set; }
        public Tax? Tax { get; set; }
        public Guid? ReferenceTypeId { get; set;}
        public ReferenceType? ReferenceType { get; set;}
        public Guid? CustomerId { get; set;}
        public Customer? Customer { get; set;}
        // deprecated
        public decimal Cost { get; set; } = decimal.Zero;
        public decimal Price { get; set; } = decimal.Zero;
        public bool Sales { get; set; }
        public bool Purchase { get; set; }
        public bool Production { get; set; }
        public bool IsService { get; set; }
        public Guid? ReferenceFormatId { get; set; }
        public ReferenceFormat? ReferenceFormat { get; set; }
        public decimal LastCost { get; set; } = decimal.Zero;
        public decimal WorkMasterCost { get; set; } = decimal.Zero;
    }
}
