﻿using System.Text;
using System.Text.Json;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using ZATCA_V2.Helpers;
using ZATCA_V2.Models;
using ZATCA_V2.Repositories;
using ZATCA_V2.Repositories.Interfaces;
using ZATCA_V2.ZATCA;

namespace ZATCA_V2.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ConfigController : ControllerBase
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly ICompanyRepository _companyRepository;
        private readonly ICompanyCredentialsRepository _companyCredentialsRepository;
        private readonly IExternalApiService _externalApiService;


        public ConfigController(IHttpClientFactory httpClientFactory, ICompanyRepository companyRepository,
            ICompanyCredentialsRepository companyCredentialsRepository
            , IExternalApiService externalApiService)

        {
            _httpClientFactory = httpClientFactory ?? throw new ArgumentNullException(nameof(httpClientFactory));
            _companyRepository = companyRepository ?? throw new ArgumentNullException(nameof(companyRepository));
            _companyCredentialsRepository = companyCredentialsRepository ??
                                            throw new ArgumentNullException(nameof(companyCredentialsRepository));
            _externalApiService = externalApiService ?? throw new ArgumentNullException(nameof(externalApiService));
        }

        [HttpPost("generateCSR/{companyId}")]
        public async Task<IActionResult> GenerateCsr(int companyId)
        {
            var certificateConfig = new CertificateConfiguration
            {
                C = "SA",
                OU = "Wosul",
                O = "Wosul SA",
                CN = "Wosul-1",
                UID = "311111111101113",
                Title = "1100",
                RegisteredAddress = "Riyadh",
                BusinessCategory = "Technology",
                EmailAddress = "mohammed@wosul.sa"
            };

            string configFilePath = $"{Constants.ConfigBasePath}/configuration{companyId}.cnf";

            Creator.WriteConfigurationFile(configFilePath, certificateConfig);
            return Ok("Generated Successfully");
        }

        [HttpPost("generateKeys/{companyId}")]
        public async Task<IActionResult> GenerateKeys(int companyId,
            CertificateConfiguration certificateConfiguration)
        {
            try
            {
                string configFilePath = CreateConfigFile(certificateConfiguration, companyId);
                string privateKeyPath = $"{Constants.KeysBasePath}/privateKey{companyId}.pem";
                string filteredPrivateKeyPath = $"{Constants.KeysBasePath}/privateKey{companyId}-filtered.pem";
                string publicKeyPath = $"{Constants.KeysBasePath}/publicKey{companyId}.pem";
                string csrPath = $"{Constants.CsrBasePath}/taxpayer{companyId}.csr";
                string filteredCsrPath = $"{Constants.CsrBasePath}/taxpayer{companyId}-filtered.txt";

                string generatePrivateKeyCommand =
                    $"openssl ecparam -name secp256k1 -genkey -noout -out {privateKeyPath}";

                // Command to generate public key
                string generatePublicKeyCommand = $"openssl ec -in {privateKeyPath} -pubout -out {publicKeyPath}";


                await Helper.RunCommandInCMD(generatePrivateKeyCommand);
                await Helper.RunCommandInCMD(generatePublicKeyCommand);

                if (System.IO.File.Exists(privateKeyPath))
                {
                    if (System.IO.File.Exists(configFilePath))
                    {
                        // Command to generate CSR
                        string generateCSRCommand =
                            $"openssl req -new -sha256 -key {privateKeyPath} -extensions v3_req -config {configFilePath} -out {csrPath}";

                        // Generate CSR
                        await Helper.RunCommandInCMD(generateCSRCommand);
                    }
                    else
                    {
                        return BadRequest("Config Path not found");
                    }
                }
                else
                {
                    return Ok("Key Path not found");
                }


                string generatedPrivateKey = Helper.ReadFromFile(privateKeyPath);
                string filteredPrivateKey = Helper.RemoveKeyHeaders(generatedPrivateKey);
                Helper.SaveToFile(filteredPrivateKey, filteredPrivateKeyPath);

                string generatedCsr = Helper.ReadFromFile(csrPath);

                /*
                string filteredCsr = Helper.ExtractCsrContent(generatedCsr);
                Helper.SaveToFile(filteredCsr, csrPath);
                */
                string encodedCsr = Helper.EncodeToBase64(generatedCsr);
                Helper.SaveToFile(encodedCsr, filteredCsrPath);
                return Ok("Private key, public key, and CSR generated successfully.");
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal Server Error: {ex.Message}");
            }
        }

        [HttpPost("generateCSID/{companyId}")]
        public async Task<IActionResult> GenerateCSID(int companyId)
        {
            var company = await _companyRepository.GetById(companyId);
            if (company == null)
            {
                return NotFound(new { Error = $"Company with ID {companyId} not found." });
            }

            string filteredCsrPath = $"{Constants.CsrBasePath}/taxpayer{companyId}-filtered.txt";
            string filteredPrivateKeyPath = $"{Constants.KeysBasePath}/privateKey{companyId}-filtered.pem";
            var csrData = Helper.ReadFromFile(filteredCsrPath);
            var keyData = Helper.ReadFromFile(filteredPrivateKeyPath);
            try
            {
                var response = await _externalApiService.CallComplianceCSR(csrData);

                var jsonResponse = JsonDocument.Parse(response).RootElement;

                var binarySecurityToken = jsonResponse.GetProperty("binarySecurityToken").GetString();
                var secret = jsonResponse.GetProperty("secret").GetString();

                string decodedCertificate = Helper.DecodeFromBase64(binarySecurityToken);

                var companyCredentials = new CompanyCredentials
                {
                    Certificate = decodedCertificate,
                    CSR = csrData,
                    Secret = secret,
                    SecretToken = binarySecurityToken,
                    PrivateKey = keyData,
                };

                company.CompanyCredentials.Add(companyCredentials);

                await _companyCredentialsRepository.Create(companyCredentials); 

                await _companyRepository.Update(company); 

                return Ok(new
                {
                    MainRes = jsonResponse,
                    DecodedCertificate = decodedCertificate,
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new
                    { Error = $"An error occurred: {ex.Message}", InnerException = ex.InnerException?.Message });
            }
        }

        private string CreateConfigFile(CertificateConfiguration certificateConfig, int companyId)
        {
            string configFilePath = $"{Constants.ConfigBasePath}/configuration{companyId}.cnf";

            Creator.WriteConfigurationFile(configFilePath, certificateConfig);
            return configFilePath;
        }
    }
}