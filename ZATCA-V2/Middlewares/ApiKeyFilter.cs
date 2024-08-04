namespace ZATCA_V2.Middlewares
{
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.AspNetCore.Mvc.Filters;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.Logging;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text.Json;

    public class ApiKeyFilter : IAsyncActionFilter
    {
        private const string ApiKeyHeaderName = "X-Api-Key";
        private readonly ILogger<ApiKeyFilter> _logger;
        private readonly List<string> _apiKeys;

        public ApiKeyFilter(IConfiguration configuration, ILogger<ApiKeyFilter> logger)
        {
            _logger = logger;

            try
            {
                _apiKeys = configuration.GetSection("ApiKeys").Get<Dictionary<string, string>>().Values.ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to load API keys from configuration.");
                throw; // Re-throw the exception to ensure that the filter setup fails if API keys are not loaded
            }
        }

        public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            try
            {
                var path = context.HttpContext.Request.Path;

                // Bypass API key check for Swagger and health check endpoints
                if (path.StartsWithSegments("/swagger") || path.StartsWithSegments("/api/health"))
                {
                    await next();
                    return;
                }

                if (!context.HttpContext.Request.Headers.TryGetValue(ApiKeyHeaderName, out var extractedApiKey))
                {
                    _logger.LogWarning("API Key was not provided.");
                    context.Result = new UnauthorizedObjectResult(new { status = 401, message = "API Key was not provided." });
                    return;
                }

                if (!_apiKeys.Contains(extractedApiKey))
                {
                    _logger.LogWarning("Unauthorized client.");
                    context.Result = new UnauthorizedObjectResult(new { status = 401, message = "Unauthorized client." });
                    return;
                }

                await next();
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, "An error occurred while processing an HTTP request.");
                context.Result = new StatusCodeResult(503);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while processing the request.");
                context.Result = new StatusCodeResult(500);
            }
        }
    }
}
