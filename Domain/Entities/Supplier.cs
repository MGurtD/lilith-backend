namespace Domain.Entities
{    
    public class Supplier : Entity
    {
        public string ComercialName { get; set; } = string.Empty;
        public string TaxName { get; set; } = string.Empty;
        public string VatNumber { get; set; } = string.Empty;
        public string Country { get; set; } = string.Empty;
        public string Region { get; set; } = string.Empty;
        public string City { get; set; } = string.Empty;
        public string PostalCode { get; set; } = string.Empty;
        public string Address { get; set; } = string.Empty;
        public string Phone { get; set; } = string.Empty;
        public string Observations { get; set; } = string.Empty;
        public bool Disabled { get; set; } = false;

        public SupplierType? Type { get; set; }
        public Guid SupplierTypeId { get; set; }

        public ICollection<SupplierContact> Contacts { get; } = new List<SupplierContact>();
    }
}
