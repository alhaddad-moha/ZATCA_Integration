using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ZATCA_V2.Data;
using ZATCA_V2.Models;
using ZATCA_V2.Repositories.Interfaces;

namespace ZATCA_V2.Repositories
{
    public class CompanyInfoRepository : ICompanyInfoRepository
    {
        private readonly DataContext _context;

        public CompanyInfoRepository(DataContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }

        public async Task<List<CompanyInfo>?> GetAll()
        {
            return await _context.CompanyInfos.ToListAsync();
        }

        public async Task<CompanyInfo?> GetById(int id)
        {
            return await _context.CompanyInfos.FindAsync(id);
        }
        public async Task<CompanyInfo?> GetByCompanyId(int id)
        {
            return await _context.CompanyInfos
                  .Where(ci => ci.CompanyId == id)
                  .FirstOrDefaultAsync();
        }

        public async Task Create(CompanyInfo companyInfo)
        {
            if (companyInfo == null)
            {
                throw new ArgumentNullException(nameof(companyInfo));
            }

            _context.CompanyInfos.Add(companyInfo);
            await _context.SaveChangesAsync();
        }

        public async Task Update(CompanyInfo companyInfo)
        {
            if (companyInfo == null)
            {
                throw new ArgumentNullException(nameof(companyInfo));
            }

            _context.CompanyInfos.Update(companyInfo);
            await _context.SaveChangesAsync();
        }

        public async Task Delete(int id)
           {
            var companyInfo = await _context.CompanyInfos.FindAsync(id);
            if (companyInfo != null)
            {
                _context.CompanyInfos.Remove(companyInfo);
                await _context.SaveChangesAsync();
            }
        }
    }
}