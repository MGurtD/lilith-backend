namespace Domain.Entities
{
    public class Operator
    {
        public Guid Id { get; set; }

        public string Name { get; set; }

        public decimal Cost { get; set; }

        public string Type { get; set; }

        public Operator(Guid id, string name, decimal cost, string type)
        {
            Id = id;
            Name = name;
            Cost = cost;
            Type = type;
        }

        public decimal CalculateCost(int minutes)
        {
            return Cost * (minutes / 60);
        }
    }
}
