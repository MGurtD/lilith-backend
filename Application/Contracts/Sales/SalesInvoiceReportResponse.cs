using Domain.Entities.Production;

namespace Domain.Entities.Sales;

public class InvoiceReportDto
{
    public required string Number { get; set; }
    public required DateTime Date { get; set; }
    public required DateTime DueDate { get; set; }
    public required decimal Total { get; set; }
    public required Customer Customer { get; set; }
    public required Site Site { get; set; }
    public required PaymentMethod PaymentMethod { get; set; }
    public List<InvoiceReportDtoDeliveryNoteOrder> Orders { get; set; } = [];
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

public class InvoiceReportDtoDeliveryNoteOrder
{
    public required string Number { get; set; }
    public required string DeliveryNoteNumber { get; set; }
    public required DateTime Date { get; set; }
    public required string CustomerNumber { get; set; }
    public required decimal Total { get; set; }
    public required List<SalesInvoiceDetail> Details { get; set; }
}