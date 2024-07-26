using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ZATCA_V2.Data;
using ZATCA_V2.Models;
using ZATCA_V2.Repositories.Interfaces;

namespace ZATCA_V2.Repositories
{
    public class SignedInvoiceRepository : ISignedInvoiceRepository
    {
        private readonly DataContext _context;

        public SignedInvoiceRepository(DataContext context)
        {
            _context = context;
        }

        public async Task<List<SignedInvoice?>> GetAll()
        {
            return await _context.SignedInvoice.ToListAsync();
        }

        public async Task<SignedInvoice?> GetById(int id)
        {
            return await _context.SignedInvoice.FirstOrDefaultAsync(c => c.Id == id);
        }

        public async Task Create(SignedInvoice? companyCredentials)
        {
            _context.SignedInvoice.Add(companyCredentials);
            await _context.SaveChangesAsync();
        }

        public async Task Update(SignedInvoice companyCredentials)
        {
            _context.Entry(companyCredentials).State = EntityState.Modified;
            await _context.SaveChangesAsync();
        }

        public async Task Delete(int id)
        {
            var companyCredentials = await _context.SignedInvoice.FindAsync(id);
            if (companyCredentials != null)
            {
                _context.SignedInvoice.Remove(companyCredentials);
                await _context.SaveChangesAsync();
            }
        }

        public async Task<SignedInvoice?> GetLatestByCompanyId(int companyId)
        {
            return await _context.SignedInvoice
                .Where(cc => cc.CompanyId == companyId)
                .OrderByDescending(cc => cc.CreatedAt)
                .FirstOrDefaultAsync();
        }

        public async Task<List<SignedInvoice?>> GetAllByCompanyId(int companyId)
        {
            return await _context.SignedInvoice
                .Where(cc => cc.CompanyId == companyId)
                .OrderByDescending(cc => cc.CreatedAt)
                .ToListAsync();
        }
    }
}