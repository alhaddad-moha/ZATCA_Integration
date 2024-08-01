using ZATCA_V2.Models;
using ZATCA_V2.Responses.Invoices;
using ZatcaIntegrationSDK.HelperContracts;
using ZatcaIntegrationSDK;

namespace ZATCA_V2.ZATCA
{
    public interface IZatcaService
    {
        Task<IInvoiceResponse> SendInvoiceToZATCA(CompanyCredentials companyCredentials, Result res, Invoice invoice);
    }
}