using Domain.Entities.Production;
using Domain.Entities.Purchase;

namespace Domain.Entities.Warehouse
{
    public class Warehouse : Entity
    {
        
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public Guid SiteId { get; set; }
        public Site? Site { get; set; }

        public ICollection<Location>? Locations { get; set; }
    }
}

