using Domain.Entities.Production;
using Domain.Entities.Sales;

namespace Verifactu.Contracts;

public class RegisterInvoiceRequest
{
    public required SalesInvoice SalesInvoice { get; set; }
    public required Enterprise Enterprise { get; set; }
    public string? PreviousHash { get; set; } = null;
}
