using System.ComponentModel.DataAnnotations;

namespace Api.Mapping.Dtos
{
    public class SiteDto
    {
        [Required]
        public Guid Id { get; set; }
        [Required]
        public string Name { get; set; } = string.Empty;
        [Required]
        public string Description { get; set; } = string.Empty;
        [Required]
        public string Address { get; set; } = string.Empty;
        [Required]
        public string City { get; set; } = string.Empty;
        [Required]
        public string PostalCode { get; set; } = string.Empty;
        [Required]
        public string Region { get; set; } = string.Empty;
        [Required]
        public string Country { get; set; } = string.Empty;
        [Required,Phone]
        public string PhoneNumber { get; set; } = string.Empty;
        [Required,EmailAddress]
        public string Email { get; set; } = string.Empty;
        [Required]
        public string VatNumber { get; set; } = string.Empty;
        [Required]
        public Guid EnterpriseId { get; set; }
    }
}
