namespace ZATCA_V2.ZATCA;

public interface IExternalApiService
{
    Task<string> CallComplianceCSR(string csrData);
}