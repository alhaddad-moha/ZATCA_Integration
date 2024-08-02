using ZatcaIntegrationSDK.HelperContracts;

namespace ZATCA_V2.Responses.Invoices;


public class InvoiceClearanceResponseWrapper : IInvoiceResponse
{
    private readonly InvoiceClearanceResponse _response;

    public InvoiceClearanceResponseWrapper(InvoiceClearanceResponse response)
    {
        _response = response;
    }

    public bool IsSuccess => _response.IsSuccess;
    public int StatusCode => _response.StatusCode;
    public string ErrorMessage => _response.ErrorMessage;
    public string WarningMessage => _response.WarningMessage;
}
