using System.ComponentModel.DataAnnotations;

namespace Application.Contracts.Sales
{
    public class CreateInvoiceRequest
    {
        [Required]
        public Guid Id { get; set; }
        [Required]
        public DateTime InvoiceDate { get; set; }
        [Required]
        public Guid ExerciseId { get; set;}
        [Required]
        public Guid CustomerId { get; set; }
    }
}
