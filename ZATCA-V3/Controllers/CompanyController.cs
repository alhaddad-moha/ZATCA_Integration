using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Threading.Tasks;
using AutoMapper;
using ZATCA_V3.DTOs;
using ZATCA_V3.Helpers;
using ZATCA_V3.Middlewares;
using ZATCA_V3.Models;
using ZATCA_V3.Repositories.Interfaces;
using ZATCA_V3.Responses;
using ZatcaIntegrationSDK;
using ZatcaIntegrationSDK.HelperContracts;
using Invoice = ZATCA_V3.Models.Invoice;

namespace ZATCA_V3.Controllers
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
        public async Task<IActionResult> UpdateCompany(int id, [FromBody] CompanyUpdateRequestDto companyUpdateRequest)
        {
            try
            {
                var errors = new Dictionary<string, List<string>>();

                // Check if the company exists by ID
                var existingCompany = await _companyRepository.GetById(id);
                if (existingCompany == null)
                {
                    return new ApiResponse<object>(404, "Company not found.");
                }

                var existingCompanyByTax =
                    await _companyRepository.FindByTaxRegistrationNumber(companyUpdateRequest.TaxRegistrationNumber);
                if (existingCompanyByTax != null && existingCompanyByTax.Id != id)
                {
                    if (!errors.ContainsKey(nameof(companyUpdateRequest.TaxRegistrationNumber)))
                    {
                        errors[nameof(companyUpdateRequest.TaxRegistrationNumber)] = new List<string>();
                    }

                    errors[nameof(companyUpdateRequest.TaxRegistrationNumber)]
                        .Add("Tax Registration Number already exists.");
                }

                // Check for duplicate CommercialRegistrationNumber
                var existingCompanyByCommercial =
                    await _companyRepository.FindByCommercialRegistrationNumber(companyUpdateRequest
                        .CommercialRegistrationNumber);
                if (existingCompanyByCommercial != null && existingCompanyByCommercial.Id != id)
                {
                    if (!errors.ContainsKey(nameof(companyUpdateRequest.CommercialRegistrationNumber)))
                    {
                        errors[nameof(companyUpdateRequest.CommercialRegistrationNumber)] = new List<string>();
                    }

                    errors[nameof(companyUpdateRequest.CommercialRegistrationNumber)]
                        .Add("Commercial Registration Number already exists.");
                }

                // If there are validation errors, return them
                if (errors.Any())
                {
                    return new ApiResponse<object>(400, "Validation errors occurred.", null, errors);
                }

                // Update company data without modifying CompanyCredentials
                existingCompany = Creator.UpdateCompanyData(existingCompany, companyUpdateRequest);
                await _companyRepository.Update(existingCompany);

                return new ApiResponse<object>(200, "Company updated successfully.", existingCompany);
            }
            catch (Exception ex)
            {
                return new ApiResponse<object>(500, $"An error occurred: {ex.Message}");
            }
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