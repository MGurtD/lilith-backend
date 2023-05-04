namespace Domain.Entities
{
    public class Role : Entity
    {
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public bool Disabled { get; set; }

    }
}
