using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ZATCA_V2.Data;
using ZATCA_V2.Models;
using ZATCA_V2.Repositories.Interfaces;

namespace ZATCA_V2.Repositories
{
    public class CompanyCredentialsRepository : ICompanyCredentialsRepository
    {
        private readonly DataContext _context;

        public CompanyCredentialsRepository(DataContext context)
        {
            _context = context;
        }

        public async Task<List<CompanyCredentials>> GetAll()
        {
            return await _context.CompanyCredentials.ToListAsync();
        }

        public async Task<CompanyCredentials> GetById(int id)
        {
            return await _context.CompanyCredentials.FirstOrDefaultAsync(c => c.Id == id);
        }

        public async Task Create(CompanyCredentials companyCredentials)
        {
            _context.CompanyCredentials.Add(companyCredentials);
            await _context.SaveChangesAsync();
        }

        public async Task Update(CompanyCredentials companyCredentials)
        {
            _context.Entry(companyCredentials).State = EntityState.Modified;
            await _context.SaveChangesAsync();
        }

        public async Task Delete(int id)
        {
            var companyCredentials = await _context.CompanyCredentials.FindAsync(id);
            if (companyCredentials != null)
            {
                _context.CompanyCredentials.Remove(companyCredentials);
                await _context.SaveChangesAsync();
            }
        }
    }
}