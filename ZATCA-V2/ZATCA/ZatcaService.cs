using ZATCA_V2.Helpers;
using ZATCA_V2.Models;
using ZATCA_V2.Responses.Invoices;
using ZatcaIntegrationSDK;
using ZatcaIntegrationSDK.APIHelper;
using ZatcaIntegrationSDK.BLL;
using ZatcaIntegrationSDK.HelperContracts;
using Invoice = ZatcaIntegrationSDK.Invoice;


namespace ZATCA_V2.ZATCA
{
    public class ZatcaService : IZatcaService
    {

        public async Task<IInvoiceResponse> SendInvoiceToZATCA(CompanyCredentials companyCredentials, Result res,
            Invoice invoice)
        {
            Mode mode = Constants.DefaultMode;
            ApiRequestLogic apiRequestLogic = new ApiRequestLogic(mode);
            string invoiceType = invoice.invoiceTypeCode.Name;
            InvoiceReportingRequest invRequestBody = new InvoiceReportingRequest
            {
                invoice = res.EncodedInvoice,
                invoiceHash = res.InvoiceHash,
                uuid = res.UUID
            };

            bool isStandardInvoice = invoiceType.StartsWith("01");

            if (mode == Mode.developer)
            {
                var devResponse = await apiRequestLogic.CallComplianceInvoiceAPI(companyCredentials.SecretToken,
                    companyCredentials.Secret, invRequestBody);
                return new InvoiceReportingResponseWrapper(devResponse);
            }
            else
            {
                if (isStandardInvoice)
                {
                    var standardResponse = await apiRequestLogic.CallClearanceAPI(
                        companyCredentials.SecretToken,
                        companyCredentials.Secret, invRequestBody);
                    return new InvoiceClearanceResponseWrapper(standardResponse);
                }

                var reportingResponse = await apiRequestLogic.CallReportingAPI(companyCredentials.SecretToken,
                    companyCredentials.Secret, invRequestBody);
                return new InvoiceReportingResponseWrapper(reportingResponse);
            }
        }
    }
}