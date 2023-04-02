namespace Domain.Entities
{
    public abstract class Entity
    {
        public Guid Id { get; private set; } = Guid.NewGuid();
        public DateTime CreatedOn { get; private set; }
        public DateTime UpdatedOn { get; private set; }
    }
}
