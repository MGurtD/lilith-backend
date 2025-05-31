using SistemaFacturacion;
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

        try
        {
            // Obtener los bytes UTF-8 de la cadena
            byte[] bytes = Encoding.UTF8.GetBytes(msg);
            // Calcular SHA-256 de forma estática (disponible en .NET Core / .NET 5+)
            byte[] hashBytes = SHA256.HashData(bytes);

            // Convertir a hexadecimal en MAYÚSCULAS
            var sb = new StringBuilder(hashBytes.Length * 2);
            foreach (byte b in hashBytes)
            {
                // "X2" -> 2 dígitos hex en MAYÚSCULAS
                sb.Append(b.ToString("X2"));
            }

            // La longitud resultante siempre será 64 caracteres (32 bytes * 2 dígitos)
            return sb.ToString();
        }
        catch (Exception e)
        {
            throw new ArgumentException("Error al generar la huella SHA", e);
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
