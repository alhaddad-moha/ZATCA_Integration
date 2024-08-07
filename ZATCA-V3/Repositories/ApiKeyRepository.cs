using Microsoft.EntityFrameworkCore;
using ZATCA_V3.Data;
using ZATCA_V3.Models;
using ZATCA_V3.Repositories.Interfaces;

namespace ZATCA_V3.Repositories;

public class ApiKeyRepository : IApiKeyRepository
{
    private readonly DataContext _context;

    public ApiKeyRepository(DataContext context)
    {
        _context = context;
    }

    public async Task<ApiKey> Get(string key)
    {
        return await _context.ApiKeys.FirstOrDefaultAsync(k => k.Key == key);
    }

    public async Task<ApiKey> Generate(DateTime? expirationDate = null)
    {
        var apiKey = new ApiKey
        {
            Key = Guid.NewGuid().ToString(), // Generate a new unique key
            CreatedAt = DateTime.UtcNow,
            ExpirationDate = expirationDate,
            IsActive = true
        };

        _context.ApiKeys.Add(apiKey);
        await _context.SaveChangesAsync();

        return apiKey;
    }

    public async Task<bool> IsValid(string key)
    {
        var apiKey = await Get(key);
        if (!apiKey.IsActive)
        {
            return false;
        }

        if (apiKey.ExpirationDate.HasValue && apiKey.ExpirationDate.Value < DateTime.UtcNow)
        {
            return false;
        }

        return true;
    }
}