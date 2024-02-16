using Microsoft.EntityFrameworkCore;
using ZATCA_V2.Data;
using ZATCA_V2.Repositories;
using ZATCA_V2.Repositories.Interfaces;
using ZATCA_V2.ZATCA;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddHttpClient();
builder.Services.AddControllers();
builder.Services.AddDbContext<DataContext>(options =>
{
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"));
}
);

builder.Services.AddScoped<ICompanyRepository, CompanyRepository>();
builder.Services.AddScoped<ICompanyCredentialsRepository, CompanyCredentialsRepository>();
builder.Services.AddScoped<ICompanyInfoRepository, CompanyInfoRepository>();
builder.Services.AddScoped<IExternalApiService, ExternalApiService>();


// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
