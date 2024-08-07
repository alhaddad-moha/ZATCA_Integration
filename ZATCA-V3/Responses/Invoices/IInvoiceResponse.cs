namespace ZATCA_V3.Responses.Invoices;

public interface IInvoiceResponse
{
    bool IsSuccess { get; }
    int StatusCode { get; }
    string ErrorMessage { get; }
    string WarningMessage { get; }
}
