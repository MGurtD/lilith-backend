using System.ComponentModel.DataAnnotations;

namespace Application.Contracts.Purchase
{
    public class ChangeStatusOfPurchaseInvoiceRequest
    {
        [Required]
        public Guid Id { get; set; }
        [Required]
        public Guid StatusToId { get; set; }
    }
}