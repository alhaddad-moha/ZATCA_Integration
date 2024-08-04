using Microsoft.AspNetCore.Mvc;
using ZATCA_V2.Exceptions;
using ZATCA_V2.Helpers;
using ZATCA_V2.Models;
using ZATCA_V2.Repositories.Interfaces;
using ZATCA_V2.Requests;
using ZATCA_V2.Responses;
using ZATCA_V2.Utils;
using ZATCA_V2.ZATCA;
using ZatcaIntegrationSDK;
using Invoice = ZatcaIntegrationSDK.Invoice;
using DBInvoiceModel = ZATCA_V2.Models.Invoice;


namespace ZATCA_V2.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class InvoiceController : ControllerBase
    {
        private readonly ICompanyCredentialsRepository _companyCredentialsRepository;
        private readonly ISignedInvoiceRepository _signedInvoiceRepository;
        private readonly IInvoiceRepository _invoiceRepository;
        private readonly ICompanyRepository _companyRepository;
        private readonly IZatcaService _zatcaService;


        public InvoiceController(ICompanyCredentialsRepository companyCredentialsRepository,
            ISignedInvoiceRepository signedInvoiceRepository,
            ICompanyRepository companyRepository, IZatcaService zatcaService,
            IInvoiceRepository invoiceRepository)
        {
            _companyRepository = companyRepository;
            _zatcaService = zatcaService;
            _invoiceRepository = invoiceRepository;
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

                var latestInvoice = await _invoiceRepository.GetLatestByCompanyId(singleInvoiceRequest.CompanyId);
                UBLXML ubl = new UBLXML();

                Invoice inv = InvoiceHelper.CreateMainInvoice(singleInvoiceRequest.InvoiceType,
                    singleInvoiceRequest.Invoice, company!);
                string invoiceHash = latestInvoice == null ? Constants.DefaultInvoiceHash : latestInvoice.Hash;

                foreach (var invoiceItem in singleInvoiceRequest.Invoice.InvoiceItems)
                {
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

                Result res = ubl.GenerateInvoiceXML(inv, Directory.GetCurrentDirectory());
                DBInvoiceModel invoice = CreateInvoiceFromResult(res, company!, singleInvoiceRequest.Invoice.Id,
                    singleInvoiceRequest.InvoiceType.Name);

                if (!res.IsValid)
                {
                    invoice.StatusCode = 400;
                    invoice.ErrorMessage = res.ErrorMessage;
                    invoice.WarningMessage = res.WarningMessage;
                    await _invoiceRepository.Create(invoice);
                    return BadRequest(res);
                }

                var invoiceResponse = await _zatcaService.SendInvoiceToZATCA(companyCredentials, res, inv);
                invoice.ZatcaResponse = invoiceResponse.ToString();
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


                await _invoiceRepository.Create(invoice);

                var response = new
                {
                    ZATCA = invoiceResponse,
                    Res = ExtractInvoiceDetails(res)
                };

                return Ok(response);
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
                    Invoice inv =
                        InvoiceHelper.CreateMainInvoice(bulkInvoiceRequest.InvoicesType, invoiceData, company!);

                    foreach (var invoiceItem in invoiceData.InvoiceItems)
                    {
                        InvoiceLine invoiceLine = InvoiceHelper.CreateInvoiceLine(
                            invoiceItem.Name, invoiceItem.Quantity, invoiceItem.BaseQuantity,
                            invoiceItem.Price, inv.allowanceCharges, invoiceItem.TaxCategory,
                            invoiceItem.VatPercentage, invoiceItem.IsIncludingVat, invoiceItem.TaxExemptionReasonCode,
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
                        await _invoiceRepository.Create(invoice);
                        return BadRequest(res);
                    }

                    var invoiceResponse = await _zatcaService.SendInvoiceToZATCA(companyCredentials, res, inv);
                    invoice.ZatcaResponse = invoiceResponse.ToString();
                    invoice.StatusCode = invoiceResponse.StatusCode;

                    if (invoiceResponse.IsSuccess)
                    {
                        invoice.IsSigned = true;

                        if (invoiceResponse.StatusCode == 202)
                        {
                            invoice.WarningMessage = invoiceResponse.WarningMessage;
                        }
                        else
                        {
                            invoice.StatusCode = 200;
                        }
                    }
                    else
                    {
                        if (invoiceResponse.StatusCode == 400 || invoiceResponse.StatusCode == 401)
                        {
                            invoice.WarningMessage = invoiceResponse.WarningMessage;
                            invoice.ErrorMessage = invoiceResponse.ErrorMessage;
                        }
                        else if (invoiceResponse.StatusCode == 500)
                        {
                            invoice.ErrorMessage = "Internal server error occurred." + invoiceResponse.ErrorMessage;
                        }
                    }


                    await _invoiceRepository.Create(invoice);

                    responses.Add(new
                    {
                        ZATCA = invoiceResponse,
                        Res = ExtractInvoiceDetails(res)
                    });
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
            string invoiceTypeName = invoiceType switch
            {
                "0100000" => "Standard",
                "0200000" => "Simplified",
                _ => "Unknown"
            };

            return new DBInvoiceModel
            {
                UUID = res.UUID,
                Hash = res.InvoiceHash,
                SystemInvoiceId = systemInvoiceId,
                EncodedInvoice = res.EncodedInvoice,
                QRCode = res.QRCode,
                IsSigned = false,
                StatusCode = 0,
                CompanyId = company.Id,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };
        }
    }
}