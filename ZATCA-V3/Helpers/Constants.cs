using ZatcaIntegrationSDK.APIHelper;

namespace ZATCA_V3.Helpers;

public class Constants
{
    public const string KeysBasePath = "Credentials/Keys";
    public const string CsrBasePath = "Credentials/CSR";
    public const string ConfigBasePath = "Credentials/ConfigFiles";
    public const string SampleInvoicesPath = "Samples";


    public const string DigestValue =
        "YTJkM2JhYTcwZTBhZTAxOGYwODMyNzY3NTdkZDM3YzhjY2IxOTIyZDZhM2RlZGJiMGY0NDUzZWJhYWI4MDhmYg=";

    public const string X509IssuerName =
        "CN=TSZEINVOICE-SubCA-1, DC=extgazt, DC=gov, DC=local";

    public const string CertificateFilePath = "Utils/keys/cert.cer";
    public const string StandardInvoicePath = "DemoFiles/Simplified_Invoice.xml";
    public const string PkeyPath = "Utils/keys/sdk/private_key.txt";
    public const string CertPath = "Utils/keys/sdk/cert.txt";
    public const string Credentials = "Utils/keys/sdk/cred.txt";
    public const string CsrPath = "Utils/keys/taxpayer.csr";
    
    public const string DefaultInvoiceHash = "NWZlY2ViNjZmZmM4NmYzOGQ5NTI3ODZjNmQ2OTZjNzljMmRiYzIzOWRkNGU5MWI0NjcyOWQ3M2EyN2ZiNTdlOQ==";
    public const string X509SerialNumber =
        "2475382886904809774818644480820936050208702411";

    public const Mode DefaultMode = Mode.Simulation;
}