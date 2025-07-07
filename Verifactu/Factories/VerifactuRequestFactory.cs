using SistemaFacturacion;
using Verifactu.Contracts;
using Verifactu.Utils;

namespace Verifactu.Factories;

/// <summary>
/// Factory para crear solicitudes del sistema Verifactu
/// </summary>
public static class VerifactuRequestFactory
{
    /// <summary>
    /// Crea una solicitud de consulta de facturas
    /// </summary>
    /// <param name="request">Datos de la solicitud</param>
    /// <returns>Solicitud de consulta formateada</returns>
    public static ConsultaFactuSistemaFacturacionType CreateFindInvoicesRequest(FindInvoicesRequest request)
    {
        if (!Enum.TryParse<TipoPeriodoType>($"{VerifactuConstants.PeriodPrefix}{request.Month:D2}", out var periodo))
            throw new ArgumentException($"Mes inválido para el periodo: {request.Month}");

        var fechaDesde = new DateTime(request.Year, request.Month, 1);
        var fechaHasta = new DateTime(request.Year, request.Month, DateTime.DaysInMonth(request.Year, request.Month));

        return new ConsultaFactuSistemaFacturacionType
        {
            Cabecera = new CabeceraConsultaSf
            {
                IDVersion = VersionType.Item10,
                Item = new ObligadoEmisionConsultaType
                {
                    NombreRazon = request.EnterpriseName,
                    NIF = request.VatNumber
                }
            },
            FiltroConsulta = new LRFiltroRegFacturacionType
            {
                PeriodoImputacion = new PeriodoImputacionType
                {
                    Ejercicio = request.Year.ToString(),
                    Periodo = periodo
                },
                FechaExpedicionFactura = new FechaExpedicionConsultaType
                {
                    Item = new RangoFechaExpedicionType
                    {
                        Desde = VerifactuFormatUtils.FormatDate(fechaDesde),
                        Hasta = VerifactuFormatUtils.FormatDate(fechaHasta)
                    }
                }
            }
        };
    }

    /// <summary>
    /// Crea una solicitud de registro de factura
    /// </summary>
    /// <param name="request">Datos de la solicitud</param>
    /// <returns>Solicitud de registro formateada</returns>
    public static RegFactuSistemaFacturacion CreateRegisterInvoiceRequest(RegisterInvoiceRequest request)
    {
        var enterprise = request.Enterprise;
        var invoice = request.SalesInvoice;

        var registroAlta = new RegistroFacturacionAltaType
        {
            IDVersion = VersionType.Item10,
            IDFactura = new IDFacturaExpedidaType
            {
                IDEmisorFactura = invoice.Site!.VatNumber,
                NumSerieFactura = invoice.InvoiceNumber,
                FechaExpedicionFactura = VerifactuFormatUtils.FormatDate(invoice.InvoiceDate)
            },
            NombreRazonEmisor = enterprise.Name,
            TipoFactura = ClaveTipoFacturaType.F1,
            DescripcionOperacion = enterprise.Description,
            Destinatarios = [
                new PersonaFisicaJuridicaType
                {
                    NombreRazon = invoice.CustomerTaxName,
                    Item = invoice.CustomerVatNumber
                }
            ],
            Desglose = invoice.SalesInvoiceImports.Select(i => new DetalleType
            {
                ClaveRegimen = IdOperacionesTrascendenciaTributariaType.Item01,
                ClaveRegimenSpecified = true,
                Item = CalificacionOperacionType.S1,
                TipoImpositivo = VerifactuFormatUtils.FormatNumberWithLeadingZeros((int)i.Tax!.Percentatge, 2),
                BaseImponibleOimporteNoSujeto = VerifactuFormatUtils.FormatDecimal(i.BaseAmount),
                CuotaRepercutida = VerifactuFormatUtils.FormatDecimal(i.TaxAmount)
            }).ToArray(),
            CuotaTotal = VerifactuFormatUtils.FormatDecimal(invoice.TaxAmount),
            ImporteTotal = VerifactuFormatUtils.FormatDecimal(invoice.BaseAmount + invoice.TaxAmount),
            Encadenamiento = new RegistroFacturacionAltaTypeEncadenamiento(),
            SistemaInformatico = new SistemaInformaticoType
            {
                NombreRazon = enterprise.Name,
                Item = invoice.Site!.VatNumber,
                NombreSistemaInformatico = VerifactuConstants.SystemName,
                IdSistemaInformatico = VerifactuConstants.SystemId,
                Version = VerifactuConstants.SystemVersion,
                NumeroInstalacion = VerifactuConstants.InstallationNumber,
                TipoUsoPosibleSoloVerifactu = SiNoType.N,
                TipoUsoPosibleMultiOT = SiNoType.N,
                IndicadorMultiplesOT = SiNoType.N
            },
            FechaHoraHusoGenRegistro = VerifactuFormatUtils.FormatDateTime(DateTime.Now),
            TipoHuella = TipoHuellaType.Item01
        };

        return new RegFactuSistemaFacturacion
        {
            Cabecera = new CabeceraType
            {
                ObligadoEmision = new PersonaFisicaJuridicaESType
                {
                    NombreRazon = enterprise.Name,
                    NIF = invoice.Site!.VatNumber
                }
            },
            RegistroFactura = [
                new RegistroFacturaType
                {
                    Item = registroAlta
                }
            ]
        };
    }
}
