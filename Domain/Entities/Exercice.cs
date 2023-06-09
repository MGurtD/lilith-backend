namespace Domain.Entities
{
    public class Exercice : Entity
    {
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public DateTimeOffset StartDate { get; set; }
        public DateTimeOffset EndDate { get; set; }

        public int PurchaseInvoiceCounter { get; set; }

    }
}
