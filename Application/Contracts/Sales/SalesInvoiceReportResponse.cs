using Domain.Entities;
using Domain.Entities.Production;
using Domain.Entities.Sales;

namespace Application.Contracts.Sales;

public class InvoiceReportDto
{
    public required string Number { get; set; }
    public required DateTime Date { get; set; }
    public required DateTime DueDate { get; set; }
    public required decimal Total { get; set; }
    public required Customer Customer { get; set; }
    public required Site Site { get; set; }
    public required PaymentMethod PaymentMethod { get; set; }
    public required string Base64Image { get; set; }
    public List<InvoiceReportDtoDeliveryNote> DeliveryNotes { get; set; } = [];
    public List<InvoiceReportDtoTaxImport> Imports { get; set; } = [];
}

public class InvoiceReportDtoTaxImport
{
    public required string Name { get; set; }
    public required decimal Percentatge { get; set; }
    public required decimal BaseAmount { get; set; }
    public required decimal NetAmount { get; set; }
    public required decimal TaxAmount { get; set; }
}

public class InvoiceReportDtoDeliveryNote
{
    public required string Number { get; set; }
    public required DateTime Date { get; set; }
    public required string Header { get; set; }
    public required decimal Total { get; set; }
    public required List<SalesInvoiceDetail> Details { get; set; }
}