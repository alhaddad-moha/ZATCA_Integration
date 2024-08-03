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
using AllowanceCharge = ZatcaIntegrationSDK.AllowanceCharge;
using Invoice = ZatcaIntegrationSDK.Invoice;


namespace ZATCA_V2.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class InvoiceController : ControllerBase
    {
        private readonly ICompanyCredentialsRepository _companyCredentialsRepository;
        private readonly ISignedInvoiceRepository _signedInvoiceRepository;
        private readonly ICompanyRepository _companyRepository;
        private readonly IZatcaService _zatcaService;


        public InvoiceController(ICompanyCredentialsRepository companyCredentialsRepository,
            ISignedInvoiceRepository signedInvoiceRepository,
            ICompanyRepository companyRepository,IZatcaService zatcaService)
        {
            _companyRepository = companyRepository;
            _zatcaService = zatcaService;
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
                    if (!errors.ContainsKey(nameof(singleInvoiceRequest.CompanyId)))
                    {
                        errors[nameof(singleInvoiceRequest.CompanyId)] = new List<string>();
                    }

                    errors[nameof(singleInvoiceRequest.CompanyId)].Add("Company Not Found");
                }

                var companyCredentials =
                    await _companyCredentialsRepository.GetLatestByCompanyId(singleInvoiceRequest.CompanyId);
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

                var latestInvoice = await _signedInvoiceRepository.GetLatestByCompanyId(singleInvoiceRequest.CompanyId);
                UBLXML ubl = new UBLXML();

                Invoice inv = InvoiceHelper.CreateMainInvoice(singleInvoiceRequest.InvoiceType, singleInvoiceRequest.Invoice,
                    company!);

                string invoiceHash = latestInvoice == null ? Constants.DefaultInvoiceHash : latestInvoice.InvoiceHash;

                foreach (var invoiceItem in singleInvoiceRequest.Invoice.InvoiceItems)
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
                if (!res.IsValid)
                {
                    return BadRequest(res);
                }

                SignedInvoice signedInvoice = CreateSignedInvoice(res, company!, singleInvoiceRequest.InvoiceType.Name);

                var invoiceResponse = await _zatcaService.SendInvoiceToZATCA(companyCredentials, res, inv);

                if (invoiceResponse.StatusCode == 202)
                {
                    if (!string.IsNullOrEmpty(invoiceResponse.ErrorMessage))
                    {
                        return BadRequest(invoiceResponse.ErrorMessage);
                    }
                }


                await _signedInvoiceRepository.Create(signedInvoice);

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

                var latestInvoice = await _signedInvoiceRepository.GetLatestByCompanyId(bulkInvoiceRequest.companyId);
                string invoiceHash = latestInvoice == null ? Constants.DefaultInvoiceHash : latestInvoice.InvoiceHash;

                UBLXML ubl = new UBLXML();

                List<object> responses = new List<object>();
                foreach (var invoiceData in bulkInvoiceRequest.Invoices)
                {
                    Invoice inv = InvoiceHelper.CreateMainInvoice(bulkInvoiceRequest.InvoicesType, invoiceData, company!);
                    
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
                    if (res.IsValid)
                    {
                        // return Ok(res.InvoiceHash);
                        // return Ok(res.SingedXML);
                        // return Ok(res.EncodedInvoice);
                        // return Ok(res.UUID);
                        // return Ok(res.QRCode);
                        // return Ok(res.PIH);
                        // return Ok(res.SingedXMLFileName);
                    }
                    else
                    {
                        return BadRequest(res);
                    }

                    SignedInvoice signedInvoice =
                        CreateSignedInvoice(res, company!, bulkInvoiceRequest.InvoicesType.Name);


                    var invoiceResponse = await _zatcaService.SendInvoiceToZATCA(companyCredentials, res, inv);

                    if (string.IsNullOrEmpty(invoiceResponse.ErrorMessage))
                    {
                        await _signedInvoiceRepository.Create(signedInvoice);

                        responses.Add(new
                        {
                            ZATCA = invoiceResponse,
                            Res = ExtractInvoiceDetails(res)
                        });
                    }
                    else
                    {
                        responses.Add(invoiceResponse);
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


        private SignedInvoice CreateSignedInvoice(Result res, Company company, string invoiceType = "0100000")
        {
            string invoiceTypeName;

            if (invoiceType == "0100000")
            {
                invoiceTypeName = "Standard";
            }
            else if (invoiceType == "0200000")
            {
                invoiceTypeName = "Simplified";
            }
            else
            {
                invoiceTypeName = "Unknown"; // Handle any other cases if needed
            }

            return new SignedInvoice
            {
                UUID = res.UUID,
                InvoiceHash = res.InvoiceHash,
                InvoiceType = invoiceTypeName,
                Amount = Convert.ToDecimal(res.TaxExclusiveAmount),
                Tax = Convert.ToDecimal(res.TaxAmount),
                SingedXML = res.SingedXML,
                EncodedInvoice = res.EncodedInvoice,
                QRCode = res.QRCode,
                SingedXMLFileName = res.SingedXMLFileName,
                CompanyId = company.Id,
            };
        }
    }
}