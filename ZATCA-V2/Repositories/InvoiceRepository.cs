using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ZATCA_V2.Data;
using ZATCA_V2.Models;
using ZATCA_V2.Repositories.Interfaces;

namespace ZATCA_V2.Repositories
{
    public class InvoiceRepository : BaseRepository<Invoice>, IInvoiceRepository
    {
        private readonly DataContext _context;

        public InvoiceRepository(DataContext context) : base(context)
        {
            _context = context;
        }

        public async Task<Invoice?> GetLatestByCompanyId(int companyId)
        {
            return await _context.Invoices
                .Where(cc => cc.CompanyId == companyId)
                .OrderByDescending(cc => cc.CreatedAt)
                .FirstOrDefaultAsync();
        }

        public async Task<List<Invoice>> GetAllByCompanyId(int companyId)
        {
            return await _context.Invoices
                .Where(cc => cc.CompanyId == companyId)
                .OrderByDescending(cc => cc.CreatedAt)
                .ToListAsync();
        }
    }
}