using System.Globalization;
using System.IO;
using System.Xml.Serialization;

namespace Verifactu.Utils;

/// <summary>
/// Utilidades para formatear datos específicos del sistema Verifactu
/// </summary>
public static class VerifactuFormatUtils
{
    /// <summary>
    /// Formatea una fecha y hora en el formato requerido por Verifactu (yyyy-MM-ddTHH:mm:ssK)
    /// </summary>
    /// <param name="dateTime">Fecha y hora a formatear</param>
    /// <returns>Fecha y hora formateada</returns>
    public static string FormatDateTime(DateTime dateTime)
    {
        return dateTime.ToString("yyyy-MM-ddTHH:mm:ssK");
    }

    /// <summary>
    /// Formatea una fecha en el formato requerido por Verifactu (dd-MM-yyyy)
    /// </summary>
    /// <param name="date">Fecha a formatear</param>
    /// <returns>Fecha formateada</returns>
    public static string FormatDate(DateTime date)
    {
        return date.ToString("dd-MM-yyyy");
    }

    /// <summary>
    /// Formatea un número entero con ceros a la izquierda
    /// </summary>
    /// <param name="number">Número a formatear</param>
    /// <param name="totalDigits">Número total de dígitos</param>
    /// <returns>Número formateado con ceros a la izquierda</returns>
    public static string FormatNumberWithLeadingZeros(int number, int totalDigits)
    {
        return number.ToString().PadLeft(totalDigits, '0');
    }

    /// <summary>
    /// Formatea un decimal con 2 decimales usando la cultura invariante
    /// </summary>
    /// <param name="value">Valor decimal a formatear</param>
    /// <returns>Decimal formateado</returns>
    public static string FormatDecimal(decimal value)
    {
        return value.ToString("F2", CultureInfo.InvariantCulture);
    }

    /// <summary>
    /// Serializa un objeto a XML
    /// </summary>
    /// <typeparam name="T">Tipo del objeto</typeparam>
    /// <param name="obj">Objeto a serializar</param>
    /// <returns>XML serializado o string vacío si el objeto es null</returns>
    public static string SerializeToXml<T>(T obj)
    {
        if (obj == null) return string.Empty;
        
        var xmlSerializer = new XmlSerializer(typeof(T));
        using var stringWriter = new StringWriter();
        xmlSerializer.Serialize(stringWriter, obj);
        return stringWriter.ToString();
    }
}
