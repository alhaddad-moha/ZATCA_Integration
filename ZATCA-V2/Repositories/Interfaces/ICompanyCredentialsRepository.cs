using ZATCA_V2.Models;

namespace ZATCA_V2.Repositories.Interfaces
{
    public interface ICompanyCredentialsRepository : IBaseRepository<CompanyCredentials>
    {
        Task<CompanyCredentials?> GetLatestByCompanyId(int companyId);
    }
}