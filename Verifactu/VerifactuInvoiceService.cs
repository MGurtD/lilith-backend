using Domain.Entities.Production;
using SistemaFacturacion;
using System.Linq;
using System.Security.AccessControl;
using System.Security.Cryptography.X509Certificates;
using System.Xml.Serialization;
using Verifactu.Contracts;

namespace Verifactu;

public interface IVerifactuInvoiceService
{
    Task<RegisterInvoiceResponse> RegisterInvoice(RegisterInvoiceRequest request);
}

public class VerifactuInvoiceService : IVerifactuInvoiceService
{
    private readonly sfPortTypeVerifactuClient _client;

    public VerifactuInvoiceService(string baseUrl, string certificatePath, string certificatePassword)
    {
        if (string.IsNullOrWhiteSpace(baseUrl))
        {
            throw new ArgumentException("La URL de Verifactu no está configurada.");
        }
        if (string.IsNullOrWhiteSpace(certificatePath))
        {
            throw new ArgumentException("La ruta del certificado no está configurada.");
        }
        if (string.IsNullOrWhiteSpace(certificatePassword))
        {
            throw new ArgumentException("La contraseña del certificado no está configurada.");
        }

        _client = new sfPortTypeVerifactuClient(
            baseUrl.Contains("prewww")
                ? sfPortTypeVerifactuClient.EndpointConfiguration.SistemaVerifactuPruebas
                : sfPortTypeVerifactuClient.EndpointConfiguration.SistemaVerifactu);

        // 2.1) Asignar el certificado  
        _client.ClientCredentials.ClientCertificate.Certificate = new X509Certificate2(certificatePath, certificatePassword);

        // 2.2) Asegurar que el binding es BasicHttpBinding con modo Transport y credencial de cliente por certificado:  
        var binding = (System.ServiceModel.BasicHttpBinding)_client.Endpoint.Binding;
        binding.Security.Mode = System.ServiceModel.BasicHttpSecurityMode.Transport;
        binding.Security.Transport.ClientCredentialType = System.ServiceModel.HttpClientCredentialType.Certificate;
    }

    public async Task<FindInvoicesResponse> FindInvoices(FindInvoicesRequest inputRequest)
    {
        if (!Enum.TryParse<TipoPeriodoType>($"Item{inputRequest.Month:D2}", out var periodoEnum))
            throw new ArgumentException($"Mes inválido para el periodo: {inputRequest.Month}");
        var periodo = periodoEnum;
        var desde = new DateTime(inputRequest.Year, inputRequest.Month, 1);
        var hasta = new DateTime(inputRequest.Year, inputRequest.Month, DateTime.DaysInMonth(inputRequest.Year, inputRequest.Month));

        var request = new ConsultaFactuSistemaFacturacionType()
        {
            Cabecera = new CabeceraConsultaSf()
            {
                IDVersion = VersionType.Item10, // "1.0" de tu XML
                Item = new ObligadoEmisionConsultaType()
                {
                    NombreRazon = inputRequest.EnterpriseName,
                    NIF = inputRequest.VatNumber
                }
            },
            FiltroConsulta = new LRFiltroRegFacturacionType()
            {
                PeriodoImputacion = new PeriodoImputacionType()
                {
                    Ejercicio = inputRequest.Year.ToString(),
                    Periodo = periodo
                },
                FechaExpedicionFactura = new FechaExpedicionConsultaType()
                {
                    Item = new RangoFechaExpedicionType()
                    {
                        Desde = FormatDate(desde),
                        Hasta = FormatDate(hasta)
                    }
                },
            }
        };

        var response = await _client.ConsultaFactuSistemaFacturacionAsync(request);
        if (response.RespuestaConsultaFactuSistemaFacturacion.ResultadoConsulta == ResultadoConsultaType.SinDatos)
        {
            return new FindInvoicesResponse();
        }
        else
        {
            var invoices = response.RespuestaConsultaFactuSistemaFacturacion.RegistroRespuestaConsultaFactuSistemaFacturacion
                .Select(r => new Factura
                {
                    IDEmisorFactura = r.IDFactura.IDEmisorFactura,
                    NumSerieFactura = r.IDFactura.NumSerieFactura,
                    FechaExpedicionFactura = r.IDFactura.FechaExpedicionFactura,
                    ImporteTotal = r.DatosRegistroFacturacion.ImporteTotal,
                    TipoFactura = r.DatosRegistroFacturacion.TipoFactura.ToString(),
                    CuotaTotal = r.DatosRegistroFacturacion.CuotaTotal,
                    FechaHoraUsoRegistro = r.DatosRegistroFacturacion.FechaHoraHusoGenRegistro,
                    Huella = r.DatosRegistroFacturacion.Huella
                }).ToList();
            return new FindInvoicesResponse { Invoices = invoices };
        }
    }

