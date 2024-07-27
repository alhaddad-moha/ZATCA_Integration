using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using ZATCA_V2.DTOs;
using ZATCA_V2.Helpers;
using ZATCA_V2.Models;
using ZATCA_V2.Repositories.Interfaces;
using ZATCA_V2.Requests;
using ZATCA_V2.Utils;
using ZatcaIntegrationSDK;
using ZatcaIntegrationSDK.APIHelper;
using ZatcaIntegrationSDK.BLL;
using ZatcaIntegrationSDK.HelperContracts;

namespace ZATCA_V2.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class InvoiceController : ControllerBase
    {
        private readonly ICompanyCredentialsRepository _companyCredentialsRepository;
        private readonly ISignedInvoiceRepository _signedInvoiceRepository;
        private readonly ICompanyInfoRepository _companyInfoRepository;
        private readonly ICompanyRepository _companyRepository;
        private readonly ILogger<InvoiceController> _logger;

        public InvoiceController(
            ICompanyCredentialsRepository companyCredentialsRepository,
            ISignedInvoiceRepository signedInvoiceRepository,
            ICompanyInfoRepository companyInfoRepository,
            ICompanyRepository companyRepository,
            ILogger<InvoiceController> logger)
        {
            _companyCredentialsRepository = companyCredentialsRepository;
            _signedInvoiceRepository = signedInvoiceRepository;
            _companyInfoRepository = companyInfoRepository;
            _companyRepository = companyRepository;
            _logger = logger;
        }

        [HttpGet("companies/{id}")]
        public async Task<ActionResult<List<SignedInvoice>>> GetByCompanyId(int id)
        {
            _logger.LogInformation("Fetching invoices for company with ID {CompanyId}", id);
            try
            {
                var invoices = await _signedInvoiceRepository.GetAllByCompanyId(id);
                return Ok(invoices);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while fetching invoices for company with ID {CompanyId}", id);
                return StatusCode(StatusCodes.Status500InternalServerError, ex);
            }
        }

        [HttpPost("simplified")]
        public async Task<IActionResult> GenerateSimpleInvoices()
        {
            _logger.LogInformation("Generating simplified invoices");
            try
            {
                // Implementation needed
                return Ok();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while generating simplified invoices");
                return StatusCode(StatusCodes.Status500InternalServerError, ex);
            }
        }

        [HttpPost("sign-single")]
        public async Task<IActionResult> SignSingleInvoice(SingleInvoiceRequest singleInvoiceRequest)
        {
            _logger.LogInformation("Signing single invoice for company with ID {CompanyId}",
                singleInvoiceRequest.CompanyId);
            try
            {
                var company = await _companyRepository.GetById(singleInvoiceRequest.CompanyId);
                if (company == null)
                {
                    _logger.LogWarning("Company with ID {CompanyId} not found", singleInvoiceRequest.CompanyId);
                    return BadRequest("Company Not Found");
                }

                var companyInfo = await _companyInfoRepository.GetByCompanyId(singleInvoiceRequest.CompanyId);
                var companyCredentials =
                    await _companyCredentialsRepository.GetLatestByCompanyId(singleInvoiceRequest.CompanyId);
                var latestSignedInvoice =
                    await _signedInvoiceRepository.GetLatestByCompanyId(singleInvoiceRequest.CompanyId);

                if (companyCredentials == null)
                {
                    _logger.LogWarning("Company credentials for company ID {CompanyId} not found",
                        singleInvoiceRequest.CompanyId);
                    return BadRequest("CompanyCredentials Not Found");
                }

                if (singleInvoiceRequest.Invoice == null)
                {
                    _logger.LogWarning("Invoice data not provided for company ID {CompanyId}",
                        singleInvoiceRequest.CompanyId);
                    return BadRequest("Invoice Data Not Provided");
                }

                UBLXML ubl = new UBLXML();
                ApiRequestLogic apireqlogic = new ApiRequestLogic(Mode.developer);

                Invoice inv = CreateMainInvoice(singleInvoiceRequest.InvoiceType!, singleInvoiceRequest.Invoice,
                    companyInfo!);
                string latestInvoiceHash = latestSignedInvoice?.InvoiceHash ?? string.Empty;
                inv.AdditionalDocumentReferencePIH.EmbeddedDocumentBinaryObject = latestInvoiceHash;

                Result res = ubl.GenerateInvoiceXML(inv, Directory.GetCurrentDirectory());
                if (!res.IsValid)
                {
                    _logger.LogWarning("Generated invoice XML is invalid for company ID {CompanyId}",
                        singleInvoiceRequest.CompanyId);
                    return BadRequest(res);
                }

                SignedInvoice signedInvoice = CreateSignedInvoice(res, company);

                InvoiceReportingRequest invrequestbody = new InvoiceReportingRequest
                {
                    invoice = res.EncodedInvoice,
                    invoiceHash = res.InvoiceHash,
                    uuid = res.UUID
                };

                InvoiceReportingResponse invoicereportingmodel =
                    await CallComplianceInvoiceAPI(apireqlogic, companyCredentials, res);

                if (!string.IsNullOrEmpty(invoicereportingmodel.ErrorMessage))
                {
                    _logger.LogWarning("Compliance invoice API returned an error: {ErrorMessage}",
                        invoicereportingmodel.ErrorMessage);
                    return BadRequest(invoicereportingmodel);
                }

                await _signedInvoiceRepository.Create(signedInvoice);

                var response = new
                {
                    ZATCA = invoicereportingmodel,
                    Res = ExtractInvoiceDetails(res)
                };

                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while signing single invoice for company with ID {CompanyId}",
                    singleInvoiceRequest.CompanyId);
                return StatusCode(StatusCodes.Status500InternalServerError, ex);
            }
        }

        [HttpPost("sign")]
        public async Task<IActionResult> GenerateDynamicStandard(BulkInvoiceRequest bulkInvoiceRequest)
        {
            _logger.LogInformation("Generating dynamic standard invoices for company with ID {CompanyId}",
                bulkInvoiceRequest.companyId);
            try
            {
                var companyInfo = await _companyInfoRepository.GetByCompanyId(bulkInvoiceRequest.companyId);
                var company = await _companyRepository.GetById(bulkInvoiceRequest.companyId);
                var companyCredentials =
                    await _companyCredentialsRepository.GetLatestByCompanyId(bulkInvoiceRequest.companyId);

                if (company == null)
                {
                    _logger.LogWarning("Company with ID {CompanyId} not found", bulkInvoiceRequest.companyId);
                    return BadRequest("Company Not Found");
                }

                if (companyCredentials == null)
                {
                    _logger.LogWarning("Company credentials for company ID {CompanyId} not found",
                        bulkInvoiceRequest.companyId);
                    return BadRequest("CompanyCredentials Not Found");
                }

                UBLXML ubl = new UBLXML();
                ApiRequestLogic apireqlogic = new ApiRequestLogic(Mode.developer);

                List<object> responses = new List<object>();
                foreach (var invoiceData in bulkInvoiceRequest.Invoices!)
                {
                    Invoice inv = CreateMainInvoice(bulkInvoiceRequest.InvoicesType!, invoiceData, companyInfo!);
                    Result res = ubl.GenerateInvoiceXML(inv, Directory.GetCurrentDirectory());

                    if (res.IsValid)
                    {
                        SignedInvoice signedInvoice = CreateSignedInvoice(res, company);

                        InvoiceReportingRequest invrequestbody = new InvoiceReportingRequest
                        {
                            invoice = res.EncodedInvoice,
                            invoiceHash = res.InvoiceHash,
                            uuid = res.UUID
                        };

                        InvoiceReportingResponse invoicereportingmodel =
                            await CallComplianceInvoiceAPI(apireqlogic, companyCredentials, res);

                        if (string.IsNullOrEmpty(invoicereportingmodel.ErrorMessage))
                        {
                            await _signedInvoiceRepository.Create(signedInvoice);

                            responses.Add(new
                            {
                                ZATCA = invoicereportingmodel,
                                Res = ExtractInvoiceDetails(res)
                            });
                        }
                        else
                        {
                            _logger.LogWarning("Compliance invoice API returned an error: {ErrorMessage}",
                                invoicereportingmodel.ErrorMessage);
                            responses.Add(invoicereportingmodel);
                        }
                    }
                    else
                    {
                        _logger.LogWarning("Generated invoice XML is invalid for company ID {CompanyId}",
                            bulkInvoiceRequest.companyId);
                        responses.Add(res);
                    }
                }

                return Ok(responses);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex,
                    "An error occurred while generating dynamic standard invoices for company with ID {CompanyId}",
                    bulkInvoiceRequest.companyId);
                return StatusCode(StatusCodes.Status500InternalServerError, ex);
            }
        }

        private Invoice CreateMainInvoice(InvoiceType invoicesType, InvoiceData invoiceData, CompanyInfo companyInfo)
        {
            Invoice inv = new Invoice();
            

            inv.ID = invoiceData.Id;
            inv.IssueDate = invoiceData.IssueDate;
            inv.IssueTime = invoiceData.IssueTime;

            inv.invoiceTypeCode.id =
                invoicesType.Id; // Use the parameter 'invoicesType' instead of 'bulkInvoiceRequest.InvoicesType'

            inv.invoiceTypeCode.Name = invoicesType.Name;
            inv.DocumentCurrencyCode = invoicesType.DocumentCurrencyCode;

            inv.TaxCurrencyCode = invoicesType.TaxCurrencyCode;

            if (inv.invoiceTypeCode.id == 383 || inv.invoiceTypeCode.id == 381)
            {
                // فى حالة ان اشعار دائن او مدين فقط هانكتب رقم الفاتورة اللى اصدرنا الاشعار ليها
                // in case of return sales invoice or debit notes we must mention the original sales invoice number
                InvoiceDocumentReference invoiceDocumentReference = new InvoiceDocumentReference();
                invoiceDocumentReference.ID =
                    invoiceData.InvoiceDocumentReferenceID; // mandatory in case of return sales invoice or debit notes
                inv.billingReference.invoiceDocumentReferences.Add(invoiceDocumentReference);
            }


            inv.AdditionalDocumentReferenceICV.UUID = invoiceData.AddtionalId;

            PaymentMeans paymentMeans = new PaymentMeans();
            paymentMeans.PaymentMeansCode = invoiceData.PaymentDetails.Type;
            paymentMeans.InstructionNote = invoiceData.PaymentDetails.InstructionNote;
            inv.paymentmeans.Add(paymentMeans);

            inv.delivery.ActualDeliveryDate = invoiceData.ActualDeliveryDate;
            inv.delivery.LatestDeliveryDate = invoiceData.LatestDeliveryDate;

            AccountingSupplierParty supplierParty = InvoiceHelper.CreateSupplierParty(companyInfo); 
            inv.SupplierParty = supplierParty;

            AccountingCustomerParty customerParty = InvoiceHelper.CreateCustomerParty(invoiceData.CustomerInformation);

            inv.CustomerParty = customerParty;

            AllowanceCharge allowancecharge = new AllowanceCharge();

            allowancecharge.taxCategory.ID = invoiceData.AllowanceCharge.TaxCategoryId;
            allowancecharge.taxCategory.Percent = invoiceData.AllowanceCharge.TaxCategoryPercent;

            allowancecharge.Amount = invoiceData.AllowanceCharge.TaxCategoryId.Equals("S")
                ? 0
                : invoiceData.AllowanceCharge.Amount;

            allowancecharge.AllowanceChargeReason = invoiceData.AllowanceCharge.Reason;
            inv.allowanceCharges.Add(allowancecharge);

            return inv;
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

        private async Task<InvoiceReportingResponse> CallComplianceInvoiceAPI(ApiRequestLogic apireqlogic,
            CompanyCredentials companyCredentials, Result res)
        {
            InvoiceReportingRequest invrequestbody = new InvoiceReportingRequest
            {
                invoice = res.EncodedInvoice,
                invoiceHash = res.InvoiceHash,
                uuid = res.UUID
            };

            return apireqlogic.CallComplianceInvoiceAPI(companyCredentials.SecretToken, companyCredentials.Secret,
                invrequestbody);
        }

        private SignedInvoice CreateSignedInvoice(Result res, Company company)
        {
            return new SignedInvoice
            {
                UUID = res.UUID,
                InvoiceHash = res.InvoiceHash,
                InvoiceType = "Standard",
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