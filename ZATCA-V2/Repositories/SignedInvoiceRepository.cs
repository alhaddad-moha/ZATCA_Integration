﻿using Microsoft.EntityFrameworkCore;
using ZATCA_V2.Data;
using ZATCA_V2.Models;
using ZATCA_V2.Repositories.Interfaces;

namespace ZATCA_V2.Repositories
{
    public class SignedInvoiceRepository : BaseRepository<SignedInvoice>, ISignedInvoiceRepository
    {
        private readonly DataContext _context;

        public SignedInvoiceRepository(DataContext context) : base(context)
        {
            _context = context;
        }

        public async Task<SignedInvoice?> GetLatestByCompanyId(int companyId)
        {
            return await _context.SignedInvoice
                .Where(cc => cc.CompanyId == companyId)
                .OrderByDescending(cc => cc.CreatedAt)
                .FirstOrDefaultAsync();
        }

        public async Task<List<SignedInvoice>> GetAllByCompanyId(int companyId)
        {
            return await _context.SignedInvoice
                .Where(cc => cc.CompanyId == companyId)
                .OrderByDescending(cc => cc.CreatedAt)
                .ToListAsync();
        }
    }
}