namespace Domain.Entities
{
    public class Lifecycle : Entity
    {
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;

        public Guid? InitialStatusId { get; set; }

        public ICollection<Status>? Statuses { get; set; }
    }
}
