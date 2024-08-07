using System.Text;

namespace ZATCA_V3.ZATCA;

public class ExternalApiService : IExternalApiService
{
    private readonly IHttpClientFactory _httpClientFactory;

    public ExternalApiService(IHttpClientFactory httpClientFactory)
    {
        _httpClientFactory = httpClientFactory ?? throw new ArgumentNullException(nameof(httpClientFactory));
    }

    public async Task<string> CallComplianceCSR(string csrData)
    {
        using (var client = _httpClientFactory.CreateClient())
        {
            var request = new HttpRequestMessage(HttpMethod.Post,
                "https://gw-fatoora.zatca.gov.sa/e-invoicing/developer-portal/compliance");

            request.Headers.Add("accept", "application/json");
            request.Headers.Add("OTP", "123345");
            request.Headers.Add("Accept-Version", "V2");

            // Set the CSR data in the request content
            request.Content = new StringContent($"{{ \"csr\": \"{csrData}\" }}", Encoding.UTF8, "application/json");

            // Send the request and await the response
            var response = await client.SendAsync(request);

            // Ensure a successful status code
            response.EnsureSuccessStatusCode();

            // Read the response content
            return await response.Content.ReadAsStringAsync();
        }
    }
}

