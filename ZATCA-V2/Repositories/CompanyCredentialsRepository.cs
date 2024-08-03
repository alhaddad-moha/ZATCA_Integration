using Microsoft.EntityFrameworkCore;
using ZATCA_V2.Data;
using ZATCA_V2.Models;
using ZATCA_V2.Repositories.Interfaces;

namespace ZATCA_V2.Repositories
{
    public class CompanyCredentialsRepository : BaseRepository<CompanyCredentials>, ICompanyCredentialsRepository
    {
        private readonly DataContext _context;

        public CompanyCredentialsRepository(DataContext context) : base(context)
        {
            _context = context;
        }


        public async Task<CompanyCredentials?> GetLatestByCompanyId(int companyId)
        {
            return await _context.CompanyCredentials
                .Where(cc => cc.CompanyId == companyId)
                .OrderByDescending(cc => cc.CreatedAt)
                .FirstOrDefaultAsync();
        }
    }
}