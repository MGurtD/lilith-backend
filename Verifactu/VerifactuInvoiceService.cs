using Domain.Entities.Production;
using SistemaFacturacion;
using System.Linq;
using System.Security.AccessControl;
using System.Security.Cryptography.X509Certificates;
using System.Xml.Serialization;
using Verifactu.Contracts;
using Verifactu.Factories;
using Verifactu.Mappers;
using Verifactu.Utils;

namespace Verifactu;

public interface IVerifactuInvoiceService
{
    Task<FindInvoicesResponse> FindInvoices(FindInvoicesRequest inputRequest);
    Task<RegisterInvoiceResponse> RegisterInvoice(RegisterInvoiceRequest request);
    
}

public class VerifactuInvoiceService : IVerifactuInvoiceService
{
    private readonly sfPortTypeVerifactuClient _client;

    public VerifactuInvoiceService(string baseUrl, string certificatePath, string certificatePassword)
    {
        ValidateConstructorParameters(baseUrl, certificatePath, certificatePassword);
        
        _client = CreateAndConfigureClient(baseUrl, certificatePath, certificatePassword);
    }

    private static void ValidateConstructorParameters(string baseUrl, string certificatePath, string certificatePassword)
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
    }

    private static sfPortTypeVerifactuClient CreateAndConfigureClient(string baseUrl, string certificatePath, string certificatePassword)
    {
        var client = new sfPortTypeVerifactuClient(
            baseUrl.Contains(VerifactuConstants.TestEndpointIdentifier)
                ? sfPortTypeVerifactuClient.EndpointConfiguration.SistemaVerifactuPruebas
                : sfPortTypeVerifactuClient.EndpointConfiguration.SistemaVerifactu);

        // Configurar certificado
        client.ClientCredentials.ClientCertificate.Certificate = new X509Certificate2(certificatePath, certificatePassword);

        // Configurar binding para seguridad de transporte
        var binding = (System.ServiceModel.BasicHttpBinding)client.Endpoint.Binding;
        binding.Security.Mode = System.ServiceModel.BasicHttpSecurityMode.Transport;
        binding.Security.Transport.ClientCredentialType = System.ServiceModel.HttpClientCredentialType.Certificate;

        return client;
    }

    public async Task<FindInvoicesResponse> FindInvoices(FindInvoicesRequest inputRequest)
    {
        var request = VerifactuRequestFactory.CreateFindInvoicesRequest(inputRequest);
        var response = await _client.ConsultaFactuSistemaFacturacionAsync(request);
        
        return VerifactuResponseMapper.MapFindInvoicesResponse(response);
    }

    public async Task<RegisterInvoiceResponse> RegisterInvoice(RegisterInvoiceRequest request)
    {
        var peticion = VerifactuRequestFactory.CreateRegisterInvoiceRequest(request);
        
        // Configurar encadenamiento y generar hash
        var registroFactura = peticion.RegistroFactura[0].Item;
        if (registroFactura is RegistroFacturacionAltaType alta)
        {
            ConfigureChaining(alta, request);
            var hash = GeneradorHuella.GenerarHuellaRegistroAlta(alta, request.PreviousHash);
            alta.Huella = hash;
            
            var response = await _client.RegFactuSistemaFacturacionAsync(peticion);
            return VerifactuResponseMapper.MapRegisterInvoiceResponse(response, peticion, hash);
        }
        
        throw new InvalidOperationException("Error al procesar el registro de factura");
    }

    private static void ConfigureChaining(RegistroFacturacionAltaType alta, RegisterInvoiceRequest request)
    {
        if (request.PreviousNotificatedInvoice == null)
        {
            alta.Encadenamiento.Item = PrimerRegistroCadenaType.S;
        }
        else
        {
            alta.Encadenamiento.Item = new EncadenamientoFacturaAnteriorType
            {
                NumSerieFactura = request.PreviousNotificatedInvoice.InvoiceNumber,
                FechaExpedicionFactura = VerifactuFormatUtils.FormatDate(request.PreviousNotificatedInvoice.InvoiceDate),
                IDEmisorFactura = request.PreviousNotificatedInvoice.VatNumber,
                Huella = request.PreviousHash
            };
        }
    }
    
    public Task CancelInvoice(RegisterInvoiceRequest request)
    {
        // TODO: Implementar cancelación de factura
        throw new NotImplementedException("La cancelación de facturas no ha sido implementada aún");
    }
}