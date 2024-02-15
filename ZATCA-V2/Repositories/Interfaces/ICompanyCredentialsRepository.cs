using ZATCA_V2.Models;

namespace ZATCA_V2.Repositories.Interfaces
{
    public interface ICompanyCredentialsRepository
    {
        Task<List<CompanyCredentials>> GetAll();
        Task<CompanyCredentials> GetById(int id);
        Task Create(CompanyCredentials company);
        Task Update(CompanyCredentials company);
        Task Delete(int id);
    }
}