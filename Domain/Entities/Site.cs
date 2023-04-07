using System.Reflection.Metadata;

namespace Domain.Entities
{
    public class Site : Entity
    {
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public bool IsActive { get; set; } = true;
        public string Address { get; set; } = string.Empty;
        public string City { get; set; } = string.Empty;
        public string PostalCode { get; set; } = string.Empty;
        public string Region { get; set; } = string.Empty;
        public string Country { get; set; } = string.Empty;
        public string PhoneNumber { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string VatNumber { get; set; } = string.Empty;
        public Guid EnterpriseId { get; set; } // Required foreign key property
        public Enterprise Enterprise { get; set; } = null!; // Required reference navigation to principal
    }
}
