namespace ZATCA_V3.Helpers
{
    using Microsoft.Extensions.Diagnostics.HealthChecks;
    using System.Net.Http;
    using System.Threading;
    using System.Threading.Tasks;

    public class ZatcaHealthCheck : IHealthCheck
    {
     

        public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
        {
            using var httpClient = new HttpClient();
            try
            {
                var response = httpClient.GetAsync("https://sandbox.zatca.gov.sa/").Result;
                if (response.IsSuccessStatusCode)
                {
                    return HealthCheckResult.Healthy("ZATCA URL is reachable");
                }
                return HealthCheckResult.Unhealthy("ZATCA URL is unreachable");
            }
            catch (Exception ex)
            {
                return HealthCheckResult.Unhealthy("ZATCA URL is unreachable"+ex.Message);
            }
        }
    }

}
