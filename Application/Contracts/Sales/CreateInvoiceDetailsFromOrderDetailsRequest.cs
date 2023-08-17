using Domain.Entities.Sales;
using System.ComponentModel.DataAnnotations;

namespace Application.Contracts.Sales
{
    public class CreateInvoiceDetailsFromOrderDetailsRequest
    {
        [Required]
        public Guid InvoiceId { get; set; }
        [Required]
        public IEnumerable<SalesOrderDetail> OrderDetails { get; set; } = Enumerable.Empty<SalesOrderDetail>();
    }
}
