namespace Domain.Entities.Warehouse
{
    public class MaterialType : Entity
    {
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string PrimaryColor {get; set;} = string.Empty;
        public string SecondaryColor {get; set;} = string.Empty;
    }
}
