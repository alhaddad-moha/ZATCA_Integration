using Microsoft.EntityFrameworkCore;
using ZATCA_V2.Data;
using ZATCA_V2.Models;
using ZATCA_V2.Repositories.Interfaces;

namespace ZATCA_V2.Repositories
{
    public class CompanyRepository:ICompanyRepository
    {
            private readonly DataContext _context;

            public CompanyRepository(DataContext context)
            {
                _context = context;
            }

            public async Task<List<Company>> GetAll()
            {
                return await _context.Companies.Include(c => c.CompanyCredentials).ToListAsync();
            }

            public async Task<Company?> GetById(int id)
            {
                return await _context.Companies.Include(c => c.CompanyCredentials).FirstOrDefaultAsync(c => c.Id == id);
            }

            public async Task Create(Company company)
            {
                _context.Companies.Add(company);
                await _context.SaveChangesAsync();
            }

            public async Task Update(Company company)
            {
                _context.Entry(company).State = EntityState.Modified;
                await _context.SaveChangesAsync();
            }

            public async Task Delete(int id)
            {
                var company = await _context.Companies.FindAsync(id);
                if (company != null)
                {
                    _context.Companies.Remove(company);
                    await _context.SaveChangesAsync();
                }
            }
        }
    }

