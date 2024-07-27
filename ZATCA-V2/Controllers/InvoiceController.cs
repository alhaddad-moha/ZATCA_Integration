using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
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


        public InvoiceController(ICompanyCredentialsRepository companyCredentialsRepository,
            ISignedInvoiceRepository signedInvoiceRepository, ICompanyInfoRepository companyInfoRepository,
            ICompanyRepository companyRepository)
        {
            _companyInfoRepository = companyInfoRepository;
            _companyRepository = companyRepository;
            _companyCredentialsRepository = companyCredentialsRepository;
            _signedInvoiceRepository = signedInvoiceRepository;
        }

        [HttpGet("companies/{id}")]
        public async Task<ActionResult<List<SignedInvoice>>> GetByCompanyId(int id)
        {
            var invoices = await _signedInvoiceRepository.GetAllByCompanyId(id);
            return Ok(invoices);
        }

        [HttpGet("generate")]
        public async Task<IActionResult> Generate()
        {
            UBLXML ubl = new UBLXML();
            Invoice inv = new Invoice();
            Result res = new Result();
            inv.ID = "1230"; // مثال SME00010
            inv.UUID = Guid.NewGuid().ToString();
            inv.IssueDate = "2023-05-14";
            inv.IssueTime = "11:25:55";

            inv.invoiceTypeCode.id = 388;
            inv.invoiceTypeCode.Name = "0200000";
            inv.DocumentCurrencyCode = "SAR";
            inv.TaxCurrencyCode = "SAR";

            // هنا ممكن اضيف ال pih من قاعدة البيانات  
            inv.AdditionalDocumentReferencePIH.EmbeddedDocumentBinaryObject =
                "NWZlY2ViNjZmZmM4NmYzOGQ5NTI3ODZjNmQ2OTZjNzljMmRiYzIzOWRkNGU5MWI0NjcyOWQ3M2EyN2ZiNTdlOQ==";
            // قيمة عداد الفاتورة
            inv.AdditionalDocumentReferenceICV.UUID = 123456; // لابد ان يكون ارقام فقط

            inv.delivery.ActualDeliveryDate = "2022-10-22";
            inv.delivery.LatestDeliveryDate = "2022-10-23";
            PaymentMeans paymentMeans = new PaymentMeans();
            paymentMeans.PaymentMeansCode = "10"; //اختيارى
            PaymentMeans paymentMeans1 = new PaymentMeans();
            paymentMeans1.PaymentMeansCode = "42"; //اختيارى
            paymentMeans1.InstructionNote = "Payment Notes"; //اجبارى فى الاشعارات
            //inv.paymentmeans.payeefinancialaccount.ID = "";//اختيارى
            //inv.paymentmeans.payeefinancialaccount.paymentnote = "Payment by credit";//اختيارى
            inv.paymentmeans.Add(paymentMeans);
            inv.paymentmeans.Add(paymentMeans1);

            AccountingCustomerParty customerParty = InvoiceHelper.CreateCustomerParty(
                "123456", "CRN", "Kemarat Street,", "", "3724", "9833", "Jeddah",
                "15385", "Makkah", "Alfalah", "SA", "buyyername", "301121971100003");
            inv.CustomerParty = customerParty;

            inv.legalMonetaryTotal.PayableAmount = 0;
            inv.legalMonetaryTotal.PrepaidAmount = 46;

            AllowanceChargeCollection allowanceCharges = new AllowanceChargeCollection();
            AllowanceCharge allowance = new AllowanceCharge();
            allowance.ChargeIndicator = false;
            //فى حالة انى هاضيف الخصم على مستوى الفاتورة بالنسبة
            allowance.MultiplierFactorNumeric = 0;
            allowance.BaseAmount = 0;

            allowance.Amount = 0; //قيمة الخصم ويكون صفر فى حالة لو هانستخدم النسبة
            allowance.AllowanceChargeReasonCode = ""; //سبب الخصم
            allowance.AllowanceChargeReason = "discount"; //سبب الخصم
            allowance.taxCategory.ID = "S"; // كود الضريبة
            allowance.taxCategory.Percent = 15; // نسبة الضريبة
            //فى حالة عندى اكثر من خصم بعمل loop على الاسطر السابقة
            allowanceCharges.Add(allowance);

            InvoiceLine invline =
                InvoiceHelper.GetInvoiceLine("Item 1", 20, 20, 80, allowanceCharges, "S", 15, true, "", "");
            InvoiceLine invline1 = InvoiceHelper.GetAdditionalInvoiceLine("Item 1", allowanceCharges, "S", 15, "", "");

            inv.InvoiceLines.Add(invline);
            inv.InvoiceLines.Add(invline1);

            string certificateFilePath = "Utils/keys/cert.txt";
            string certificate = System.IO.File.ReadAllText(certificateFilePath);

            string privateKeyPath = "Utils/keys/privateKey-filtered.pem";
            string privateKey = System.IO.File.ReadAllText(privateKeyPath);

            inv.cSIDInfo.CertPem = certificate;
            inv.cSIDInfo.PrivateKey = privateKey;

            InvoiceTotal CalculateInvoiceTotal = ubl.CalculateInvoiceTotal(inv.InvoiceLines, inv.allowanceCharges);

            res = ubl.GenerateInvoiceXML(inv, Directory.GetCurrentDirectory());
            if (res.IsValid)
            {
                return Ok(res);
            }

            int companyId = 1; // Replace with your actual companyId

            var companyCredentials = await _companyCredentialsRepository.GetLatestByCompanyId(companyId);
            if (companyCredentials != null)
            {
                ApiRequestLogic apireqlogic = new ApiRequestLogic(Mode.developer);
                InvoiceReportingRequest invrequestbody = new InvoiceReportingRequest();


                invrequestbody.invoice = res.EncodedInvoice;
                invrequestbody.invoiceHash = res.InvoiceHash;
                invrequestbody.uuid = res.UUID;
                InvoiceReportingResponse invoicereportingmodel =
                    apireqlogic.CallComplianceInvoiceAPI(companyCredentials.SecretToken, companyCredentials.Secret,
                        invrequestbody);

                if (string.IsNullOrEmpty(invoicereportingmodel.ErrorMessage))
                {
                    return Ok(invoicereportingmodel);

                    return Ok("reported");
                }
                else
                {
                    return BadRequest(invoicereportingmodel.ErrorMessage);
                }
            }

            else

            {
                return BadRequest("No Credentials Found");
            }
        }

        [HttpPost("simplified")]
        public async Task<IActionResult> GenerateSimpleInvoices()
        {
            return Ok();
        }

        [HttpPost("sign-single")]
        public async Task<IActionResult> SignSingleInvoice(SingleInvoiceRequest singleInvoiceRequest)
        {
            try
            {
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

                Invoice inv = CreateMainInvoice(singleInvoiceRequest.InvoiceType!, singleInvoiceRequest.Invoice,
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
                    await CallComplianceInvoiceAPI(apireqlogic, companyCredentials, res);

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
            catch (Exception ex)
            {
                // Log the exception (you might want to use a logging framework here)
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        [HttpPost("sign")]
        public async Task<IActionResult> GenerateDynamicStandard(BulkInvoiceRequest bulkInvoiceRequest)
        {
            try
            {
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
                    Invoice inv = CreateMainInvoice(bulkInvoiceRequest.InvoicesType!, invoiceData, companyInfo!);
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

                    SignedInvoice signedInvoice = CreateSignedInvoice(res, company,bulkInvoiceRequest.InvoicesType.Name);

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
                        responses.Add(invoicereportingmodel);
                    }
                }

                return Ok(responses);
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
                invoiceData.CustomerInformation.CommercialNumber,
                invoiceData.CustomerInformation.CommercialNumberType,
                invoiceData.CustomerInformation.Address.StreetName,
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

        private async Task<InvoiceReportingResponse> CallComplianceInvoiceAPI(ApiRequestLogic apireqlogic,
            CompanyCredentials companyCredentials, Result res)
        {
            InvoiceReportingRequest invrequestbody = new InvoiceReportingRequest
            {
                invoice = res.EncodedInvoice,
                invoiceHash = res.InvoiceHash,
                uuid = res.UUID
            };
            var response = apireqlogic.CallComplianceInvoiceAPI(companyCredentials.SecretToken,
                companyCredentials.Secret, invrequestbody);

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