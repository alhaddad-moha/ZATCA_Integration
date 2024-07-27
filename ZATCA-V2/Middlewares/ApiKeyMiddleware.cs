namespace ZATCA_V2.Middlewares
{
    using Microsoft.AspNetCore.Http;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.Logging;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text.Json;
    using System.Threading.Tasks;

    public class ApiKeyMiddleware
    {
        private readonly RequestDelegate _next;
        private const string ApiKeyHeaderName = "X-Api-Key";
        private readonly ILogger<ApiKeyMiddleware> _logger;
        private readonly List<string> _apiKeys;

        public ApiKeyMiddleware(RequestDelegate next, IConfiguration configuration, ILogger<ApiKeyMiddleware> logger)
        {
            _next = next;
            _logger = logger;

            try
            {
                _apiKeys = configuration.GetSection("ApiKeys").Get<Dictionary<string, string>>().Values.ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to load API keys from configuration.");
                throw; // Re-throw the exception to ensure that the middleware setup fails if API keys are not loaded
            }
        }

        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                var path = context.Request.Path;

                // Bypass API key check for Swagger and health check endpoints
                if (path.StartsWithSegments("/swagger") || path.StartsWithSegments("/api/health"))
                {
                    await _next(context);
                    return;
                }

                if (!context.Request.Headers.TryGetValue(ApiKeyHeaderName, out var extractedApiKey))
                {
                    _logger.LogWarning("API Key was not provided.");
                    await WriteJsonResponseAsync(context, 401, "API Key was not provided.");
                    return;
                }

                if (!_apiKeys.Contains(extractedApiKey))
                {
                    _logger.LogWarning("Unauthorized client.");
                    await WriteJsonResponseAsync(context, 401, "Unauthorized client.");
                    return;
                }

                await _next(context);
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, "An error occurred while processing an HTTP request.");
                await WriteJsonResponseAsync(context, 503, "A service error occurred. Please try again later.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while processing the request.");
                await WriteJsonResponseAsync(context, 500, "An unexpected error occurred.");
            }
        }

        private async Task WriteJsonResponseAsync(HttpContext context, int statusCode, string message)
        {
            context.Response.ContentType = "application/json";
            context.Response.StatusCode = statusCode;

            var response = new
            {
                status = statusCode,
                message
            };

            await context.Response.WriteAsync(JsonSerializer.Serialize(response));
        }
    }
}
