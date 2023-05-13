using System.ComponentModel.DataAnnotations;

namespace Api.Mapping.Dtos
{
    public class SupplierTypeDto
    {
        [Required]
        public Guid Id { get; set; }
        [Required]
        public string Name { get; set; } = string.Empty;
        [Required]
        public string Description { get; set; } = string.Empty;
    }
}
