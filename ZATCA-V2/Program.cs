using System.Text.Json.Serialization;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using ZATCA_V2.Data;
using ZATCA_V2.Repositories;
using ZATCA_V2.Repositories.Interfaces;
using ZATCA_V2.ZATCA;
using ZATCA_V2.Helpers;
using HealthChecks.UI.Client;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using ZATCA_V2.CustomValidators;
using ZATCA_V2.Mappers;
using ZATCA_V2.Requests;
using SlackLogger;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddHttpClient();
builder.Services.AddControllers();

builder.Logging.ClearProviders();
builder.Logging.AddConsole();

var logger = LoggerFactory.Create(logging => logging.AddConsole()).CreateLogger<Program>();
builder.Logging.AddSlack(options =>
{
    options.WebhookUrl = "https://hooks.slack.com/services/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";
    options.LogLevel = LogLevel.Error;
    options.NotificationLevel = LogLevel.Warning;
    options.ApplicationName = "ZATCA";
});
builder.Services.AddDbContext<DataContext>(options =>
{
    var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
    logger.LogInformation("Default connection string from appsettings: {ConnectionString}", connectionString);

    if (!builder.Environment.IsDevelopment())
    {
        logger.LogInformation("Running in a non-Development environment");

        var server = Environment.GetEnvironmentVariable("DB_SERVER");
        var database = Environment.GetEnvironmentVariable("DB_NAME");
        var user = Environment.GetEnvironmentVariable("DB_USER");
        var password = Environment.GetEnvironmentVariable("DB_PASSWORD");
        var trustedConnection = Environment.GetEnvironmentVariable("TRUSTED_CONNECTION") ?? "True";

        logger.LogInformation(
            "Environment variables - DB_SERVER: {Server}, DB_NAME: {Database}, DB_USER: {User}, DB_PASSWORD: {Password}, TRUSTED_CONNECTION: {TrustedConnection}",
            server ?? "null", database ?? "null", user ?? "null", password ?? "null", trustedConnection);

        if (!string.IsNullOrEmpty(server) &&
            !string.IsNullOrEmpty(database) &&
            !string.IsNullOrEmpty(user) &&
            !string.IsNullOrEmpty(password))
        {
            connectionString =
                $"Server={server};Database={database};User Id={user};Password={password};Trusted_Connection={trustedConnection};";
            logger.LogInformation("Using environment variables for connection string: {ConnectionString}",
                connectionString);
        }
        else
        {
            logger.LogWarning(
                "One or more environment variables for the database connection string are missing. Using default connection string.");
        }
    }
    else
    {
        logger.LogInformation("Using default connection string from appsettings.");
    }

    options.UseSqlServer(connectionString);

    // Log the final connection string (excluding password for security)
    var loggedConnectionString = connectionString.Replace("password", "*****");
    logger.LogInformation("Final connection string used by DbContext: {ConnectionString}", loggedConnectionString);
});

builder.Services.AddScoped<ICompanyRepository, CompanyRepository>();
builder.Services.AddScoped<ICompanyCredentialsRepository, CompanyCredentialsRepository>();
builder.Services.AddScoped<ICompanyInfoRepository, CompanyInfoRepository>();
builder.Services.AddScoped<ISignedInvoiceRepository, SignedInvoiceRepository>();
builder.Services.AddScoped(typeof(IBaseRepository<>), typeof(BaseRepository<>));

builder.Services.AddScoped<IExternalApiService, ExternalApiService>();


builder.Services.AddScoped<IZatcaService, ZatcaService>();

builder.Services.AddScoped<IValidator<BulkInvoiceRequest>>(provider =>
{
    var companyRepository = provider.GetRequiredService<ICompanyRepository>();
    return new BulkInvoiceRequestValidator(companyRepository);
});

builder.Services.AddScoped<IValidator<SingleInvoiceRequest>>(provider =>
{
    var companyRepository = provider.GetRequiredService<ICompanyRepository>();
    return new SignInvoiceRequestValidator(companyRepository);
});
builder.Services.AddControllers().AddJsonOptions(options =>
{
    options.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
    options.JsonSerializerOptions.WriteIndented = true;
});

builder.Services.AddValidatorsFromAssemblyContaining<BulkInvoiceRequestValidator>();

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddAutoMapper(typeof(CompanyProfile));


// Add health checks
builder.Services.AddHealthChecks()
    .AddCheck("zatca_health_check", new ZatcaHealthCheck())
    .AddCheck<SqlHealthCheck>("sql_health_check", HealthStatus.Unhealthy);

builder.Services.AddHealthChecksUI(opt =>
    {
        opt.SetEvaluationTimeInSeconds(120); // Time in seconds between check evaluations
        opt.MaximumHistoryEntriesPerEndpoint(60); // Maximum history of checks
        opt.SetApiMaxActiveRequests(1); // API requests concurrency
        opt.AddHealthCheckEndpoint("Basic Health Check", "/health"); // Map health check endpoint
    })
    .AddInMemoryStorage();

var app = builder.Build();

app.UseHttpLogging();
app.MapHealthChecks("/health", new HealthCheckOptions
{
    Predicate = _ => true,
    ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse
});

app.MapHealthChecksUI(setup =>
{
    setup.UIPath = "/health-ui"; // URL for the health check UI
    setup.ApiPath = "/health-ui-api"; // API for the health check UI
});

logger.LogInformation("Running in {Environment} environment", app.Environment.EnvironmentName);

// Add the API key middleware
//app.UseMiddleware<ApiKeyMiddleware>();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}
else
{
    logger.LogInformation("Running in a non-Development environment");
}

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();
app.Run();