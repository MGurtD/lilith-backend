namespace Domain.Entities.Warehouse
{
    public class ProductType : Entity
    {
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string primaryColor {get; set;} = string.Empty;
        public string secondaryColor {get; set;} = string.Empty;
    }
}
