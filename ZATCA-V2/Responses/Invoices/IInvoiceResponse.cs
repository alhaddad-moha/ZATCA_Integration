namespace ZATCA_V2.Responses.Invoices;

public interface IInvoiceResponse
{
    bool IsSuccess { get; }
    int StatusCode { get; }
    string ErrorMessage { get; }
    string WarningMessage { get; }
}
