using System.ComponentModel.DataAnnotations;

namespace Domain.Entities.Shared
{
    public class Language : Entity
    {
        [Required]
        [MaxLength(10)]
        public string Code { get; set; } = string.Empty; // ISO 639-1 code (e.g., "ca", "es", "en")
        
        [Required]
        [MaxLength(100)]
        public string Name { get; set; } = string.Empty; // Display name
        
        [MaxLength(255)]
        public string Icon { get; set; } = string.Empty; // Icon path or CSS class
        
        public bool IsDefault { get; set; } = false;
        
        public int SortOrder { get; set; } = 0;
    }
}