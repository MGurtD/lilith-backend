namespace Domain.Entities.Warehouse
{
    public class Location : Entity
    {
        public string Name {get; set;} = string.Empty;
        public string Description {get; set;} = string.Empty;
        public bool Default { get; set; } = false;

        public Warehouse? warehouse { get; set; }

        public Guid WarehouseId { get; set; }

    }
}