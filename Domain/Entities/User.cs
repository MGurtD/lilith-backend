namespace Domain.Entities
{
    public class User : Entity
    {
        public string Username { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public bool Disabled { get; set; } = true;

        public ICollection<Role> Roles { get; set; } = new HashSet<Role>();

    }
}
