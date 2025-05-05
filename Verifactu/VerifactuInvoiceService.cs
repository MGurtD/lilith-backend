using Domain.Entities.Production;
using Domain.Entities.Sales;
using Microsoft.Extensions.Options;
using SistemaFacturacion;
using System.Security.Cryptography.X509Certificates;

namespace Verifactu;

public interface IVerifactuInvoiceService
{
    Task<RespuestaRegFactuSistemaFacturacionType> RegisterInvoice(Enterprise enterprise, SalesInvoice invoice);
}

public class VerifactuInvoiceService : IVerifactuInvoiceService
{
    private readonly VerifactuSettings _settings;
    private readonly sfPortTypeVerifactuClient _client;

    public VerifactuInvoiceService(IOptions<VerifactuSettings> settings)
    {
        _settings = settings.Value;
        if (string.IsNullOrWhiteSpace(_settings.Url))
        {
            throw new ArgumentException("La URL de Verifactu no está configurada.");
        }
        if (string.IsNullOrWhiteSpace(_settings.Certificate.Path))
        {
            throw new ArgumentException("La ruta del certificado no está configurada.");
        }
        if (string.IsNullOrWhiteSpace(_settings.Certificate.Password))
        {
            throw new ArgumentException("La contraseña del certificado no está configurada.");
        }

        _client = new sfPortTypeVerifactuClient(sfPortTypeVerifactuClient.EndpointConfiguration.SistemaVerifactuPruebas);

        // 2.1) Asignar el certificado  
        _client.ClientCredentials.ClientCertificate.Certificate = new X509Certificate2(_settings.Certificate.Path, _settings.Certificate.Password);

        // 2.2) Asegurar que el binding es BasicHttpBinding con modo Transport y credencial de cliente por certificado:  
        var binding = (System.ServiceModel.BasicHttpBinding)_client.Endpoint.Binding;
        binding.Security.Mode = System.ServiceModel.BasicHttpSecurityMode.Transport;
        binding.Security.Transport.ClientCredentialType = System.ServiceModel.HttpClientCredentialType.Certificate;
    }

    public async Task<RespuestaRegFactuSistemaFacturacionType> RegisterInvoice(Enterprise enterprise, SalesInvoice invoice)
    {
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
                            FechaExpedicionFactura = invoice.InvoiceDate.ToString("dd-MM-yyyy")
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
                                TipoImpositivo = i.Tax!.Percentatge.ToString("F2"),
                                BaseImponibleOimporteNoSujeto = i.BaseAmount.ToString("F2"),
                                CuotaRepercutida = i.TaxAmount.ToString("F2")
                            }).ToArray()
                        ,

                        CuotaTotal = invoice.TaxAmount.ToString("F2"),
                        ImporteTotal = (invoice.NetAmount + invoice.TaxAmount).ToString("F2"),

                        // Encadenamiento con un <PrimerRegistro>S</PrimerRegistro>
                        Encadenamiento = new RegistroFacturacionAltaTypeEncadenamiento
                        {
                            // El WSDL define una propiedad "Item"
                            // que puede ser "PrimerRegistro" o "RegistroAnterior".
                            // "PrimerRegistro" se mapea al enum "PrimerRegistroCadenaType.S"
                            Item = PrimerRegistroCadenaType.S
                        },

                        // SistemaInformatico
                        SistemaInformatico = new SistemaInformaticoType
                        {
                            NombreRazon = enterprise.Name,
                            // De nuevo, en esta clase para el NIF se usa la propiedad 'Item'
                            Item = invoice.Site !.VatNumber,
                            NombreSistemaInformatico = enterprise.Description,
                            IdSistemaInformatico = "10",
                            Version = "1.0",
                            NumeroInstalacion = "1",
                            TipoUsoPosibleSoloVerifactu = SiNoType.S,
                            TipoUsoPosibleMultiOT = SiNoType.N,
                            IndicadorMultiplesOT = SiNoType.N
                        },

                        FechaHoraHusoGenRegistro = DateTime.Now.ToString("yyyy-MM-ddTHH:mm:ssK"),

                        TipoHuella = TipoHuellaType.Item01, // "01"
                        Huella = "DE49BAAB8058292DEA7CD8ABA7A397AAC17EF28EB212FAA619C636068F70A7E8"
                    }
                }
            ]
        };

        // 4. Invocar el método asíncrono "RegFactuSistemaFacturacionAsync"
        var response = await _client!.RegFactuSistemaFacturacionAsync(peticion);


        // 5. Procesar la respuesta
        return response.RespuestaRegFactuSistemaFacturacion;
    }
}