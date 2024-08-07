using ZATCA_V3.Models;
using ZATCA_V3.Responses.Invoices;
using ZatcaIntegrationSDK;
using ZatcaIntegrationSDK.HelperContracts;
using Invoice = ZatcaIntegrationSDK.Invoice;

namespace ZATCA_V3.ZATCA
{
    public interface IZatcaService
    {
        Task<IInvoiceResponse> SendInvoiceToZATCA(CompanyCredentials companyCredentials, Result res, Invoice invoice);
        Task<InvoiceReportingResponse> SendSimplifiedInvoiceToZATCA(CompanyCredentials companyCredentials, Result res, Invoice invoice);

        Task<IInvoiceResponse> ReSendInvoiceToZATCA(CompanyCredentials companyCredentials,
            InvoiceReportingRequest request, string invoiceType);
    }
}