using ZATCA_V3.Models;

namespace ZATCA_V3.Repositories.Interfaces;

public interface IApiKeyRepository
{
    Task<ApiKey> Get(string key);
    Task<ApiKey> Generate(DateTime? expirationDate = null);
    Task<bool> IsValid(string key);
}