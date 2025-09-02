
namespace Verifactu.Contracts;

public class FindInvoicesResponse
{
    public List<Factura> Invoices { get; set; } = [];
}

public class Factura
{
    public string NumSerieFactura { get; set; } = string.Empty;
    public string FechaExpedicionFactura { get; set; } = string.Empty;
    public string ImporteTotal { get; internal set; }
    public string IDEmisorFactura { get; internal set; }
    public string Huella { get; internal set; }
    public string TipoFactura { get; internal set; }
    public string CuotaTotal { get; internal set; }
    public DateTime FechaHoraUsoRegistro { get; internal set; }
}