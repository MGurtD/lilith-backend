using System.ComponentModel.DataAnnotations;

namespace Application.Dtos
{
    public class UserDto
    {
        [Required]
        public Guid Id { get; set; }

        [Required]
        public string Username { get; set; } = string.Empty;

        [Required]
        public string FirstName { get; set; } = string.Empty;

        [Required]
        public string LastName { get; set; } = string.Empty;

        [Required]
        public bool Disabled { get; set; } = false;

        [Required]
        public Guid RoleId { get; set; }

    }
}
