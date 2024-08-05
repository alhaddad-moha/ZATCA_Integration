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


        public async Task<Invoice?> GetBySystemInvoiceId(string invoiceId, int companyId)
        {
            return await _context.Invoices.FirstOrDefaultAsync(i =>
                i.SystemInvoiceId == invoiceId && i.CompanyId == companyId);
        }

        public async Task CreateOrUpdate(Invoice invoice)
        {
            var existingInvoice = await GetBySystemInvoiceId(invoice.SystemInvoiceId, invoice.CompanyId);
            if (existingInvoice != null)
            {
                invoice.CreatedAt = existingInvoice.CreatedAt;
                _context.Entry(existingInvoice).State = EntityState.Detached;
                invoice.Id = existingInvoice.Id;
                _context.Invoices.Update(invoice);
            }
            else
            {
                // Create new invoice
                await _context.Invoices.AddAsync(invoice);
            }

            await _context.SaveChangesAsync();
        }

        public async Task<bool> IsInvoiceSigned(string invoiceId, int companyId)
        {
            return await _context.Invoices
                .AnyAsync(i => i.SystemInvoiceId == invoiceId && i.CompanyId == companyId && i.IsSigned);
        }

        public async Task<List<Invoice>> GetAllUnsignedInvoices()
        {
            return await _context.Invoices
                .Where(i => !i.IsSigned)
                .ToListAsync();
        }

        public async Task<List<Invoice>> GetUnsignedInvoicesByCompanyId(int companyId)
        {
            return await _context.Invoices
                .Where(i => !i.IsSigned && i.CompanyId == companyId)
                .ToListAsync();
        }
    }
}