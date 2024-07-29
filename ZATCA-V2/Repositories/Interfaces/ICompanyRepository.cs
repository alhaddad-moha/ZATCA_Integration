using ZATCA_V2.Models;

namespace ZATCA_V2.Repositories.Interfaces
{
    public interface ICompanyRepository
    {
        Task<List<Company>> GetAll();
        Task<Company?> GetById(int id);
        Task Create(Company company);
        Task Update(Company company);
        Task Delete(int id);
    }
}
