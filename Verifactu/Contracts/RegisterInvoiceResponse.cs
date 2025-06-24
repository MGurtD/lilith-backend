using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Verifactu.Contracts;

public class RegisterInvoiceResponse
{
    public bool Success { get; set; }
    public string? ErrorMessage { get; set; }
    public string Hash { get; set; } = string.Empty;
    public string XmlRequest { get; set; } = string.Empty;
    public string XmlResponse { get; set; } = string.Empty;
    public DateTime Timestamp { get; set; }
}
