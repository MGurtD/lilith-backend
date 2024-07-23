using System.ComponentModel.DataAnnotations;

namespace Application.Contracts.Sales
{
    public class CreateRectificativeInvoiceRequest
    {
        [Required]
        public Guid Id { get; set; }
        [Required]
        public decimal Quantity { get; set; }
    }
}
