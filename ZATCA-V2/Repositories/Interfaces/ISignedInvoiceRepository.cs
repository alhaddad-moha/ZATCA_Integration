using ZATCA_V2.Models;

namespace ZATCA_V2.Repositories.Interfaces
{
    public interface ISignedInvoiceRepository : IBaseRepository<SignedInvoice>
    {
        Task<SignedInvoice?> GetLatestByCompanyId(int companyId);
        Task<List<SignedInvoice>> GetAllByCompanyId(int companyId);
    }
}