using System.Security.Cryptography;
using System.Text;
using System.Xml.Serialization;

namespace Domain.Entities.Sales;

public class SalesInvoiceVerifactuRequest : Entity
{
    public required Guid SalesInvoiceId { get; set; }
    public SalesInvoice? SalesInvoice { get; set; }
    public required string Request { get; set; }
    public string? Response { get; set; }
    public bool? Status { get; set; }
    public DateTime? TimestampResponse { get; set; }
}

public static class SalesInvoiceToVerifactuXmlConverter
{
    public static string ConvertToXml(SalesInvoice salesInvoice)
    {
        var verifactuRequest = new VerifactuRequest
        {
            IDEmisorFactura = salesInvoice.Site!.VatNumber,
            NumSerieFactura = salesInvoice.InvoiceNumber,
            FechaExpedicionFactura = salesInvoice.InvoiceDate,
            TipoFactura = "F1",
            CuotaTotal = salesInvoice.TaxAmount.ToString(),
            ImporteTotal = salesInvoice.GrossAmount.ToString(),
            FechaHoraHusoGenRegistro = DateTime.Now,
            IDEmisorFacturaAnulada = salesInvoice.ParentSalesInvoice?.Site?.VatNumber,
            NumSerieFacturaAnulada = salesInvoice.ParentSalesInvoice?.InvoiceNumber,
            FechaExpedicionFacturaAnulada = salesInvoice.ParentSalesInvoice?.InvoiceDate
        };

        var xmlSerializer = new XmlSerializer(typeof(VerifactuRequest));
        using var stringWriter = new StringWriter();
        xmlSerializer.Serialize(stringWriter, verifactuRequest);
        return stringWriter.ToString();
    }
}

[XmlRoot("VerifactuRequest")]
public class VerifactuRequest
{
    [XmlElement("IDEmisorFactura")]
    public string IDEmisorFactura { get; set; } = string.Empty;

    [XmlElement("NumSerieFactura")]
    public string NumSerieFactura { get; set; } = string.Empty;

    [XmlElement("FechaExpedicionFactura")]
    public DateTime FechaExpedicionFactura { get; set; }

    [XmlElement("TipoFactura")]
    public string TipoFactura { get; set; } = string.Empty;

    [XmlElement("CuotaTotal")]
    public string CuotaTotal { get; set; } = string.Empty;

    [XmlElement("ImporteTotal")]
    public string ImporteTotal { get; set; } = string.Empty;

    [XmlElement("FechaHoraHusoGenRegistro")]
    public DateTime FechaHoraHusoGenRegistro { get; set; }

    [XmlElement("IDEmisorFacturaAnulada")]
    public string? IDEmisorFacturaAnulada { get; set; }
    [XmlElement("NumSerieFacturaAnulada")]
    public string? NumSerieFacturaAnulada { get; set; }
    [XmlElement("FechaExpedicionFacturaAnulada")]
    public DateTime? FechaExpedicionFacturaAnulada { get; set; }

    [XmlElement("Huella")]
    public string Huella
    {
        get
        {
            return GenerateHash();
        }
    }

    public string GenerateHash()
    {
        var inputString = new StringBuilder();
        inputString.AppendFormat("IDEmisorFactura={0}&", IDEmisorFactura);
        inputString.AppendFormat("NumSerieFactura={0}&", NumSerieFactura);
        inputString.AppendFormat("FechaExpedicionFactura={0}&", FechaExpedicionFactura);
        inputString.AppendFormat("TipoFactura={0}&", TipoFactura);
        inputString.AppendFormat("CuotaTotal={0}&", CuotaTotal);
        inputString.AppendFormat("ImporteTotal={0}&", ImporteTotal);
        inputString.AppendFormat("FechaHoraHusoGenRegistro={0}", FechaHoraHusoGenRegistro);

        var inputBytes = Encoding.UTF8.GetBytes(inputString.ToString());
        var hashBytes = SHA256.HashData(inputBytes);
        return Convert.ToBase64String(hashBytes);
    }
}