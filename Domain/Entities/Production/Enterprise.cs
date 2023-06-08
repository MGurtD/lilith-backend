namespace Domain.Entities.Production
{
    public class Enterprise : Entity
    {
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public ICollection<Site> Sites { get; } = new List<Site>();

    }
}
