namespace ZATCA_V3.ZATCA;

public interface IExternalApiService
{
    Task<string> CallComplianceCSR(string csrData);
}