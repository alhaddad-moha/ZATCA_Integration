using ZATCA_V2.Models;

namespace ZATCA_V2.Repositories.Interfaces;

public interface IApiKeyRepository
{
    Task<ApiKey> Get(string key);
    Task<ApiKey> Generate(DateTime? expirationDate = null);
    Task<bool> IsValid(string key);
}