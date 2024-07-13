using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using ZATCA_V2.Data;
using ZATCA_V2.Repositories;
using ZATCA_V2.Repositories.Interfaces;
using ZATCA_V2.ZATCA;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddHttpClient();
builder.Services.AddControllers();

builder.Logging.ClearProviders();
builder.Logging.AddConsole();

var logger = LoggerFactory.Create(logging => logging.AddConsole()).CreateLogger<Program>();


builder.Services.AddDbContext<DataContext>(options =>
{
    var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
    logger.LogInformation("Default connection string from appsettings: {ConnectionString}", connectionString);

    if (builder.Environment.IsDevelopment() == false)
    {
        logger.LogInformation("Running in a non-Development environment");

        var server = Environment.GetEnvironmentVariable("DB_SERVER");
        var database = Environment.GetEnvironmentVariable("DB_NAME");
        var user = Environment.GetEnvironmentVariable("DB_USER");
        var password = Environment.GetEnvironmentVariable("DB_PASSWORD");
        var trustedConnection = Environment.GetEnvironmentVariable("TRUSTED_CONNECTION") ?? "True";

        logger.LogInformation("Environment variables - DB_SERVER: {Server}, DB_NAME: {Database}, DB_USER: {User}, DB_PASSWORD: {Password}, TRUSTED_CONNECTION: {TrustedConnection}",
            server ?? "null", database ?? "null", user ?? "null", password ?? "null", trustedConnection);

        if (!string.IsNullOrEmpty(server) &&
            !string.IsNullOrEmpty(database) &&
            !string.IsNullOrEmpty(user) &&
            !string.IsNullOrEmpty(password))
        {
            connectionString = $"Server={server};Database={database};User Id={user};Password={password};Trusted_Connection={trustedConnection};";
            logger.LogInformation("Using environment variables for connection string: {ConnectionString}", connectionString);
        }
        else
        {
            logger.LogWarning("One or more environment variables for the database connection string are missing. Using default connection string.");
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
builder.Services.AddScoped<IExternalApiService, ExternalApiService>();

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

logger.LogInformation("Running in {Environment} environment", app.Environment.EnvironmentName);

// Configure the HTTP request pipeline.
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
