using ZATCA_V3.Models;

namespace ZATCA_V3.Repositories.Interfaces
{
    public interface ICompanyRepository : IBaseRepository<Company>
    {
        Task<Company?> FindByTaxRegistrationNumber(string taxRegistrationNumber);
        Task<Company?> FindByCommercialRegistrationNumber(string commercialRegistrationNumber);
    }
}