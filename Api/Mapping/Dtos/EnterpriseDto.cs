using Domain.Entities;
using System.ComponentModel.DataAnnotations;

namespace Api.Mapping.Dtos
{
    public class EnterpriseDto
    {
        [Required]
        public Guid Id { get; set; }
        [Required]
        public string Name { get; set; } = string.Empty;
        [Required]
        public string Description { get; set; } = string.Empty;
        public List<SiteDto> Sites { get; set; } = new List<SiteDto>();
    }
}
