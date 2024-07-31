using ZatcaIntegrationSDK.HelperContracts;
using ZatcaIntegrationSDK;

namespace ZATCA_V2.ZATCA
{
    public interface IZatcaService
    {
        Task<InvoiceClearanceResponse> SendClearanceAsync(Invoice inv, Result res);
        Task<InvoiceReportingResponse> SendReportingAsync(Invoice inv, Result res);
    }

}
