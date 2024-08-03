using ZATCA_V2.Models;

namespace ZATCA_V2.Repositories.Interfaces
{
    public interface ICompanyRepository : IBaseRepository<Company>
    {
        Task<Company?> FindByTaxRegistrationNumber(string taxRegistrationNumber);
        Task<Company?> FindByCommercialRegistrationNumber(string commercialRegistrationNumber);
    }
}