using ZATCA_V2.Models;
using ZATCA_V2.Responses.Invoices;
using ZatcaIntegrationSDK;
using ZatcaIntegrationSDK.HelperContracts;
using Invoice = ZatcaIntegrationSDK.Invoice;

namespace ZATCA_V2.ZATCA
{
    public interface IZatcaService
    {
        Task<IInvoiceResponse> SendInvoiceToZATCA(CompanyCredentials companyCredentials, Result res, Invoice invoice);

        Task<IInvoiceResponse> ReSendInvoiceToZATCA(CompanyCredentials companyCredentials,
            InvoiceReportingRequest request, string invoiceType);
    }
}