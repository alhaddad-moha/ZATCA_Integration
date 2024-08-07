using ZATCA_V3.Helpers;
using ZATCA_V3.Models;
using ZATCA_V3.Responses.Invoices;
using ZatcaIntegrationSDK;
using ZatcaIntegrationSDK.APIHelper;
using ZatcaIntegrationSDK.BLL;
using ZatcaIntegrationSDK.HelperContracts;
using Invoice = ZatcaIntegrationSDK.Invoice;


namespace ZATCA_V3.ZATCA
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

        public async Task<IInvoiceResponse> ReSendInvoiceToZATCA(CompanyCredentials companyCredentials,
            InvoiceReportingRequest invRequestBody, string invoiceType)
        {
            Mode mode = Constants.DefaultMode;
            ApiRequestLogic apiRequestLogic = new ApiRequestLogic(mode);

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