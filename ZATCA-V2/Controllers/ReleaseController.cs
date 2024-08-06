using Microsoft.AspNetCore.Mvc;
using ZATCA_V2.DTOs;
using ZATCA_V2.Helpers;
using ZATCA_V2.Middlewares;
using ZATCA_V2.Models;
using ZATCA_V2.Repositories.Interfaces;
using ZATCA_V2.Responses;
using ZATCA_V2.Utils;
using ZatcaIntegrationSDK;
using ZatcaIntegrationSDK.APIHelper;
using ZatcaIntegrationSDK.HelperContracts;
using Invoice = ZatcaIntegrationSDK.Invoice;

namespace ZATCA_V2.Controllers
{
    [ServiceFilter(typeof(ApiKeyFilter))]
    [Route("api/[controller]")]
    [ApiController]
    public class ReleaseController : ControllerBase
    {
        private readonly ICompanyRepository _companyRepository;
        private readonly ICompanyCredentialsRepository _companyCredentialsRepository;
        private Mode _mode = Constants.DefaultMode;


        public ReleaseController(ICompanyRepository companyRepository,
            ICompanyCredentialsRepository companyCredentialsRepository
        )

        {
            _companyRepository = companyRepository ?? throw new ArgumentNullException(nameof(companyRepository));
            _companyCredentialsRepository = companyCredentialsRepository ??
                                            throw new ArgumentNullException(nameof(companyCredentialsRepository));
        }

        [HttpPost("register")]
        public async Task<IActionResult> CreateCompanyWithCsid(CompanyReleaseRequestDto companyReleaseRequest)
        {
            try
            {
                var errors = new Dictionary<string, List<string>>();

                // Check if the TaxRegistrationNumber already exists
                var existingCompanyByTax =
                    await _companyRepository.FindByTaxRegistrationNumber(companyReleaseRequest.TaxRegistrationNumber);
                if (existingCompanyByTax != null)
                {
                    if (!errors.ContainsKey(nameof(companyReleaseRequest.TaxRegistrationNumber)))
                    {
                        errors[nameof(companyReleaseRequest.TaxRegistrationNumber)] = new List<string>();
                    }

                    errors[nameof(companyReleaseRequest.TaxRegistrationNumber)]
                        .Add("Tax Registration Number already exists.");
                }

                var existingCompanyByCommercial =
                    await _companyRepository.FindByCommercialRegistrationNumber(companyReleaseRequest
                        .CommercialRegistrationNumber);
                if (existingCompanyByCommercial != null)
                {
                    if (!errors.ContainsKey(nameof(companyReleaseRequest.CommercialRegistrationNumber)))
                    {
                        errors[nameof(companyReleaseRequest.CommercialRegistrationNumber)] = new List<string>();
                    }

                    errors[nameof(companyReleaseRequest.CommercialRegistrationNumber)]
                        .Add("Commercial Registration Number already exists.");
                }

                if (errors.Any())
                {
                    return new ApiResponse<object>(400, "Validation errors occurred.", null, errors);
                }

                var company = Creator.GenerateCompanyData(companyReleaseRequest);
                Invoice inv = GenerateInvoice(company);

                CertificateRequest certrequest = GetCsrRequestData(companyReleaseRequest);


                CSIDGenerator generator = new CSIDGenerator(_mode);
                string fullPath = Path.Combine(Directory.GetCurrentDirectory(), Constants.SampleInvoicesPath);

                CertificateResponse response =
                    await generator.GenerateCSID(certrequest, inv, fullPath);

                if (response.IsSuccess)
                {
                    await _companyRepository.Create(company);

                    var binarySecurityToken = Utility.ToBase64Encode(response.CSID);

                    var companyCredentials = new CompanyCredentials
                    {
                        Certificate = response.CSID,
                        CSR = response.CSR,
                        Secret = response.SecretKey,
                        SecretToken = binarySecurityToken,
                        PrivateKey = response.PrivateKey
                    };

                    if (company.CompanyCredentials != null) company.CompanyCredentials.Add(companyCredentials);

                    await _companyCredentialsRepository.Create(companyCredentials);
                    await _companyRepository.Update(company);

                    var data = new
                    {
                        company = Creator.MapToCompanyDto(company),
                        zatcaResponse = new
                        {
                            Success = response.IsSuccess,
                            Secret = response.SecretKey
                        }
                    };

                    return new ApiResponse<object>(201, "Company created successfully.", data);
                }
                else
                {
                    return new ApiResponse<object>(500,
                        $"An error occurred during CSID generation: {response.ErrorMessage}", response);
                }
            }
            catch (Exception ex)
            {
                return new ApiResponse<object>(500, $"An error occurred: {ex.Message}");
            }
        }

        [HttpPost("generate-csid")]
        public async Task<IActionResult> GenerateCsidForExistingCompany(GenerateCsidDto request)
        {
            try
            {
                // Retrieve the existing company based on the provided company ID
                var existingCompany = await _companyRepository.GetById(request.CompanyId);
                if (existingCompany == null)
                {
                    return new ApiResponse<object>(404, "Company not found.");
                }

                Invoice inv = GenerateInvoice(existingCompany);

                CertificateRequest certRequest = GenerateCertificateRequestData(existingCompany,request.Otp);
                
                CSIDGenerator generator = new CSIDGenerator(_mode);
                string fullPath = Path.Combine(Directory.GetCurrentDirectory(), Constants.SampleInvoicesPath);

                CertificateResponse response =
                    await generator.GenerateCSID(certRequest, inv, fullPath);

                if (response.IsSuccess)
                {
                    var binarySecurityToken = response.SecretKey;

                    var companyCredentials = new CompanyCredentials
                    {
                        Certificate = response.CSID,
                        CSR = response.CSR,
                        Secret = response.SecretKey,
                        SecretToken = binarySecurityToken,
                        PrivateKey = response.PrivateKey,
                        CompanyId = existingCompany.Id
                    };

                    existingCompany.CompanyCredentials?.Add(companyCredentials);

                    await _companyCredentialsRepository.Create(companyCredentials);
                    await _companyRepository.Update(existingCompany);

                    var companyDto = Creator.MapToCompanyDto(existingCompany);
                    var data = new
                    {
                        company = companyDto,
                        zatcaResponse = new
                        {
                            Success = response.IsSuccess,
                            Secret = response.SecretKey
                        }
                    };

                    return new ApiResponse<object>(201, "CSID generated successfully.", data);
                }
                else
                {
                    return new ApiResponse<object>(400, $"An error occurred: {response.ErrorMessage}");
                }
            }
            catch
                (Exception ex)
            {
                return new ApiResponse<object>(500, $"An error occurred: {ex.Message}");
            }
        }

