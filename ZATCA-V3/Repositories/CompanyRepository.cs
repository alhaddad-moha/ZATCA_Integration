using Microsoft.EntityFrameworkCore;
using ZATCA_V3.Data;
using ZATCA_V3.Models;
using ZATCA_V3.Repositories.Interfaces;

namespace ZATCA_V3.Repositories
{
    public class CompanyRepository : BaseRepository<Company>, ICompanyRepository
    {
        private readonly DataContext _context;

        public CompanyRepository(DataContext context) : base(context)
        {
            _context = context;
        }


        public async Task<Company?> FindByTaxRegistrationNumber(string taxRegistrationNumber)
        {
            return await _context.Companies.FirstOrDefaultAsync(c => c.TaxRegistrationNumber == taxRegistrationNumber);
        }

        public async Task<Company?> FindByCommercialRegistrationNumber(string commercialRegistrationNumber)
        {
            return await _context.Companies.FirstOrDefaultAsync(c =>
                c.CommercialRegistrationNumber == commercialRegistrationNumber);
        }
    }
}