namespace Domain.Entities
{
    public class Exercice : Entity
    {
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }

        public int PurchaseInvoiceCounter { get; set; }

    }
}