        private CertificateRequest GenerateCertificateRequestData(Company company, string? otp = null)
        {
            return new CertificateRequest
            {
                OTP = otp,
                CommonName = company.CommonName,
                OrganizationName = company.OrganizationName,
                OrganizationUnitName = company.OrganizationUnitName,
                CountryName = company.CountryName,
                SerialNumber = company.DeviceSerialNumber,
                OrganizationIdentifier = company.TaxRegistrationNumber,
                Location =
                    $"{company.CityName},{company.StreetName} {company.BuildingNumber}",
                BusinessCategory = company.BusinessIndustry,
                InvoiceType = company.InvoiceType
            };
        }


        private Invoice GenerateInvoice(Company company)
        {
            Invoice inv = new Invoice();

            inv.ID = "INV00001";
            inv.IssueDate = DateTime.Now.ToString("yyyy-MM-dd");
            inv.IssueTime = DateTime.Now.ToString("HH:mm:ss");
            inv.DocumentCurrencyCode = "SAR";
            inv.TaxCurrencyCode = "SAR";

            inv.AdditionalDocumentReferencePIH.EmbeddedDocumentBinaryObject =
                "NWZlY2ViNjZmZmM4NmYzOGQ5NTI3ODZjNmQ2OTZjNzljMmRiYzIzOWRkNGU5MWI0NjcyOWQ3M2EyN2ZiNTdlOQ==";
            inv.AdditionalDocumentReferenceICV.UUID = 123;

            PaymentMeans paymentMeans = new PaymentMeans();
            paymentMeans.PaymentMeansCode = "10";
            paymentMeans.InstructionNote = "Payment Notes";
            inv.paymentmeans.Add(paymentMeans);

            AccountingSupplierParty supplierParty = InvoiceHelper.CreateSupplierPartyFromCompany(company);
            inv.SupplierParty = supplierParty;

            inv.CustomerParty.partyIdentification.ID = "1234567";
            inv.CustomerParty.partyIdentification.schemeID = "CRN";
            inv.CustomerParty.postalAddress.StreetName = "شارع تجربة";
            inv.CustomerParty.postalAddress.AdditionalStreetName = "شارع اضافى";
            inv.CustomerParty.postalAddress.BuildingNumber = "1234";
            inv.CustomerParty.postalAddress.PlotIdentification = "9833";
            inv.CustomerParty.postalAddress.CityName = "Jeddah";
            inv.CustomerParty.postalAddress.PostalZone = "12345";
            inv.CustomerParty.postalAddress.CountrySubentity = "Makkah";
            inv.CustomerParty.postalAddress.CitySubdivisionName = "المحافظة";
            inv.CustomerParty.postalAddress.country.IdentificationCode = "SA";
            inv.CustomerParty.partyLegalEntity.RegistrationName = "اسم شركة المشترى";
            inv.CustomerParty.partyTaxScheme.CompanyID = "310424415000003";

            inv.legalMonetaryTotal.PrepaidAmount = 0;
            inv.legalMonetaryTotal.PayableAmount = 0;

            InvoiceLine invline = new InvoiceLine();
            invline.InvoiceQuantity = 1;
            invline.item.Name = "منتج تجربة";
            invline.item.classifiedTaxCategory.ID = "S";
            invline.taxTotal.TaxSubtotal.taxCategory.ID = "S";
            invline.item.classifiedTaxCategory.Percent = 15;
            invline.taxTotal.TaxSubtotal.taxCategory.Percent = 15;
            invline.price.PriceAmount = 120;
            inv.InvoiceLines.Add(invline);

            return inv;
        }

        private CertificateRequest GetCsrRequestData(CompanyReleaseRequestDto companyReleaseRequest)
        {
            CertificateRequest certrequest = new CertificateRequest();
            //TODO get OTP 
            certrequest.OTP = companyReleaseRequest.OTP;
            certrequest.CommonName = companyReleaseRequest.CommonName;
            certrequest.OrganizationName = companyReleaseRequest.OrganizationName;
            certrequest.OrganizationUnitName = companyReleaseRequest.OrganizationUnitName;
            certrequest.CountryName = companyReleaseRequest.CountryName;
            certrequest.SerialNumber = companyReleaseRequest.DeviceSerialNumber;
            certrequest.OrganizationIdentifier = companyReleaseRequest.TaxRegistrationNumber;
            certrequest.Location = companyReleaseRequest.Address.CityName + "," +
                                   companyReleaseRequest.Address.StreetName + " " +
                                   companyReleaseRequest.Address.BuildingNumber;
            certrequest.BusinessCategory = companyReleaseRequest.BusinessIndustry;
            certrequest.InvoiceType = companyReleaseRequest.InvoiceType;
            return certrequest;
        }
    }
}