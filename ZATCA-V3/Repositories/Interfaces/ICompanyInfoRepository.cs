using ZATCA_V3.Models;

namespace ZATCA_V3.Repositories.Interfaces
{
    public interface ICompanyInfoRepository : IBaseRepository<CompanyInfo>
    {
        Task<CompanyInfo?> GetByCompanyId(int id);
    }
}