    public async Task<RegisterInvoiceResponse> RegisterInvoice(RegisterInvoiceRequest request)
    {
        var response = new RegisterInvoiceResponse();
        var enterprise = request.Enterprise;
        var invoice = request.SalesInvoice;

        var peticion = new RegFactuSistemaFacturacion
        {
            Cabecera = new CabeceraType
            {
                ObligadoEmision = new PersonaFisicaJuridicaESType
                {
                    NombreRazon = enterprise.Name,
                    NIF = invoice.Site!.VatNumber
                }
            },

            RegistroFactura =
            [
                new RegistroFacturaType
                {
                    // Este "Item" puede ser un RegistroFacturacionAltaType o RegistroFacturacionAnulacionType
                    // según la WSDL. Aquí usamos el de "Alta" (RegistroAlta).
                    Item = new RegistroFacturacionAltaType
                    {
                        IDVersion = VersionType.Item10, // "1.0" de tu XML

                        IDFactura = new IDFacturaExpedidaType
                        {
                            IDEmisorFactura = invoice.Site!.VatNumber,
                            NumSerieFactura = invoice.InvoiceNumber,
                            FechaExpedicionFactura = FormatDate(invoice.InvoiceDate)
                        },

                        NombreRazonEmisor = enterprise.Name,
                        TipoFactura = ClaveTipoFacturaType.F1,
                        DescripcionOperacion = enterprise.Description,

                        // Destinatarios
                        Destinatarios =
                        [
                            new PersonaFisicaJuridicaType
                            {
                                // Para "NIF" en PersonaFisicaJuridicaType,
                                // se usa la propiedad 'Item' (object):
                                NombreRazon = invoice.CustomerTaxName,
                                Item = invoice.CustomerVatNumber
                            }
                        ],

                        // Desglose => array de DetalleType
                        Desglose =
                            invoice.SalesInvoiceImports.Select(i => new DetalleType
                            {
                                // Equivale a <ClaveRegimen>01</ClaveRegimen>
                                ClaveRegimen = IdOperacionesTrascendenciaTributariaType.Item01,
                                ClaveRegimenSpecified = true,
                                // Equivale a <CalificacionOperacion>S1</CalificacionOperacion>
                                // En el XML se ve <CalificacionOperacion>S1</CalificacionOperacion>
                                // Como el WSDL lo modela con "Item" (puede ser CalificacionOperacionType o OperacionExentaType),
                                // hacemos:
                                Item = CalificacionOperacionType.S1,
                                TipoImpositivo = FormatNumberWithLeadingZeros(((int) i.Tax!.Percentatge), 2),
                                BaseImponibleOimporteNoSujeto = FormatDecimal(i.BaseAmount),
                                CuotaRepercutida = FormatDecimal(i.TaxAmount)
                            }).ToArray()
                        ,

                        CuotaTotal = FormatDecimal(invoice.TaxAmount),
                        ImporteTotal = FormatDecimal(invoice.BaseAmount + invoice.TaxAmount),

                        // Encadenamiento con un <PrimerRegistro>S</PrimerRegistro>
                        Encadenamiento = new RegistroFacturacionAltaTypeEncadenamiento(),

                        // SistemaInformatico
                        SistemaInformatico = new SistemaInformaticoType
                        {
                            NombreRazon = enterprise.Name,
                            // De nuevo, en esta clase para el NIF se usa la propiedad 'Item'
                            Item = invoice.Site!.VatNumber,
                            NombreSistemaInformatico = "Zenith",
                            IdSistemaInformatico = "01",
                            Version = "1.0.0",
                            NumeroInstalacion = "1",
                            TipoUsoPosibleSoloVerifactu = SiNoType.N,
                            TipoUsoPosibleMultiOT = SiNoType.N,
                            IndicadorMultiplesOT = SiNoType.N
                        },

                        FechaHoraHusoGenRegistro = FormatDateTime(DateTime.Now),

                        TipoHuella = TipoHuellaType.Item01,
                    }
                }
            ]
        };

        // Generar hash de la factura
        var registroFactura = peticion.RegistroFactura[0].Item;
        if (registroFactura is RegistroFacturacionAltaType alta)
        {
            alta.Huella = GeneradorHuella.GenerarHuellaRegistroAlta(alta, request.PreviousHash);
            response.Hash = alta.Huella;

            if (request.PreviousNotificatedInvoice == null)
            {
                alta.Encadenamiento.Item = PrimerRegistroCadenaType.S; // Primer registro
            }
            else
            {
                alta.Encadenamiento.Item = new EncadenamientoFacturaAnteriorType
                {
                    NumSerieFactura = request.PreviousNotificatedInvoice.InvoiceNumber,
                    FechaExpedicionFactura = FormatDate(request.PreviousNotificatedInvoice.InvoiceDate),
                    IDEmisorFactura = request.PreviousNotificatedInvoice.VatNumber,
                    Huella = request.PreviousHash
                };
            }
        }

        // Enviar factura al sistema Verifactu
        var respuesta = await _client!.RegFactuSistemaFacturacionAsync(peticion);

        response.Timestamp = DateTime.Now;
        response.XmlRequest = SerializeToXml(peticion);
        response.XmlResponse = SerializeToXml(respuesta.RespuestaRegFactuSistemaFacturacion);
        response.Success = respuesta.RespuestaRegFactuSistemaFacturacion.RespuestaLinea[0].EstadoRegistro == EstadoRegistroType.Correcto ||
                           respuesta.RespuestaRegFactuSistemaFacturacion.RespuestaLinea[0].EstadoRegistro == EstadoRegistroType.AceptadoConErrores;
        response.StatusRegister = respuesta.RespuestaRegFactuSistemaFacturacion.RespuestaLinea[0].EstadoRegistro.ToString();

        if (!response.Success)
        {
            response.ErrorMessage = $"{respuesta.RespuestaRegFactuSistemaFacturacion.RespuestaLinea[0].CodigoErrorRegistro} - {respuesta.RespuestaRegFactuSistemaFacturacion.RespuestaLinea[0].DescripcionErrorRegistro}";
            return response;
        }

        return response;
    }
    
