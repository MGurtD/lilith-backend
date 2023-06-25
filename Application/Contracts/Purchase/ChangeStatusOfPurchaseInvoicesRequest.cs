using System.ComponentModel.DataAnnotations;

namespace Application.Contracts.Purchase
{
    public class ChangeStatusOfPurchaseInvoicesRequest
    {
        [Required]
        public IEnumerable<Guid> Ids { get; set; } = Enumerable.Empty<Guid>();
        [Required]
        public Guid StatusToId { get; set; }
    }
}