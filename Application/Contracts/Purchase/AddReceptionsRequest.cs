using Domain.Entities.Purchase;
using System.ComponentModel.DataAnnotations;

namespace Application.Contracts.Purchase
{
    public class AddReceptionsRequest
    {
        [Required]
        public Guid ReceiptId { get; set; }
        [Required]
        public List<PurchaseOrderReceiptDetail> Receptions { get; set; } = [];
    }
}