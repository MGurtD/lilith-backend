namespace Verifactu;
public class VerifactuSettings
{
    public string Url { get; set; } = string.Empty;

    public CertificateSettings Certificate { get; set; } = new CertificateSettings();

    public class CertificateSettings
    {
        public string Path { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
    }
}

