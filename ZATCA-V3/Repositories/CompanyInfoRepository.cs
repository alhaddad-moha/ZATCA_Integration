using Microsoft.EntityFrameworkCore;
using ZATCA_V3.Data;
using ZATCA_V3.Models;
using ZATCA_V3.Repositories.Interfaces;

namespace ZATCA_V3.Repositories
{
    public class CompanyInfoRepository : BaseRepository<CompanyInfo>, ICompanyInfoRepository
    {
        private readonly DataContext _context;

        public CompanyInfoRepository(DataContext context) : base(context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }

        public async Task<CompanyInfo?> GetByCompanyId(int id)
        {
            return await _context.CompanyInfos
                .Where(ci => ci.CompanyId == id)
                .FirstOrDefaultAsync();
        }
    }
}