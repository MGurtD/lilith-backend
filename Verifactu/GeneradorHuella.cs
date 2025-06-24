using SistemaFacturacion;
using System.Drawing;
using System.Net;
using System.Security.Cryptography;
using System.Text;

namespace Verifactu;

public static class GeneradorHuella
{

    /// <summary>
    /// Construye la referencia de registro para alta concatenando pares "nombre=valor" separados por '&'.
    /// </summary>
    public static string GenerarHuellaRegistroAlta(RegistroFacturacionAltaType alta, string? huellaAnterior)
    {
        var sb = new StringBuilder();
        sb
            .Append(GetValorCampo("IDEmisorFactura", alta.IDFactura.IDEmisorFactura, true))
            .Append(GetValorCampo("NumSerieFactura", alta.IDFactura.NumSerieFactura, true))
            .Append(GetValorCampo("FechaExpedicionFactura", alta.IDFactura.FechaExpedicionFactura, true))
            .Append(GetValorCampo("TipoFactura", alta.TipoFactura.ToString(), true))
            .Append(GetValorCampo("CuotaTotal", alta.CuotaTotal, true))
            .Append(GetValorCampo("ImporteTotal", alta.ImporteTotal, true))
            .Append(GetValorCampo("Huella", huellaAnterior, true))
            .Append(GetValorCampo("FechaHoraUsoRegistro", alta.FechaHoraHusoGenRegistro, false));

        return GetHashVerifFactu(sb.ToString());
    }

    /// <summary>
    /// Construye la referencia de registro para alta concatenando pares "nombre=valor" separados por '&'.
    /// </summary>
    public static string GenerarHuellaRegistroAnulacion(RegistroFacturacionAnulacionType anulacion, string? huellaAnterior)
    {
        var sb = new StringBuilder();
        sb
            .Append(GetValorCampo("IDEmisorFacturaAnulada", anulacion.IDFactura.IDEmisorFacturaAnulada, true))
            .Append(GetValorCampo("NumSerieFacturaAnulada", anulacion.IDFactura.NumSerieFacturaAnulada, true))
            .Append(GetValorCampo("FechaExpedicionFacturaAnulada", anulacion.IDFactura.FechaExpedicionFacturaAnulada, true))
            .Append(GetValorCampo("Huella", huellaAnterior, true))
            .Append(GetValorCampo("FechaHoraUsoRegistro", anulacion.FechaHoraHusoGenRegistro, false));

        return GetHashVerifFactu(sb.ToString());
    }

    /// <summary>
    /// Genera el hash SHA-256 de la cadena de texto dada y lo devuelve en
    /// formato hexadecimal en MAYÚSCULAS (64 caracteres: 32 bytes * 2 dígitos).
    /// </summary>
    public static string GetHashVerifFactu(string msg)
    {
        ArgumentNullException.ThrowIfNull(msg);

        using (SHA256 sha256 = SHA256.Create())
        {
            byte[] bytes = Encoding.UTF8.GetBytes(msg);
            byte[] hashBytes = sha256.ComputeHash(bytes);

            // Convertir el hash a una cadena hexadecimal
            StringBuilder hash = new StringBuilder();
            foreach (byte b in hashBytes)
            {
                hash.Append(b.ToString("x2"));
            }

            return hash.ToString().ToUpper();
        }
    }


    /// <summary>
    /// Devuelve "nombre=valor" y agrega '&' al final si separador == true.
    /// </summary>
    public static string GetValorCampo(string nombre, string? valor, bool separador)
    {
        var raw = string.IsNullOrEmpty(valor) ? string.Empty : valor.Trim();
        string campo = $"{nombre}={raw}";
        return separador ? campo + "&" : campo;
    }

    /// <summary>
    /// Devuelve "nombre=valorEncoded" y agrega '&' al final si separador == true. Utiliza UTF-8 para el URL encoding.
    /// </summary>
    public static string GetValorCampoEncoded(string nombre, string valor, bool separador)
    {
        // En .NET Core/Standard, WebUtility.UrlEncode aplica UTF-8 por defecto.
        string encoded = valor == null ? string.Empty : WebUtility.UrlEncode(valor);
        string campo = $"{nombre}={encoded}";
        return separador ? campo + "&" : campo;
    }

}
