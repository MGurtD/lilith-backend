namespace Domain.Entities
{
    public class Operator
    {
        public Guid Id { get; set; }

        public string Name { get; set; }

        public decimal Cost { get; set; }

        public Operator(Guid id, string name, decimal cost)
        {
            Id = id;
            Name = name;
            Cost = cost;
        }

        public decimal CalculateCost(int minutes)
        {
            return Cost * (minutes / 60);
        }
    }
}
