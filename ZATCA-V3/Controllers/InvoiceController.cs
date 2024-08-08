using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using ZATCA_V3.Exceptions;
using ZATCA_V3.Helpers;
using ZATCA_V3.Middlewares;
using ZATCA_V3.Models;
using ZATCA_V3.Repositories.Interfaces;
using ZATCA_V3.Requests;
using ZATCA_V3.Responses;
using ZATCA_V3.Utils;
using ZATCA_V3.ZATCA;
using ZatcaIntegrationSDK;
using ZatcaIntegrationSDK.HelperContracts;
using Invoice = ZatcaIntegrationSDK.Invoice;
using DBInvoiceModel = ZATCA_V3.Models.Invoice;


namespace ZATCA_V3.Controllers
{
    [ServiceFilter(typeof(ApiKeyFilter))]
    [Route("api/[controller]")]
    [ApiController]
    public class InvoiceController : ControllerBase
    {
        private readonly ICompanyCredentialsRepository _companyCredentialsRepository;
        private readonly ISignedInvoiceRepository _signedInvoiceRepository;
        private readonly IInvoiceRepository _invoiceRepository;
        private readonly ICompanyRepository _companyRepository;
        private readonly IZatcaService _zatcaService;
        private readonly ILogger<InvoiceController> _logger;


        public InvoiceController(ICompanyCredentialsRepository companyCredentialsRepository,
            ISignedInvoiceRepository signedInvoiceRepository,
            ICompanyRepository companyRepository, IZatcaService zatcaService,
            IInvoiceRepository invoiceRepository, ILogger<InvoiceController> logger)
        {
            _companyRepository = companyRepository;
            _zatcaService = zatcaService;
            _invoiceRepository = invoiceRepository;
            _logger = logger;
            _companyCredentialsRepository = companyCredentialsRepository;
            _signedInvoiceRepository = signedInvoiceRepository;
        }

        [HttpGet("companies/{id}")]
        public async Task<ActionResult<List<SignedInvoice>>> GetByCompanyId(int id)
        {
            var invoices = await _signedInvoiceRepository.GetAllByCompanyId(id);
            return Ok(invoices);
        }


