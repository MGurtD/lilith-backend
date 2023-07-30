namespace Domain.Entities
{
    public class Lifecycle : Entity
    {
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;

        public ICollection<Status>? Statuses { get; set; }
    }
}
