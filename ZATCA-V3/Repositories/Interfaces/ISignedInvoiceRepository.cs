using ZATCA_V3.Models;

namespace ZATCA_V3.Repositories.Interfaces
{
    public interface ISignedInvoiceRepository : IBaseRepository<SignedInvoice>
    {
        Task<SignedInvoice?> GetLatestByCompanyId(int companyId);
        Task<List<SignedInvoice>> GetAllByCompanyId(int companyId);
    }
}