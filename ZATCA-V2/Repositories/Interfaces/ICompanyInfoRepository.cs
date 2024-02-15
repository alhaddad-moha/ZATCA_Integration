﻿using System.Collections.Generic;
using System.Threading.Tasks;
using ZATCA_V2.Models;

namespace ZATCA_V2.Repositories.Interfaces
{
    public interface ICompanyInfoRepository
    {
        Task<List<CompanyInfo>?> GetAll();
        Task<CompanyInfo?> GetById(string id);
        Task Create(CompanyInfo companyInfo);
        Task Update(CompanyInfo companyInfo);
        Task Delete(string id);
    }
}