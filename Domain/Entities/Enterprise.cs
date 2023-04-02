namespace Domain.Entities
{
    public class Enterprise : Entity
    {
        public string Name { get; private set; } = string.Empty;
        public string Description { get; private set; } = string.Empty;
        public bool IsActive { get; set; }

    }
}
