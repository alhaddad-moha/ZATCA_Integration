namespace ZATCA_V2.Middlewares
{
    public class RequestLoggingMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<RequestLoggingMiddleware> _logger;

        public RequestLoggingMiddleware(RequestDelegate next, ILogger<RequestLoggingMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            // Log the request body
            context.Request.EnableBuffering();
            var requestBody = await new StreamReader(context.Request.Body).ReadToEndAsync();
            context.Request.Body.Position = 0;
            _logger.LogInformation($"Incoming Request: {context.Request.Method} {context.Request.Path} {requestBody}");

            // Capture the response body
            var originalBodyStream = context.Response.Body;
            using var responseBody = new MemoryStream();
            context.Response.Body = responseBody;

            await _next(context);

            // Log the response body
            context.Response.Body.Seek(0, SeekOrigin.Begin);
            var responseBodyText = await new StreamReader(context.Response.Body).ReadToEndAsync();
            context.Response.Body.Seek(0, SeekOrigin.Begin);

            if (context.Response.StatusCode != StatusCodes.Status200OK
                && context.Response.StatusCode != StatusCodes.Status201Created
                && context.Response.StatusCode != StatusCodes.Status400BadRequest)
            { 
                _logger.LogError($"Error Response: {context.Response.StatusCode} {responseBodyText}");
            }
            else if (context.Response.StatusCode == StatusCodes.Status400BadRequest)
            {
                _logger.LogDebug($"Validation Error Response: {context.Response.StatusCode} {responseBodyText}");
            }
            else
            {
                _logger.LogInformation($"Outgoing Response: {context.Response.StatusCode} {responseBodyText}");
            }

            await responseBody.CopyToAsync(originalBodyStream);
        }
    }
}