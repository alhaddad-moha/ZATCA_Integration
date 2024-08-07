using ZATCA_V3.Models;

namespace ZATCA_V3.Repositories.Interfaces
{
    public interface ICompanyCredentialsRepository : IBaseRepository<CompanyCredentials>
    {
        Task<CompanyCredentials?> GetLatestByCompanyId(int companyId);
    }
}