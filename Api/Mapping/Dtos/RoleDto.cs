using System.ComponentModel.DataAnnotations;

namespace Api.Mapping.Dtos
{
    public class RoleDto
    {
        [Required]
        public Guid Id { get; set; }
        [Required]
        public string Name { get; set; } = string.Empty;
    }
}
