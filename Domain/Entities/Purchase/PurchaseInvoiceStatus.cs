using System.ComponentModel.DataAnnotations.Schema;

namespace Domain.Entities.Purchase
{
    public class PurchaseInvoiceStatus : Entity
    {
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;

        [NotMapped]
        public IList<PurchaseInvoiceStatus>? Transitions { get; set; }
    }
}
