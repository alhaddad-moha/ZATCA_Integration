using ZATCA_V2.Models;

namespace ZATCA_V2.Repositories.Interfaces
{
    public interface ISignedInvoiceRepository
    {
        Task<List<SignedInvoice?>> GetAll();
        Task<SignedInvoice?> GetById(int id);
        Task Create(SignedInvoice? company);
        Task Update(SignedInvoice company);
        Task Delete(int id);

        Task<SignedInvoice?> GetLatestByCompanyId(int companyId);
    }
}