        [HttpPost("sign-single")]
        public async Task<IActionResult> SignSingleInvoice(SingleInvoiceRequest singleInvoiceRequest)
        {
            try
            {
                var errors = new Dictionary<string, List<string>>();

                var company = await _companyRepository.GetById(singleInvoiceRequest.CompanyId);
                if (company == null)
                {
                    errors[nameof(singleInvoiceRequest.CompanyId)] = new List<string> { "Company Not Found" };
                }

                var companyCredentials =
                    await _companyCredentialsRepository.GetLatestByCompanyId(singleInvoiceRequest.CompanyId);
                if (companyCredentials == null)
                {
                    errors["Credentials"] = new List<string> { "Company Credentials Not Found" };
                }

                if (errors.Any())
                {
                    return new ApiResponse<object>(400, "Validation errors occurred.", null, errors);
                }


                var isInvoiceSigned = await _invoiceRepository.IsInvoiceSigned(singleInvoiceRequest.Invoice.Id,
                    singleInvoiceRequest.CompanyId);

                if (isInvoiceSigned)
                {
                    return new ApiResponse<object>(400, "Invoice already signed.");
                }

                var latestInvoice = await _invoiceRepository.GetLatestByCompanyId(singleInvoiceRequest.CompanyId);
                UBLXML ubl = new UBLXML();

                Invoice inv = InvoiceHelper.CreateMainInvoice(singleInvoiceRequest.InvoiceType,
                    singleInvoiceRequest.Invoice, company!);
                string invoiceHash = latestInvoice == null ? Constants.DefaultInvoiceHash : latestInvoice.Hash;

                foreach (var invoiceItem in singleInvoiceRequest.Invoice.InvoiceItems)
                {
                    if (!invoiceItem.TaxCategory.Contains('S'))
                    {
                        if (invoiceItem.TaxExemptionReason == null)
                        {
                            errors["TaxExemptionReason"] = new List<string>
                                { "TaxExemptionReason must be included for Z, O, or E" };
                            return new ApiResponse<object>(400, "Validation errors occurred.", null, errors);

                        }
                    }

                    var invoiceLine = InvoiceHelper.CreateInvoiceLine(
                        invoiceItem.Name, invoiceItem.Quantity, invoiceItem.BaseQuantity, invoiceItem.Price,
                        inv.allowanceCharges,
                        invoiceItem.TaxCategory, invoiceItem.VatPercentage, invoiceItem.IsIncludingVat,
                        invoiceItem.TaxExemptionReasonCode,
                        invoiceItem.TaxExemptionReason
                    );
                    inv.InvoiceLines.Add(invoiceLine);
                }

                inv.cSIDInfo.CertPem = companyCredentials!.Certificate;
                inv.cSIDInfo.PrivateKey = companyCredentials.PrivateKey;
                inv.AdditionalDocumentReferencePIH.EmbeddedDocumentBinaryObject = invoiceHash;

                Result res = ubl.GenerateInvoiceXML(inv, Directory.GetCurrentDirectory(), false);
                
                DBInvoiceModel invoice = CreateInvoiceFromResult(res, company!, singleInvoiceRequest.Invoice.Id,
                    singleInvoiceRequest.InvoiceType.Name);
                
                if (!res.IsValid)
                {
                    invoice.StatusCode = 400;
                    invoice.ErrorMessage = res.ErrorMessage;
                    invoice.WarningMessage = res.WarningMessage;
                    await _invoiceRepository.CreateOrUpdate(invoice);
                    return BadRequest(res);
                }


                if (!singleInvoiceRequest.isSignAllowed)
                {
                    invoice.StatusCode = 0;
                    await _invoiceRepository.CreateOrUpdate(invoice);

                    return Ok(new
                    {
                        ZATCA = "",
                        Res = ExtractInvoiceDetails(res)
                    });
                }

                var invoiceResponse = await _zatcaService.SendInvoiceToZATCA(companyCredentials, res, inv);
                _logger.LogDebug("Response Data: " + JsonConvert.SerializeObject(invoiceResponse));
                try
                {
                    invoice.ZatcaResponse =
                        JsonConvert.SerializeObject(invoiceResponse); // Serialize the response to JSON
                }
                catch (Exception e)
                {
                    _logger.LogError(e, "Error");
                }

                invoice.StatusCode = invoiceResponse.StatusCode;

                if (invoiceResponse.IsSuccess)
                {
                    invoice.IsSigned = true;

                    if (invoiceResponse.StatusCode == 202)
                    {
                        invoice.WarningMessage = invoiceResponse.WarningMessage;
                    }
                }
                else
                {
                    if (invoiceResponse.StatusCode is 400 or 401)
                    {
                        invoice.StatusCode = invoiceResponse.StatusCode;
                        invoice.WarningMessage = invoiceResponse.WarningMessage ?? string.Empty;
                    }
                    else if (invoiceResponse.StatusCode == 500)
                    {
                        invoice.StatusCode = 500;
                        invoice.ErrorMessage = "Internal server error occurred." + invoiceResponse.ErrorMessage;
                    }
                    else
                    {
                        invoice.StatusCode = 500;
                    }

                    invoice.ErrorMessage = invoiceResponse.ErrorMessage;
                }


                await _invoiceRepository.CreateOrUpdate(invoice);

                var response = new
                {
                    ZATCA = invoiceResponse,
                    Res = ExtractInvoiceDetails(res)
                };

                return new ApiResponse<object>(201, "Invoices created successfully.", response);
            }
            catch (CustomValidationException ex)
            {
                return BadRequest(new { errors = ex.Errors });
            }
            catch (Exception ex)
            {
                // Log the exception (you might want to use a logging framework here)
                return StatusCode(500, $"Internal server error: {ex.Message} {ex.StackTrace}");
            }
        }

