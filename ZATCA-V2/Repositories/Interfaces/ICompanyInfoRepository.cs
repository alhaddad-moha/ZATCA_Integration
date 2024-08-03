using ZATCA_V2.Models;

namespace ZATCA_V2.Repositories.Interfaces
{
    public interface ICompanyInfoRepository : IBaseRepository<CompanyInfo>
    {
        Task<CompanyInfo?> GetByCompanyId(int id);
    }
}