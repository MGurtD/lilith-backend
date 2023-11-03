using Domain.Entities.Production;

namespace Domain.Entities.Sales
{
    public class DeliveryNote : Entity
    {
        public string Number { get; set; } = string.Empty;
        public DateTime? DeliveryDate { get; set; }

        public Exercise? Exercise { get; set; }
        public Guid ExerciseId { get; set; }
        public Customer? Customer { get; set; }
        public Guid CustomerId { get; set; }
        public Site? Site { get; set; }
        public Guid SiteId { get; set; }
        public Status? Status { get; set; }
        public Guid StatusId { get; set; }
    }
}
