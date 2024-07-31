using System.ComponentModel.DataAnnotations;
using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using ZATCA_V2.CustomValidators;
using ZATCA_V2.Exceptions;
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
        private readonly IValidator<BulkInvoiceRequest> _bulkInvoiceRequestValidator;
        private readonly IValidator<SingleInvoiceRequest> _signInvoiceRequestValidator;


        public InvoiceController(ICompanyCredentialsRepository companyCredentialsRepository,
            ISignedInvoiceRepository signedInvoiceRepository, ICompanyInfoRepository companyInfoRepository,
            ICompanyRepository companyRepository, IValidator<BulkInvoiceRequest> bulkInvoiceRequestValidator,
            IValidator<SingleInvoiceRequest> signInvoiceRequestValidator)
        {
            _companyInfoRepository = companyInfoRepository;
            _companyRepository = companyRepository;
            _bulkInvoiceRequestValidator = bulkInvoiceRequestValidator;
            _signInvoiceRequestValidator = signInvoiceRequestValidator;
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
                var validator = await _signInvoiceRequestValidator.ValidateAsync(singleInvoiceRequest);
                if (!validator.IsValid)
                {
                    throw new CustomValidationException(validator);
                }

                var company = await _companyRepository.GetById(singleInvoiceRequest.CompanyId);
                if (company == null)
                {
                    return BadRequest("Company Not Found");
                }


                var companyInfo = await _companyInfoRepository.GetByCompanyId(singleInvoiceRequest.CompanyId);
                var companyCredentials =
                    await _companyCredentialsRepository.GetLatestByCompanyId(singleInvoiceRequest.CompanyId);

                if (companyCredentials == null)
                {
                    return BadRequest("companyCredentials Not Found");
                }

                if (singleInvoiceRequest.Invoice == null)
                {
                    return BadRequest("Invoice Data Not Provided");
                }

                var latestInvoice = await _signedInvoiceRepository.GetLatestByCompanyId(singleInvoiceRequest.CompanyId);
                string invoiceHash = latestInvoice == null ? "112345" : latestInvoice.InvoiceHash;
                UBLXML ubl = new UBLXML();
                ApiRequestLogic apireqlogic = new ApiRequestLogic(Mode.developer);

                Invoice inv = CreateMainInvoice(singleInvoiceRequest.InvoiceType, singleInvoiceRequest.Invoice,
                    companyInfo!);
                Result res = new Result();

                foreach (var invoiceItem in singleInvoiceRequest.Invoice.InvoiceItems!)
                {
                    InvoiceLine invoiceLine = InvoiceHelper.CreateInvoiceLine(
                        invoiceItem.Name, invoiceItem.Quantity, invoiceItem.BaseQuantity,
                        invoiceItem.Price, inv.allowanceCharges, invoiceItem.TaxCategory,
                        invoiceItem.VatPercentage, invoiceItem.IsIncludingVat, invoiceItem.TaxExemptionReasonCode,
                        invoiceItem.TaxExemptionReason);

                    inv.InvoiceLines.Add(invoiceLine);
                }

                inv.cSIDInfo.CertPem = companyCredentials.Certificate;
                inv.cSIDInfo.PrivateKey = companyCredentials.PrivateKey;
                inv.AdditionalDocumentReferencePIH.EmbeddedDocumentBinaryObject = invoiceHash;

                res = ubl.GenerateInvoiceXML(inv, Directory.GetCurrentDirectory());
                if (!res.IsValid)
                {
                    return BadRequest(res);
                }

                SignedInvoice signedInvoice = CreateSignedInvoice(res, company, singleInvoiceRequest.InvoiceType.Name);

                InvoiceReportingRequest invrequestbody = new InvoiceReportingRequest
                {
                    invoice = res.EncodedInvoice,
                    invoiceHash = res.InvoiceHash,
                    uuid = res.UUID
                };

                InvoiceReportingResponse invoicereportingmodel =
                    await SendInvoiceToZATCA(apireqlogic, companyCredentials, res, inv);

                if (!string.IsNullOrEmpty(invoicereportingmodel.ErrorMessage))
                {
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
                FluentValidation.Results.ValidationResult validationResult =
                    await _bulkInvoiceRequestValidator.ValidateAsync(bulkInvoiceRequest);
                if (!validationResult.IsValid)
                {
                    var errors = validationResult.Errors
                        .GroupBy(e => e.PropertyName)
                        .ToDictionary(
                            g => g.Key,
                            g => g.Select(e => e.ErrorMessage).ToArray()
                        );

                    // Create a problem details object
                    var problemDetails = new ProblemDetails
                    {
                        Status = 400,
                        Title = "Validation Error",
                        Detail = "One or more validation errors occurred. test",
                        Instance = HttpContext?.TraceIdentifier // Optional: Use trace identifier for request tracking
                    };

                    // Add errors to the problem details
                    problemDetails.Extensions["errors"] = errors;

                    return BadRequest(problemDetails);
                }

                var company = await _companyRepository.GetById(bulkInvoiceRequest.companyId);
                if (company == null)
                {
                    return BadRequest("Company Not Found");
                }

                var companyInfo = await _companyInfoRepository.GetByCompanyId(bulkInvoiceRequest.companyId);
                var companyCredentials =
                    await _companyCredentialsRepository.GetLatestByCompanyId(bulkInvoiceRequest.companyId);

                if (companyCredentials == null)
                {
                    return BadRequest("companyCredentials Not Found");
                }

                var latestInvoice = await _signedInvoiceRepository.GetLatestByCompanyId(bulkInvoiceRequest.companyId);
                string invoiceHash = latestInvoice == null ? "112345" : latestInvoice.InvoiceHash;

                UBLXML ubl = new UBLXML();
                ApiRequestLogic apireqlogic = new ApiRequestLogic(Mode.developer);

                List<object> responses = new List<object>();
                foreach (var invoiceData in bulkInvoiceRequest.Invoices!)
                {
                    Invoice inv = CreateMainInvoice(bulkInvoiceRequest.InvoicesType, invoiceData, companyInfo!);
                    Result res = new Result();

                    foreach (var invoiceItem in invoiceData.InvoiceItems!)
                    {
                        InvoiceLine invoiceLine = InvoiceHelper.CreateInvoiceLine(
                            invoiceItem.Name, invoiceItem.Quantity, invoiceItem.BaseQuantity,
                            invoiceItem.Price, inv.allowanceCharges, invoiceItem.TaxCategory,
                            invoiceItem.VatPercentage, invoiceItem.IsIncludingVat, invoiceItem.TaxExemptionReasonCode,
                            invoiceItem.TaxExemptionReason);

                        inv.InvoiceLines.Add(invoiceLine);
                    }

                    inv.cSIDInfo.CertPem = companyCredentials.Certificate;
                    inv.cSIDInfo.PrivateKey = companyCredentials.PrivateKey;
                    inv.AdditionalDocumentReferencePIH.EmbeddedDocumentBinaryObject = invoiceHash;

                    res = ubl.GenerateInvoiceXML(inv, Directory.GetCurrentDirectory());
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
                        CreateSignedInvoice(res, company, bulkInvoiceRequest.InvoicesType.Name);

                    InvoiceReportingRequest invrequestbody = new InvoiceReportingRequest
                    {
                        invoice = res.EncodedInvoice,
                        invoiceHash = res.InvoiceHash,
                        uuid = res.UUID
                    };

                    InvoiceReportingResponse invoicereportingmodel =
                        await SendInvoiceToZATCA(apireqlogic, companyCredentials, res, inv);

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
                        responses.Add(invoicereportingmodel);
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


        private Invoice CreateMainInvoice(InvoiceType invoicesType, InvoiceData invoiceData, CompanyInfo companyInfo)
        {
            Invoice inv = new Invoice();

            if (invoicesType == null)
            {
                Console.WriteLine("Invoice Type is Null");
            }

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

            AccountingCustomerParty customerParty = InvoiceHelper.CreateCustomerParty(
                invoiceData!.CustomerInformation.CommercialNumber!,
                invoiceData.CustomerInformation.CommercialNumberType,
                invoiceData.CustomerInformation.Address!.StreetName,
                invoiceData.CustomerInformation.Address.AdditionalStreetName,
                invoiceData.CustomerInformation.Address.BuildingNumber,
                invoiceData.CustomerInformation.Address.PlotIdentification,
                invoiceData.CustomerInformation.Address.CityName,
                invoiceData.CustomerInformation.Address.PostalZone,
                invoiceData.CustomerInformation.Address.CountrySubentity,
                invoiceData.CustomerInformation.Address.CitySubdivisionName,
                invoiceData.CustomerInformation.Address.IdentificationCode,
                invoiceData.CustomerInformation.RegistrationName,
                invoiceData.CustomerInformation.RegistrationNumber
            );

            inv.CustomerParty = customerParty;

            AllowanceCharge allowancecharge = new AllowanceCharge();

            allowancecharge.taxCategory.ID = invoiceData.AllowanceCharge.TaxCategory;
            allowancecharge.taxCategory.Percent = invoiceData.AllowanceCharge.TaxCategoryPercent;

            allowancecharge.Amount = invoiceData.AllowanceCharge.TaxCategory.Equals("S")
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

        private async Task<InvoiceReportingResponse> SendInvoiceToZATCA(ApiRequestLogic apireqlogic,
            CompanyCredentials companyCredentials, Result res, Invoice invoice)
        {
            string invoiceType = invoice.invoiceTypeCode.Name;
            InvoiceReportingRequest invrequestbody = new InvoiceReportingRequest
            {
                invoice = res.EncodedInvoice,
                invoiceHash = res.InvoiceHash,
                uuid = res.UUID
            };
            var mode = Mode.developer;
            bool isStandardInvoice = invoiceType.Substring(0, 2) == "01";
            var response = new InvoiceReportingResponse();
            switch (mode)
            {
                case Mode.developer:
                    response = apireqlogic.CallComplianceInvoiceAPI(companyCredentials.SecretToken,
                        companyCredentials.Secret, invrequestbody);
                    break;
                case Mode.Simulation:
                    if (isStandardInvoice)
                    {
                    }
                    else
                    {
                        response = apireqlogic.CallReportingAPI(companyCredentials.SecretToken,
                            companyCredentials.Secret, invrequestbody);
                    }

                    break;
                case Mode.Production:
                    break;
                default:
                    break;
            }


            // Ensure that the response object is properly initialized
            response.ErrorMessage ??= string.Empty;

            return response;
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