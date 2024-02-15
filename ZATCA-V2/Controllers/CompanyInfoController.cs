using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using ZATCA_V2.Models;
using ZATCA_V2.Repositories.Interfaces;

namespace ZATCA_V2.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CompanyInfoController : ControllerBase
    {
        private readonly ICompanyInfoRepository _companyInfoRepository;

        public CompanyInfoController(ICompanyInfoRepository companyInfoRepository)
        {
            _companyInfoRepository = companyInfoRepository ?? throw new ArgumentNullException(nameof(companyInfoRepository));
        }

        [HttpGet]
        public async Task<ActionResult<List<CompanyInfo>>> GetAll()
        {
            var companyInfos = await _companyInfoRepository.GetAll();
            return Ok(companyInfos);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<CompanyInfo>> GetById(string id)
        {
            var companyInfo = await _companyInfoRepository.GetById(id);

            if (companyInfo == null)
            {
                return NotFound();
            }

            return Ok(companyInfo);
        }

        [HttpPost]
        public async Task<ActionResult> Create([FromBody] CompanyInfo companyInfo)
        {
            if (companyInfo == null)
            {
                return BadRequest("CompanyInfo object is null");
            }

            await _companyInfoRepository.Create(companyInfo);

            return CreatedAtAction(nameof(GetById), new { id = companyInfo.ID }, companyInfo);
        }

        [HttpPut("{id}")]
        public async Task<ActionResult> Update(string id, [FromBody] CompanyInfo companyInfo)
        {
            if (companyInfo == null || id != companyInfo.ID)
            {
                return BadRequest("Invalid input");
            }

            var existingCompanyInfo = await _companyInfoRepository.GetById(id);

            if (existingCompanyInfo == null)
            {
                return NotFound();
            }

            existingCompanyInfo.StreetName = companyInfo.StreetName; // Update other properties as needed

            await _companyInfoRepository.Update(existingCompanyInfo);

            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<ActionResult> Delete(string id)
        {
            var companyInfo = await _companyInfoRepository.GetById(id);

            if (companyInfo == null)
            {
                return NotFound();
            }

            await _companyInfoRepository.Delete(id);

            return NoContent();
        }
    }
}
