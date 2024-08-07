using ZatcaIntegrationSDK.HelperContracts;

namespace ZATCA_V3.Responses.Invoices;

public class InvoiceReportingResponseWrapper : IInvoiceResponse
{
    private readonly InvoiceReportingResponse _response;

    public InvoiceReportingResponseWrapper(InvoiceReportingResponse response)
    {
        _response = response;
    }

    public bool IsSuccess => _response.IsSuccess;
    public int StatusCode => _response.StatusCode;
    public string ErrorMessage => _response.ErrorMessage;
    public string WarningMessage => _response.WarningMessage;
}