using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Entities.Sales
{
    public class SalesInvoiceHash : Entity
    {
        public Guid SalesInvoiceID { get; set; }
        public SalesInvoice SalesInvoice { get; set; } = null!;
        public string IDEmisorFactura { get; set; } = string.Empty;
        public string NumSerieFactura {  get; set; } = string.Empty;
        public DateTime FechaExpedicionFactura {  get; set; } = DateTime.Now;
        public string TipoFactura {  get; set; } = string.Empty;
        public string CuotaTotal {  get; set; } = string.Empty;
        public string ImporteTotal { get; set; } = string.Empty;
        public string Huella { get; set; } = string.Empty;
        public DateTime FechaHoraHusoGenRegistro {  get; set; } = DateTime.Now;
        public string IDEmisorFacturaAnulada { get; set; } = string.Empty;
        public string NumSerieFacturaAnulada { get; set; } = string.Empty;
        public DateTime FechaExpedicionFacturaAnulada { get; set; } = DateTime.Now;
        public string Response { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public DateTime TimeStampResponse = DateTime.Now;
    }
}