        [HttpPost("sign")]
        public async Task<IActionResult> GenerateDynamicStandard(BulkInvoiceRequest bulkInvoiceRequest)
        {
            try
            {
                var errors = new Dictionary<string, List<string>>();

                var company = await _companyRepository.GetById(bulkInvoiceRequest.companyId);
                if (company == null)
                {
                    if (!errors.ContainsKey(nameof(bulkInvoiceRequest.companyId)))
                    {
                        errors[nameof(bulkInvoiceRequest.companyId)] = new List<string>();
                    }

                    errors[nameof(bulkInvoiceRequest.companyId)].Add("Company Not Found");
                }

                var companyCredentials =
                    await _companyCredentialsRepository.GetLatestByCompanyId(bulkInvoiceRequest.companyId);
                if (companyCredentials == null)
                {
                    if (!errors.ContainsKey("Credentials"))
                    {
                        errors["Credentials"] = new List<string>();
                    }

                    errors["Credentials"].Add("Company Credentials Not Found");
                }

                if (errors.Any())
                {
                    return new ApiResponse<object>(400, "Validation errors occurred.", null, errors);
                }


                var latestInvoice = await _invoiceRepository.GetLatestByCompanyId(bulkInvoiceRequest.companyId);
                string invoiceHash = latestInvoice == null ? Constants.DefaultInvoiceHash : latestInvoice.Hash;

                UBLXML ubl = new UBLXML();

                List<object> responses = new List<object>();
                foreach (var invoiceData in bulkInvoiceRequest.Invoices)
                {
                    var isInvoiceSigned = await _invoiceRepository.IsInvoiceSigned(invoiceData.Id,
                        bulkInvoiceRequest.companyId);

                    if (!isInvoiceSigned)
                    {
                        Invoice inv =
                            InvoiceHelper.CreateMainInvoice(bulkInvoiceRequest.InvoicesType, invoiceData, company!);

                        foreach (var invoiceItem in invoiceData.InvoiceItems)
                        {
                            InvoiceLine invoiceLine = InvoiceHelper.CreateInvoiceLine(
                                invoiceItem.Name, invoiceItem.Quantity, invoiceItem.BaseQuantity,
                                invoiceItem.Price, inv.allowanceCharges, invoiceItem.TaxCategory,
                                invoiceItem.VatPercentage, invoiceItem.IsIncludingVat,
                                invoiceItem.TaxExemptionReasonCode,
                                invoiceItem.TaxExemptionReason);

                            inv.InvoiceLines.Add(invoiceLine);
                        }

                        inv.cSIDInfo.CertPem = companyCredentials!.Certificate;
                        inv.cSIDInfo.PrivateKey = companyCredentials.PrivateKey;
                        inv.AdditionalDocumentReferencePIH.EmbeddedDocumentBinaryObject = invoiceHash;

                        Result res = ubl.GenerateInvoiceXML(inv, Directory.GetCurrentDirectory());

                        DBInvoiceModel invoice = CreateInvoiceFromResult(res, company!, invoiceData.Id,
                            bulkInvoiceRequest.InvoicesType.Name);

                        if (!res.IsValid)
                        {
                            invoice.StatusCode = 400;
                            invoice.ErrorMessage = res.ErrorMessage;
                            invoice.WarningMessage = res.WarningMessage;
                            await _invoiceRepository.CreateOrUpdate(invoice);
                            return BadRequest(res);
                        }

                        var invoiceResponse = await _zatcaService.SendInvoiceToZATCA(companyCredentials, res, inv);
                        invoice.ZatcaResponse =
                            JsonConvert.SerializeObject(invoiceResponse); // Serialize the response to JSON

                        invoice.StatusCode = invoiceResponse.StatusCode;

                        if (invoiceResponse.IsSuccess)
                        {
                            invoice.IsSigned = true;

                            if (invoiceResponse.StatusCode == 202)
                            {
                                invoice.WarningMessage = invoiceResponse.WarningMessage;
                            }
                        }
                        else
                        {
                            if (invoiceResponse.StatusCode is 400 or 401)
                            {
                                invoice.StatusCode = invoiceResponse.StatusCode;
                                invoice.WarningMessage = invoiceResponse.WarningMessage;
                            }
                            else if (invoiceResponse.StatusCode == 500)
                            {
                                invoice.StatusCode = 500;
                                invoice.ErrorMessage = "Internal server error occurred." + invoiceResponse.ErrorMessage;
                            }
                            else
                            {
                                invoice.StatusCode = 500;
                            }

                            invoice.ErrorMessage = invoiceResponse.ErrorMessage;
                        }

                        await _invoiceRepository.CreateOrUpdate(invoice);

                        responses.Add(new
                        {
                            InvoiceId = invoiceData.Id,
                            ZATCA = invoiceResponse,
                            Res = ExtractInvoiceDetails(res)
                        });
                    }
                }

                return Ok(responses);
            }
            catch (CustomValidationException ex)
            {
                return BadRequest(new { errors = ex.Errors });
            }
            catch (Exception ex)
            {
                // Log the exception (you might want to use a logging framework here)
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        [HttpPost("send-unsigned")]
        public async Task<IActionResult> SendUnsignedInvoices(int companyId, bool isForAll)
        {
            try
            {
                List<DBInvoiceModel> unsignedInvoices;

                if (isForAll)
                {
                    unsignedInvoices = await _invoiceRepository.GetAllUnsignedInvoices();
                }
                else
                {
                    unsignedInvoices = await _invoiceRepository.GetUnsignedInvoicesByCompanyId(companyId);
                }

                if (!unsignedInvoices.Any())
                {
                    return Ok(new { message = "No unsigned invoices found." });
                }

                Company? company;
                CompanyCredentials? companyCredentials = null;

                if (!isForAll)
                {
                    company = await _companyRepository.GetById(companyId);
                    if (company == null)
                    {
                        return BadRequest(new { message = "Company not found." });
                    }

                    companyCredentials = await _companyCredentialsRepository.GetLatestByCompanyId(companyId);
                    if (companyCredentials == null)
                    {
                        return BadRequest(new { message = "Company credentials not found." });
                    }
                }

                var responses = new List<object>();

                foreach (var invoice in unsignedInvoices)
                {
                    if (isForAll)
                    {
                        company = await _companyRepository.GetById(invoice.CompanyId);
                        if (company == null)
                        {
                            continue;
                        }

                        companyCredentials =
                            await _companyCredentialsRepository.GetLatestByCompanyId(invoice.CompanyId);
                        if (companyCredentials == null)
                        {
                            continue;
                        }
                    }

                    InvoiceReportingRequest invoiceRequestBody = new InvoiceReportingRequest
                    {
                        invoice = invoice.EncodedInvoice,
                        invoiceHash = invoice.Hash,
                        uuid = invoice.UUID
                    };
                    if (companyCredentials != null)
                    {
                        var invoiceResponse =
                            await _zatcaService.ReSendInvoiceToZATCA(companyCredentials, invoiceRequestBody,
                                invoice.Type);
                        invoice.ZatcaResponse = JsonConvert.SerializeObject(invoiceResponse);
                        invoice.StatusCode = invoiceResponse.StatusCode;

                        if (invoiceResponse.IsSuccess)
                        {
                            invoice.IsSigned = true;

                            if (invoiceResponse.StatusCode == 202)
                            {
                                invoice.WarningMessage = invoiceResponse.WarningMessage;
                            }
                        }
                        else
                        {
                            invoice.StatusCode = invoiceResponse.StatusCode;
                            invoice.ErrorMessage = invoiceResponse.ErrorMessage;
                        }

                        await _invoiceRepository.CreateOrUpdate(invoice);


                        responses.Add(new
                        {
                            Invoice = invoice,
                            ZATCA = invoiceResponse,
                        });
                    }
                }

                return new ApiResponse<object>(201, "Invoices singed successfully.", responses);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while sending unsigned invoices to ZATCA.");
                return new ApiResponse<object>(500, $"An error occurred: {ex.Message}");
            }
        }

        private object ExtractInvoiceDetails(Result res)
        {
            return new
            {
                res.InvoiceHash,
                res.UUID,
                res.PIH,
                res.QRCode,
                res.LineExtensionAmount,
                res.TaxExclusiveAmount,
                res.TaxInclusiveAmount,
                res.AllowanceTotalAmount,
                res.ChargeTotalAmount,
                res.PayableAmount,
                res.PrepaidAmount,
                res.TaxAmount,
                res.EncodedInvoice
            };
        }


        private DBInvoiceModel CreateInvoiceFromResult(Result res, Company company, string systemInvoiceId,
            string invoiceType = "0100000")
        {
            return new DBInvoiceModel
            {
                UUID = res.UUID,
                Hash = res.InvoiceHash,
                SystemInvoiceId = systemInvoiceId,
                EncodedInvoice = res.EncodedInvoice,
                QRCode = res.QRCode,
                IsSigned = false,
                StatusCode = 0,
                Type = invoiceType,
                CompanyId = company.Id,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };
        }
    }
}