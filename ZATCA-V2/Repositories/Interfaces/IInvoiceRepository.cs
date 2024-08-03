using ZATCA_V2.Models;

namespace ZATCA_V2.Repositories.Interfaces
{
    public interface IInvoiceRepository : IBaseRepository<Invoice>
    {
        Task<Invoice?> GetLatestByCompanyId(int companyId);
        Task<List<Invoice>> GetAllByCompanyId(int companyId);
    }
}