    public async Task CancelInvoice(RegisterInvoiceRequest request)
    {
        //var response;  TODO > Crear DTOs;
        var enterprise = request.Enterprise;
        var invoice = request.SalesInvoice;

        var peticion = new RegFactuSistemaFacturacion
        {
            Cabecera = new CabeceraType
            {
                ObligadoEmision = new PersonaFisicaJuridicaESType
                {
                    NombreRazon = enterprise.Name,
                    NIF = invoice.Site!.VatNumber
                }
            },

            RegistroFactura =
            []
        };

        // Generar hash de la factura

        // Enviar petición de cancelación al sistema Verifactu

        // Devolver respuesta
    }

    private static string FormatDateTime(DateTime dateTime)
    {
        return dateTime.ToString("yyyy-MM-ddTHH:mm:ssK");
    }

    private static string FormatDate(DateTime date)
    {
        return date.ToString("dd-MM-yyyy");
    }

    private static string FormatNumberWithLeadingZeros(int number, int totalDigits)
    {
        return number.ToString().PadLeft(totalDigits, '0');
    }

    private static string FormatDecimal(decimal value)
    {
        return value.ToString("F2", System.Globalization.CultureInfo.InvariantCulture);
    }

    private static string SerializeToXml<T>(T obj)
    {
        if (obj == null) return string.Empty;
        var xmlSerializer = new XmlSerializer(typeof(T));
        using var stringWriter = new StringWriter();
        xmlSerializer.Serialize(stringWriter, obj);
        return stringWriter.ToString();
    }
}