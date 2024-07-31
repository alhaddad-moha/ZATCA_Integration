using ZatcaIntegrationSDK.BLL;
using ZatcaIntegrationSDK.HelperContracts;
using ZatcaIntegrationSDK;

namespace ZATCA_V2.ZATCA
{
  
    public class ZatcaService : IZatcaService
    {
        private readonly ApiRequestLogic _apiRequestLogic;

        public ZatcaService(ApiRequestLogic apiRequestLogic)
        {
            _apiRequestLogic = apiRequestLogic;
        }

        public async Task<InvoiceClearanceResponse> SendClearanceAsync(Invoice inv, Result res)
        {

            var invRequestBody = new InvoiceReportingRequest
            {
                invoice = res.EncodedInvoice,
                invoiceHash = res.InvoiceHash,
                uuid = res.UUID
            };

            var secretKey = "lHntHtEGWi+ZJtssv167Dy+R64uxf/PTMXg3CEGYfvM=";
            return _apiRequestLogic.CallClearanceAPI(Utility.ToBase64Encode(inv.cSIDInfo.CertPem), secretKey, invRequestBody);

           
        }

        public async Task<InvoiceReportingResponse> SendReportingAsync(Invoice inv, Result res)
        {

            var invRequestBody = new InvoiceReportingRequest
            {
                invoice = res.EncodedInvoice,
                invoiceHash = res.InvoiceHash,
                uuid = res.UUID
            };

          
                var secretKey = "lHntHtEGWi+ZJtssv167Dy+R64uxf/PTMXg3CEGYfvM=";
return _apiRequestLogic.CallReportingAPI(Utility.ToBase64Encode(inv.cSIDInfo.CertPem), secretKey, invRequestBody);
               
            }
        

        private void DetermineMode()
        {
            // Logic to determine mode
        }
    }

}
