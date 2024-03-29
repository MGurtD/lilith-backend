﻿using Domain.Entities.Purchase;

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
        public bool Sales { get; set; }
        public decimal Cost { get; set; } = decimal.Zero;
        public decimal Price { get; set; } = decimal.Zero;
        public bool Purchase { get; set; }
        public Guid? ReferenceFormatId { get; set; }
        public ReferenceFormat? ReferenceFormat { get; set; }
        public decimal LastCost { get; set; } = decimal.Zero;
        public decimal WorkMasterCost { get; set; } = decimal.Zero;
        public bool Production { get; set; }
        public bool IsService { get; set; }
    }
}
