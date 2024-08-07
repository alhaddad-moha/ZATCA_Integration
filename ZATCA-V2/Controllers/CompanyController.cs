using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Threading.Tasks;
using AutoMapper;
using ZATCA_V2.DTOs;
using ZATCA_V2.Middlewares;
using ZATCA_V2.Models;
using ZATCA_V2.Repositories.Interfaces;
using ZATCA_V2.Responses;

namespace ZATCA_V2.Controllers
{
    [ServiceFilter(typeof(ApiKeyFilter))]
    [ApiController]
    [Route("api/companies")]
    public class CompanyController : ControllerBase
    {
        private readonly ICompanyRepository _companyRepository;

        private readonly IMapper _mapper;

        public CompanyController(ICompanyRepository companyRepository, IMapper mapper)
        {
            _companyRepository = companyRepository;
            _mapper = mapper;
        }

        [HttpGet]
        public async Task<IActionResult> GetAllCompanies()
        {
            var companies = await _companyRepository.GetAll();
            var companyDtos = _mapper.Map<List<CompanyDto>>(companies);
            var response = new ApiResponse<List<CompanyDto>>(200, "Got Data Successfully", companyDtos);
            return response;
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetCompanyById(int id)
        {
            var company = await _companyRepository.GetById(id);

            if (company == null)
            {
                return new ApiResponse<object>(404, "Company not found.");
            }

            var companyDtos = _mapper.Map<CompanyDto>(company);
            return new ApiResponse<CompanyDto>(200, "Got Data Successfully", companyDtos);
        }

        [HttpPost]
        public async Task<ActionResult> CreateCompany([FromBody] Company company)
        {
            await _companyRepository.Create(company);
            return CreatedAtAction(nameof(GetCompanyById), new { id = company.Id }, company);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateCompany(int id, [FromBody] Company company)
        {
            if (id != company.Id && !await _companyRepository.Exists(id))
            {
                return new ApiResponse<object>(404, "Company not found.");
            }

            await _companyRepository.Update(company);
            return new ApiResponse<object>(200, "Company updated successfully.");
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteCompany(int id)
        {
            if (!await _companyRepository.Exists(id))
            {
                return new ApiResponse<object>(404, "Company not found.");
            }

            await _companyRepository.Delete(id);
            return new ApiResponse<object>(200, "Company deleted successfully.");
        }
    }
}