using Microsoft.AspNetCore.Hosting.Server.Features;

namespace ZATCA_V2.Middlewares;
public class LogUrlsMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<LogUrlsMiddleware> _logger;

    public LogUrlsMiddleware(RequestDelegate next, ILogger<LogUrlsMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var serverAddressesFeature = context.Features.Get<IServerAddressesFeature>();
        if (serverAddressesFeature != null)
        {
            foreach (var address in serverAddressesFeature.Addresses)
            {
                _logger.LogInformation("Application started at {Address}", address);
            }
        }

        // Call the next middleware in the pipeline
        await _next(context);
    }
}
