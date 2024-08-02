using ZATCA_V2.Models;

namespace ZATCA_V2.Repositories.Interfaces
{
    public interface ICompanyRepository
    {
        Task<List<Company>> GetAll();
        Task<Company?> GetById(int id);
        Task<Company?> FindByTaxRegistrationNumber(string taxRegistrationNumber);
        Task<Company?> FindByCommercialRegistrationNumber(string commercialRegistrationNumber);

        Task Create(Company company);
        Task Update(Company company);
        Task Delete(int id);
        
    }
}
