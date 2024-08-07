using ZATCA_V3.Models;

namespace ZATCA_V3.Repositories.Interfaces
{
    public interface IInvoiceRepository : IBaseRepository<Invoice>
    {
        Task<Invoice?> GetLatestByCompanyId(int companyId);
        Task<List<Invoice>> GetAllByCompanyId(int companyId);
        Task<List<Invoice>> GetAllUnsignedInvoices();
        Task<List<Invoice>> GetUnsignedInvoicesByCompanyId(int companyId);


        Task<Invoice?> GetBySystemInvoiceId(string invoiceId, int companyId);
        Task<bool> IsInvoiceSigned(string invoiceId, int companyId);
        Task CreateOrUpdate(Invoice invoice);
    }
}