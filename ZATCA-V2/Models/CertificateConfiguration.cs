namespace ZATCA_V2.Models;
public class CertificateConfiguration
{
    public string C { get; set; }
    public string OU { get; set; }
    public string O { get; set; }
    public string CN { get; set; }
    public string UID { get; set; }
    public string Title { get; set; }
    public string RegisteredAddress { get; set; }
    public string BusinessCategory { get; set; }
    public string EmailAddress { get; set; }

    // Additional properties specific to the generated file
    public string CertificateTemplateName => "PREZATCA-code-Signing";
    public string SN => "1-Device|2-234|3-mod";
}