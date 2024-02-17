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
    public class TestInvoiceController : ControllerBase
    {
        private readonly ICompanyInfoRepository _companyInfoRepository;
        private readonly ICompanyRepository _companyRepository;
        private readonly ISignedInvoiceRepository _signedInvoiceRepository;
        private readonly ICompanyCredentialsRepository _companyCredentialsRepository;

        public TestInvoiceController(ICompanyInfoRepository companyInfoRepository,
            ICompanyRepository companyRepository,
            ICompanyCredentialsRepository companyCredentialsRepository,
            ISignedInvoiceRepository signedInvoiceRepository)
        {
            _companyInfoRepository = companyInfoRepository;
            _companyRepository = companyRepository;
            _signedInvoiceRepository = signedInvoiceRepository;
            _companyCredentialsRepository = companyCredentialsRepository;
        }

        [HttpGet("generate")]
        public IActionResult Generate()
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

            inv.billingReference.InvoiceDocumentReferenceID = "123654";
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

            AccountingSupplierParty supplierParty = InvoiceHelper.CreateSupplierParty(
                "1515325162", "CRN", "streetnumber", "ststtstst", "3724", "9833", "gaddah",
                "15385", "makka", "flassk", "SA", "Mod Co", "300068256300003");


            inv.SupplierParty = supplierParty;
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
            else
            {
                // return BadRequest(res);
            }

            ApiRequestLogic apireqlogic = new ApiRequestLogic(Mode.developer);
            ComplianceCsrResponse tokenresponse = new ComplianceCsrResponse();
            InvoiceReportingRequest invrequestbody = new InvoiceReportingRequest();

            string csr = Helper.ReadFileToString(Constants.CSR_Path);

            tokenresponse = apireqlogic.GetComplianceCSIDAPI("123456", csr);
            if (string.IsNullOrEmpty(tokenresponse.ErrorMessage))
            {
                invrequestbody.invoice = res.EncodedInvoice;
                invrequestbody.invoiceHash = res.InvoiceHash;
                invrequestbody.uuid = res.UUID;
                InvoiceReportingResponse invoicereportingmodel =
                    apireqlogic.CallComplianceInvoiceAPI(tokenresponse.BinarySecurityToken, tokenresponse.Secret,
                        invrequestbody);
                //InvoiceReportingResponse invoicereportingmodel = apireqlogic.CallReportingAPI(Utility.ToBase64Encode(inv.cSIDInfo.CertPem), "cuqeJ5yQPoGInAF4MrynTQYOIwAYXN1jhpjFgRkga04=", invrequestbody);

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
                return Ok(tokenresponse.ErrorMessage);
            }
        }

        [HttpPost("standard")]
        public async Task<IActionResult> GenerateStandard(BulkInvoiceRequest bulkInvoiceRequest)
        {
            var companyInfo = await _companyInfoRepository.GetByCompanyId(bulkInvoiceRequest.companyId);
            var company = await _companyRepository.GetById(bulkInvoiceRequest.companyId);
            var companyCredentials =
                await _companyCredentialsRepository.GetLatestByCompanyId(bulkInvoiceRequest.companyId);

            if (company == null)
            {
                return BadRequest("Company Not Found");
            }

            if (companyCredentials == null)
            {
                return BadRequest("companyCredentials Not Found");
            }


            UBLXML ubl = new UBLXML();
            Invoice inv = new Invoice();
            Result res = new Result();

            inv.ID = "1230"; // مثال SME00010
            inv.IssueDate = "2021-01-05";
            inv.IssueTime = "09:32:40";
            //388 فاتورة  
            //383 اشعار مدين
            //381 اشعار دائن
            inv.invoiceTypeCode.id = 388;
            //inv.invoiceTypeCode.Name based on format NNPNESB
            //NN 01 فاتورة عادية
            //NN 02 فاتورة مبسطة
            //P فى حالة فاتورة لطرف ثالث نكتب 1 فى الحالة الاخرى نكتب 0
            //N فى حالة فاتورة اسمية نكتب 1 وفى الحالة الاخرى نكتب 0
            //E فى حالة فاتورة للصادرات نكتب 1 وفى الحالة الاخرى نكتب 0
            //S فى حالة فاتورة ملخصة نكتب 1 وفى الحالة الاخرى نكتب 0
            //B فى حالة فاتورة ذاتية نكتب 1
            //B فى حالة ان الفاتورة صادرات=1 لايمكن ان تكون الفاتورة ذاتية =1
            //
            inv.invoiceTypeCode.Name = "0100000";
            inv.DocumentCurrencyCode = "SAR"; //العملة
            inv.TaxCurrencyCode = "SAR"; ////فى حالة الدولار لابد ان تكون عملة الضريبة بالريال السعودى
            //inv.CurrencyRate = decimal.Parse("3.75"); // قيمة الدولار مقابل الريال
            // فى حالة ان اشعار دائن او مدين فقط هانكتب رقم الفاتورة اللى اصدرنا الاشعار ليها
            inv.billingReference.InvoiceDocumentReferenceID = "123654";
            // هنا ممكن اضيف ال pih من قاعدة البيانات  
            inv.AdditionalDocumentReferencePIH.EmbeddedDocumentBinaryObject =
                "NWZlY2ViNjZmZmM4NmYzOGQ5NTI3ODZjNmQ2OTZjNzljMmRiYzIzOWRkNGU5MWI0NjcyOWQ3M2EyN2ZiNTdlOQ==";
            // قيمة عداد الفاتورة
            inv.AdditionalDocumentReferenceICV.UUID = 123456; // لابد ان يكون ارقام فقط
            //فى حالة فاتورة مبسطة وفاتورة ملخصة هانكتب تاريخ التسليم واخر تاريخ التسليم
            // inv.delivery.ActualDeliveryDate = "2022-10-22";
            // inv.delivery.LatestDeliveryDate = "2022-10-23";
            //
            //بيانات الدفع 
            // اكواد معين
            // اختيارى كود الدفع
            //inv.paymentmeans.PaymentMeansCode = "48";//اختيارى
            //inv.paymentmeans.InstructionNote = "Purchase"; //اجبارى فى الاشعارات
            // inv.paymentmeans.payeefinancialaccount.ID = "";//اختيارى
            // inv.paymentmeans.payeefinancialaccount.paymentnote = "Payment by credit";//اختيارى
            //بيانات البائع

            inv.delivery.ActualDeliveryDate = "2022-10-22";
            inv.delivery.LatestDeliveryDate = "2022-10-23";
            AccountingSupplierParty supplierParty = InvoiceHelper.CreateSupplierParty(
                companyInfo.PartyId.ToString(), companyInfo.SchemeID, companyInfo.StreetName,
                companyInfo.AdditionalStreetName, companyInfo.BuildingNumber,
                companyInfo.PlotIdentification, companyInfo.CityName, companyInfo.PostalZone,
                companyInfo.CountrySubentity,
                companyInfo.CitySubdivisionName, companyInfo.IdentificationCode, companyInfo.RegistrationName,
                companyInfo.taxRegistrationNumber);
            inv.SupplierParty = supplierParty;
            // بيانات المشترى اجبارى
            inv.CustomerParty.partyIdentification.ID = "123456"; //رقم السجل التجارى
            inv.CustomerParty.partyIdentification.schemeID = "CRN";
            inv.CustomerParty.postalAddress.StreetName = "Kemarat Street,"; // اجبارى
            inv.CustomerParty.postalAddress.AdditionalStreetName = ""; //اختيارى
            inv.CustomerParty.postalAddress.BuildingNumber = "3724"; // اجبارى
            inv.CustomerParty.postalAddress.PlotIdentification = "9833"; //اختيارى
            inv.CustomerParty.postalAddress.CityName = "Jeddah";
            inv.CustomerParty.postalAddress.PostalZone = "15385";
            inv.CustomerParty.postalAddress.CountrySubentity = "Makkah"; // اختيارى
            inv.CustomerParty.postalAddress.CitySubdivisionName = "Alfalah";
            inv.CustomerParty.postalAddress.country.IdentificationCode = "SA";
            inv.CustomerParty.partyLegalEntity.RegistrationName = "First Shop";
            inv.CustomerParty.partyTaxScheme.CompanyID = "301121971100003";

            AllowanceCharge allowancecharge = new AllowanceCharge();
            allowancecharge.Amount = 2;
            allowancecharge.AllowanceChargeReason = "discount"; //reason
            allowancecharge.taxCategory.ID = "S";
            allowancecharge.taxCategory.Percent = 15;
            inv.allowanceCharges.Add(allowancecharge);
            //لابد من ادخال المبلغ المدفوع والمبلغ المتبقى
            inv.legalMonetaryTotal.PayableAmount = 1822.43m;
            inv.legalMonetaryTotal.PrepaidAmount = 0;
            // فى حالة فى اكتر من منتج فى الفاتورة هانعمل ليست من invoiceline مثال الكود التالى
            //for(int i=0;i<13;i++)
            //{
            InvoiceLine invline = new InvoiceLine();
            invline.item.Name = "item no 1";
            invline.InvoiceQuantity = 2; // 
            invline.price.PriceAmount = 396.68m; // سعر المنتج  

            invline.item.classifiedTaxCategory.ID = "S"; // كود الضريبة
            invline.item.classifiedTaxCategory.Percent = 15; // نسبة الضريبة

            //if the price is including vat set EncludingVat=true;
            //invline.price.EncludingVat = true;

            //invline.price.PriceAmount = 70;// سعر المنتج بعد الخصم 

            //invline.item.Name = "Computer";


            invline.taxTotal.TaxSubtotal.taxCategory.ID = "S"; //كود الضريبة
            invline.taxTotal.TaxSubtotal.taxCategory.Percent = 15; //نسبة الضريبة

            inv.InvoiceLines.Add(invline);


            InvoiceLine invline1 = new InvoiceLine();
            invline1.item.Name = "item no 2";
            invline1.InvoiceQuantity = 2; // 
            invline1.price.PriceAmount = 396.68m; // سعر المنتج  

            invline1.item.classifiedTaxCategory.ID = "S"; // كود الضريبة
            invline1.item.classifiedTaxCategory.Percent = 15; // نسبة الضريبة

            //if the price is including vat set EncludingVat=true;
            //invline.price.EncludingVat = true;

            //invline.price.PriceAmount = 70;// سعر المنتج بعد الخصم 

            //invline.item.Name = "Computer";


            invline1.taxTotal.TaxSubtotal.taxCategory.ID = "S"; //كود الضريبة
            invline1.taxTotal.TaxSubtotal.taxCategory.Percent = 15; //نسبة الضريبة

            inv.InvoiceLines.Add(invline);

            // here you can pass csid data

            // 
            /*
            inv.cSIDInfo.CertPem =
                @"MIID9jCCA5ugAwIBAgITbwAAeCy9aKcLA99HrAABAAB4LDAKBggqhkjOPQQDAjBjMRUwEwYKCZImiZPyLGQBGRYFbG9jYWwxEzARBgoJkiaJk/IsZAEZFgNnb3YxFzAVBgoJkiaJk/IsZAEZFgdleHRnYXp0MRwwGgYDVQQDExNUU1pFSU5WT0lDRS1TdWJDQS0xMB4XDTIyMDQxOTIwNDkwOVoXDTI0MDQxODIwNDkwOVowWTELMAkGA1UEBhMCU0ExEzARBgNVBAoTCjMxMjM0NTY3ODkxDDAKBgNVBAsTA1RTVDEnMCUGA1UEAxMeVFNULS05NzA1NjAwNDAtMzEyMzQ1Njc4OTAwMDAzMFYwEAYHKoZIzj0CAQYFK4EEAAoDQgAEYYMMoOaFYAhMO/steotfZyavr6p11SSlwsK9azmsLY7b1b+FLhqMArhB2dqHKboxqKNfvkKDePhpqjui5hcn0aOCAjkwggI1MIGaBgNVHREEgZIwgY+kgYwwgYkxOzA5BgNVBAQMMjEtVFNUfDItVFNUfDMtNDdmMTZjMjYtODA2Yi00ZTE1LWIyNjktN2E4MDM4ODRiZTljMR8wHQYKCZImiZPyLGQBAQwPMzEyMzQ1Njc4OTAwMDAzMQ0wCwYDVQQMDAQxMTAwMQwwCgYDVQQaDANUU1QxDDAKBgNVBA8MA1RTVDAdBgNVHQ4EFgQUO5ZiU7NakU3eejVa3I2S1B2sDwkwHwYDVR0jBBgwFoAUdmCM+wagrGdXNZ3PmqynK5k1tS8wTgYDVR0fBEcwRTBDoEGgP4Y9aHR0cDovL3RzdGNybC56YXRjYS5nb3Yuc2EvQ2VydEVucm9sbC9UU1pFSU5WT0lDRS1TdWJDQS0xLmNybDCBrQYIKwYBBQUHAQEEgaAwgZ0wbgYIKwYBBQUHMAGGYmh0dHA6Ly90c3RjcmwuemF0Y2EuZ292LnNhL0NlcnRFbnJvbGwvVFNaRWludm9pY2VTQ0ExLmV4dGdhenQuZ292LmxvY2FsX1RTWkVJTlZPSUNFLVN1YkNBLTEoMSkuY3J0MCsGCCsGAQUFBzABhh9odHRwOi8vdHN0Y3JsLnphdGNhLmdvdi5zYS9vY3NwMA4GA1UdDwEB/wQEAwIHgDAdBgNVHSUEFjAUBggrBgEFBQcDAgYIKwYBBQUHAwMwJwYJKwYBBAGCNxUKBBowGDAKBggrBgEFBQcDAjAKBggrBgEFBQcDAzAKBggqhkjOPQQDAgNJADBGAiEA7mHT6yg85jtQGWp3M7tPT7Jk2+zsvVHGs3bU5Z7YE68CIQD60ebQamYjYvdebnFjNfx4X4dop7LsEBFCNSsLY0IFaQ==";
            inv.cSIDInfo.PrivateKey =
                @"MHQCAQEEIDyLDaWIn/1/g3PGLrwupV4nTiiLKM59UEqUch1vDfhpoAcGBSuBBAAKoUQDQgAEYYMMoOaFYAhMO/steotfZyavr6p11SSlwsK9azmsLY7b1b+FLhqMArhB2dqHKboxqKNfvkKDePhpqjui5hcn0Q==";
            */
            //for simulation
            //inv.cSIDInfo.CertPem = @"MIIE7zCCBJagAwIBAgITGQAABb3G/XE3/Vof4AAAAAAFvTAKBggqhkjOPQQDAjBiMRUwEwYKCZImiZPyLGQBGRYFbG9jYWwxEzARBgoJkiaJk/IsZAEZFgNnb3YxFzAVBgoJkiaJk/IsZAEZFgdleHRnYXp0MRswGQYDVQQDExJQRVpFSU5WT0lDRVNDQTMtQ0EwHhcNMjMwNDE4MTAyMjE1WhcNMjMwODA4MTIyNzAxWjBVMQswCQYDVQQGEwJTQTEjMCEGA1UEChMaQWwtS2hhZmppIEpvaW50IE9wZXJhdGlvbnMxEzARBgNVBAsTCjMxMDE5Nzk4ODExDDAKBgNVBAMTA1NBUDBWMBAGByqGSM49AgEGBSuBBAAKA0IABF9rSkNM58lOJBl1K8uGAxooHnR3Ffrxi2NGN2+NURfoD6uUsqm/CLXEOefmQRKjQewRD3laLDUHIdlt6HbOsFqjggM5MIIDNTAnBgkrBgEEAYI3FQoEGjAYMAoGCCsGAQUFBwMCMAoGCCsGAQUFBwMDMDwGCSsGAQQBgjcVBwQvMC0GJSsGAQQBgjcVCIGGqB2E0PsShu2dJIfO+xnTwFVmgZzYLYPlxV0CAWQCARMwgc0GCCsGAQUFBwEBBIHAMIG9MIG6BggrBgEFBQcwAoaBrWxkYXA6Ly8vQ049UEVaRUlOVk9JQ0VTQ0EzLUNBLENOPUFJQSxDTj1QdWJsaWMlMjBLZXklMjBTZXJ2aWNlcyxDTj1TZXJ2aWNlcyxDTj1Db25maWd1cmF0aW9uLERDPWV4dGdhenQsREM9Z292LERDPWxvY2FsP2NBQ2VydGlmaWNhdGU/YmFzZT9vYmplY3RDbGFzcz1jZXJ0aWZpY2F0aW9uQXV0aG9yaXR5MB0GA1UdDgQWBBRz6ypyKnuve8xaVMLAIoNMtB4D6DAOBgNVHQ8BAf8EBAMCB4AwgagGA1UdEQSBoDCBnaSBmjCBlzE7MDkGA1UEBAwyMS1TQVB8Mi1FUlB8My05MmM4NTE1MC1kODA5LTRjYTctYmRhOC0zMzU4ZGM0ZTRhNzcxHzAdBgoJkiaJk/IsZAEBDA8zMTAxOTc5ODgxMDAwMDMxDTALBgNVBAwMBDExMDAxEjAQBgNVBBoMCUFsIGtoYWZqaTEUMBIGA1UEDwwLT2lsIGFuZCBHYXMwgeEGA1UdHwSB2TCB1jCB06CB0KCBzYaBymxkYXA6Ly8vQ049UEVaRUlOVk9JQ0VTQ0EzLUNBLENOPVBFWkVpbnZvaWNlc2NhMyxDTj1DRFAsQ049UHVibGljJTIwS2V5JTIwU2VydmljZXMsQ049U2VydmljZXMsQ049Q29uZmlndXJhdGlvbixEQz1leHRnYXp0LERDPWdvdixEQz1sb2NhbD9jZXJ0aWZpY2F0ZVJldm9jYXRpb25MaXN0P2Jhc2U/b2JqZWN0Q2xhc3M9Y1JMRGlzdHJpYnV0aW9uUG9pbnQwHwYDVR0jBBgwFoAUBPcGVSzJVo6t7h63943uhUOTOtswHQYDVR0lBBYwFAYIKwYBBQUHAwIGCCsGAQUFBwMDMAoGCCqGSM49BAMCA0cAMEQCIEnC4qv197vbIcwguN90HdFl8ZAI2TVS7kWWyKxT6dSxAiAu+doREITlZU+FbQEdpxSmjy64gbSUEDw3JAkIVDlpxQ==";
            //inv.cSIDInfo.PrivateKey = @"MHQCAQEEIDEl8nrkVEEiGseRs/EU5kX7+rWzjy9ZK5UZ8x1L7xq3oAcGBSuBBAAKoUQDQgAEX2tKQ0znyU4kGXUry4YDGigedHcV+vGLY0Y3b41RF+gPq5Syqb8ItcQ55+ZBEqNB7BEPeVosNQch2W3ods6wWg==";
            inv.cSIDInfo.CertPem = companyCredentials.Certificate;
            inv.cSIDInfo.PrivateKey = companyCredentials.PrivateKey;

            SignedInvoice signedInvoice = null;
            res = ubl.GenerateInvoiceXML(inv, Directory.GetCurrentDirectory());
            if (res.IsValid)
            {
                //return Ok(res.InvoiceHash);
                //return Ok(res.SingedXML);
                //return Ok(res.EncodedInvoice);
                //return Ok(res.UUID);
                //return Ok(res.QRCode);
                //return Ok(res.PIH);
                //return Ok(res.SingedXMLFileName);
            }
            else
            {
                //return BadRequest(res);
            }

            signedInvoice = new SignedInvoice
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

            ApiRequestLogic apireqlogic = new ApiRequestLogic(Mode.developer);
            ComplianceCsrResponse tokenresponse = new ComplianceCsrResponse();

            InvoiceReportingRequest invrequestbody = new InvoiceReportingRequest();

            invrequestbody.invoice = res.EncodedInvoice;
            invrequestbody.invoiceHash = res.InvoiceHash;
            invrequestbody.uuid = res.UUID;
            InvoiceReportingResponse invoicereportingmodel =
                apireqlogic.CallComplianceInvoiceAPI(companyCredentials.SecretToken, companyCredentials.Secret,
                    invrequestbody);
            //for production

            //InvoiceClearanceResponse invoicereportingmodel = apireqlogic.CallClearanceAPI(Utility.ToBase64Encode(inv.cSIDInfo.CertPem), "cuqeJ5yQPoGInAF4MrynTQYOIwAYXN1jhpjFgRkga04=", invrequestbody);

            if (string.IsNullOrEmpty(invoicereportingmodel.ErrorMessage))
            {
                await _signedInvoiceRepository.Create(signedInvoice);

                return Ok(new
                {
                    ZATCA = invoicereportingmodel,
                    Res = res
                });
            }
            else
            {
                return Ok(invoicereportingmodel);
            }
        }

        [HttpPost("standard-dynamic")]
        public async Task<IActionResult> GenerateDynamicStandard(BulkInvoiceRequest bulkInvoiceRequest)
        {
            var companyInfo = await _companyInfoRepository.GetByCompanyId(bulkInvoiceRequest.companyId);
            var company = await _companyRepository.GetById(bulkInvoiceRequest.companyId);
            var companyCredentials =
                await _companyCredentialsRepository.GetLatestByCompanyId(bulkInvoiceRequest.companyId);

            if (company == null)
            {
                return BadRequest("Company Not Found");
            }

            if (companyCredentials == null)
            {
                return BadRequest("companyCredentials Not Found");
            }


            UBLXML ubl = new UBLXML();
            ApiRequestLogic apireqlogic = new ApiRequestLogic(Mode.developer);

            List<object> responses = new List<object>();
            foreach (var invoiceData in bulkInvoiceRequest.Invoices)
            {
                Invoice inv = new Invoice();
                Result res = new Result();


                inv.ID = invoiceData.Id;
                inv.IssueDate = invoiceData.IssueDate;
                inv.IssueTime = invoiceData.IssueTime;

                inv.invoiceTypeCode.id = bulkInvoiceRequest.InvoicesType!.Id;

                inv.invoiceTypeCode.Name = bulkInvoiceRequest.InvoicesType!.Name;
                inv.DocumentCurrencyCode = bulkInvoiceRequest.InvoicesType!.DocumentCurrencyCode;

                inv.TaxCurrencyCode =
                    bulkInvoiceRequest.InvoicesType!
                        .TaxCurrencyCode; ////فى حالة الدولار لابد ان تكون عملة الضريبة بالريال السعودى
                //inv.CurrencyRate = decimal.Parse("3.75"); // قيمة الدولار مقابل الريال
                // فى حالة ان اشعار دائن او مدين فقط هانكتب رقم الفاتورة اللى اصدرنا الاشعار ليها
                inv.billingReference.InvoiceDocumentReferenceID = invoiceData.InvoiceDocumentReferenceID;
                // هنا ممكن اضيف ال pih من قاعدة البيانات  
                //TODO Change this
                inv.AdditionalDocumentReferencePIH.EmbeddedDocumentBinaryObject =
                    "NWZlY2ViNjZmZmM4NmYzOGQ5NTI3ODZjNmQ2OTZjNzljMmRiYzIzOWRkNGU5MWI0NjcyOWQ3M2EyN2ZiNTdlOQ==";
                // قيمة عداد الفاتورة
                inv.AdditionalDocumentReferenceICV.UUID = invoiceData.AddtionalId;
                //بيانات الدفع 
                // اكواد معين
                // اختيارى كود الدفع
                //inv.paymentmeans.PaymentMeansCode = "48";//اختيارى
                //inv.paymentmeans.InstructionNote = "Purchase"; //اجبارى فى الاشعارات
                // inv.paymentmeans.payeefinancialaccount.ID = "";//اختيارى
                // inv.paymentmeans.payeefinancialaccount.paymentnote = "Payment by credit";//اختيارى
                //بيانات البائع
                PaymentMeans paymentMeans = new PaymentMeans();
                paymentMeans.PaymentMeansCode = invoiceData.PaymentDetails.Type;
                paymentMeans.InstructionNote = invoiceData.PaymentDetails.InstructionNote;
                inv.paymentmeans.Add(paymentMeans);

                inv.delivery.ActualDeliveryDate = invoiceData.ActualDeliveryDate;
                inv.delivery.LatestDeliveryDate = invoiceData.LatestDeliveryDate;

                AccountingSupplierParty supplierParty = InvoiceHelper.CreateSupplierParty(
                    companyInfo.PartyId.ToString(), companyInfo.SchemeID, companyInfo.StreetName,
                    companyInfo.AdditionalStreetName, companyInfo.BuildingNumber,
                    companyInfo.PlotIdentification, companyInfo.CityName, companyInfo.PostalZone,
                    companyInfo.CountrySubentity,
                    companyInfo.CitySubdivisionName, companyInfo.IdentificationCode, companyInfo.RegistrationName,
                    companyInfo.taxRegistrationNumber);

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

                allowancecharge.taxCategory.ID = invoiceData.AllowanceCharge.TaxCategoryId;
                allowancecharge.taxCategory.Percent = invoiceData.AllowanceCharge.TaxCategoryPercent;

                allowancecharge.Amount = invoiceData.AllowanceCharge.TaxCategoryId.Equals("S")
                    ? 0
                    : invoiceData.AllowanceCharge.Amount;

                allowancecharge.AllowanceChargeReason = invoiceData.AllowanceCharge.Reason;
                inv.allowanceCharges.Add(allowancecharge);

                /*inv.legalMonetaryTotal.PrepaidAmount = invoiceData.LegalTotal.PrepaidAmount;*/


                List<object> invoItems = new List<object>();

                foreach (var invoiceItem in invoiceData.InvoiceItems)
                {
                    InvoiceLine invoiceLine = InvoiceHelper.CreateInvoiceLine(
                        invoiceItem.Name, invoiceItem.Quantity, invoiceItem.BaseQuantity,
                        invoiceItem.Price, inv.allowanceCharges, invoiceItem.VatCategory,
                        invoiceItem.VatPercentage, invoiceItem.IsIncludingVat, invoiceItem.TaxExemptionReasonCode,
                        invoiceItem.TaxExemptionReason);

                    inv.InvoiceLines.Add(invoiceLine);
                }


                inv.cSIDInfo.CertPem = companyCredentials.Certificate;
                inv.cSIDInfo.PrivateKey = companyCredentials.PrivateKey;

                SignedInvoice signedInvoice = null;
                res = ubl.GenerateInvoiceXML(inv, Directory.GetCurrentDirectory());
                if (res.IsValid)
                {
                    //return Ok(res.InvoiceHash);
                    //return Ok(res.SingedXML);
                    //return Ok(res.EncodedInvoice);
                    //return Ok(res.UUID);
                    //return Ok(res.QRCode);
                    //return Ok(res.PIH);
                    //return Ok(res.SingedXMLFileName);
                }
                else
                {
                    //return BadRequest(res);
                }

                signedInvoice = new SignedInvoice
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


                InvoiceReportingRequest invrequestbody = new InvoiceReportingRequest();

                invrequestbody.invoice = res.EncodedInvoice;
                invrequestbody.invoiceHash = res.InvoiceHash;
                invrequestbody.uuid = res.UUID;
                InvoiceReportingResponse invoicereportingmodel =
                    apireqlogic.CallComplianceInvoiceAPI(companyCredentials.SecretToken, companyCredentials.Secret,
                        invrequestbody);
                //for production

                //InvoiceClearanceResponse invoicereportingmodel = apireqlogic.CallClearanceAPI(Utility.ToBase64Encode(inv.cSIDInfo.CertPem), "cuqeJ5yQPoGInAF4MrynTQYOIwAYXN1jhpjFgRkga04=", invrequestbody);

                if (string.IsNullOrEmpty(invoicereportingmodel.ErrorMessage))
                {
                    await _signedInvoiceRepository.Create(signedInvoice);

                    responses.Add(new
                    {
                        ZATCA = invoicereportingmodel,
                        Res = new
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
                            res.EncodedInvoice,

                        }
                    });
                }
                else
                {
                    responses.Add(invoicereportingmodel);
                }
            }

            return Ok(responses);
        }


        [HttpPost("single-standard-dynamic")]
        public async Task<IActionResult> GenerateSingleDynamicStandard(BulkInvoiceRequest bulkInvoiceRequest)
        {
            var companyInfo = await _companyInfoRepository.GetByCompanyId(bulkInvoiceRequest.companyId);
            var company = await _companyRepository.GetById(bulkInvoiceRequest.companyId);
            var companyCredentials =
                await _companyCredentialsRepository.GetLatestByCompanyId(bulkInvoiceRequest.companyId);

            if (company == null)
            {
                return BadRequest("Company Not Found");
            }

            if (companyCredentials == null)
            {
                return BadRequest("companyCredentials Not Found");
            }


            UBLXML ubl = new UBLXML();
            Invoice inv = new Invoice();
            Result res = new Result();

            InvoiceData invoiceData =
                bulkInvoiceRequest.Invoices?.FirstOrDefault() ?? throw new InvalidOperationException();
            inv.ID = invoiceData.Id;
            inv.IssueDate = invoiceData.IssueDate;
            inv.IssueTime = invoiceData.IssueTime;

            inv.invoiceTypeCode.id = bulkInvoiceRequest.InvoicesType!.Id;

            inv.invoiceTypeCode.Name = bulkInvoiceRequest.InvoicesType!.Name;
            inv.DocumentCurrencyCode = bulkInvoiceRequest.InvoicesType!.DocumentCurrencyCode;

            inv.TaxCurrencyCode =
                bulkInvoiceRequest.InvoicesType!
                    .TaxCurrencyCode; ////فى حالة الدولار لابد ان تكون عملة الضريبة بالريال السعودى
            //inv.CurrencyRate = decimal.Parse("3.75"); // قيمة الدولار مقابل الريال
            // فى حالة ان اشعار دائن او مدين فقط هانكتب رقم الفاتورة اللى اصدرنا الاشعار ليها
            inv.billingReference.InvoiceDocumentReferenceID = invoiceData.InvoiceDocumentReferenceID;
            // هنا ممكن اضيف ال pih من قاعدة البيانات  
            //TODO Change this
            inv.AdditionalDocumentReferencePIH.EmbeddedDocumentBinaryObject =
                "NWZlY2ViNjZmZmM4NmYzOGQ5NTI3ODZjNmQ2OTZjNzljMmRiYzIzOWRkNGU5MWI0NjcyOWQ3M2EyN2ZiNTdlOQ==";
            // قيمة عداد الفاتورة
            inv.AdditionalDocumentReferenceICV.UUID = invoiceData.AddtionalId;
            //بيانات الدفع 
            // اكواد معين
            // اختيارى كود الدفع
            //inv.paymentmeans.PaymentMeansCode = "48";//اختيارى
            //inv.paymentmeans.InstructionNote = "Purchase"; //اجبارى فى الاشعارات
            // inv.paymentmeans.payeefinancialaccount.ID = "";//اختيارى
            // inv.paymentmeans.payeefinancialaccount.paymentnote = "Payment by credit";//اختيارى
            //بيانات البائع
            PaymentMeans paymentMeans = new PaymentMeans();
            paymentMeans.PaymentMeansCode = invoiceData.PaymentDetails.Type;
            paymentMeans.InstructionNote = invoiceData.PaymentDetails.InstructionNote;
            inv.paymentmeans.Add(paymentMeans);

            inv.delivery.ActualDeliveryDate = invoiceData.ActualDeliveryDate;
            inv.delivery.LatestDeliveryDate = invoiceData.LatestDeliveryDate;

            AccountingSupplierParty supplierParty = InvoiceHelper.CreateSupplierParty(
                companyInfo.PartyId.ToString(), companyInfo.SchemeID, companyInfo.StreetName,
                companyInfo.AdditionalStreetName, companyInfo.BuildingNumber,
                companyInfo.PlotIdentification, companyInfo.CityName, companyInfo.PostalZone,
                companyInfo.CountrySubentity,
                companyInfo.CitySubdivisionName, companyInfo.IdentificationCode, companyInfo.RegistrationName,
                companyInfo.taxRegistrationNumber);

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

            allowancecharge.taxCategory.ID = invoiceData.AllowanceCharge.TaxCategoryId;
            allowancecharge.taxCategory.Percent = invoiceData.AllowanceCharge.TaxCategoryPercent;

            allowancecharge.Amount = invoiceData.AllowanceCharge.TaxCategoryId.Equals("S")
                ? 0
                : invoiceData.AllowanceCharge.Amount;

            allowancecharge.AllowanceChargeReason = invoiceData.AllowanceCharge.Reason;
            inv.allowanceCharges.Add(allowancecharge);

            /*inv.legalMonetaryTotal.PrepaidAmount = invoiceData.LegalTotal.PrepaidAmount;*/


            InvoiceItem invoiceItem =
                invoiceData.InvoiceItems?.FirstOrDefault() ?? throw new InvalidOperationException();

            InvoiceLine invoiceLine = InvoiceHelper.CreateInvoiceLine(
                invoiceItem.Name, invoiceItem.Quantity, invoiceItem.BaseQuantity,
                invoiceItem.Price, inv.allowanceCharges, invoiceItem.VatCategory,
                invoiceItem.VatPercentage, invoiceItem.IsIncludingVat, invoiceItem.TaxExemptionReasonCode,
                invoiceItem.TaxExemptionReason);

            inv.InvoiceLines.Add(invoiceLine);

            inv.cSIDInfo.CertPem = companyCredentials.Certificate;
            inv.cSIDInfo.PrivateKey = companyCredentials.PrivateKey;

            SignedInvoice signedInvoice = null;
            res = ubl.GenerateInvoiceXML(inv, Directory.GetCurrentDirectory());
            if (res.IsValid)
            {
                //return Ok(res.InvoiceHash);
                //return Ok(res.SingedXML);
                //return Ok(res.EncodedInvoice);
                //return Ok(res.UUID);
                //return Ok(res.QRCode);
                //return Ok(res.PIH);
                //return Ok(res.SingedXMLFileName);
            }
            else
            {
                //return BadRequest(res);
            }

            signedInvoice = new SignedInvoice
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

            ApiRequestLogic apireqlogic = new ApiRequestLogic(Mode.developer);

            InvoiceReportingRequest invrequestbody = new InvoiceReportingRequest();

            invrequestbody.invoice = res.EncodedInvoice;
            invrequestbody.invoiceHash = res.InvoiceHash;
            invrequestbody.uuid = res.UUID;
            InvoiceReportingResponse invoicereportingmodel =
                apireqlogic.CallComplianceInvoiceAPI(companyCredentials.SecretToken, companyCredentials.Secret,
                    invrequestbody);
            //for production

            //InvoiceClearanceResponse invoicereportingmodel = apireqlogic.CallClearanceAPI(Utility.ToBase64Encode(inv.cSIDInfo.CertPem), "cuqeJ5yQPoGInAF4MrynTQYOIwAYXN1jhpjFgRkga04=", invrequestbody);

            if (string.IsNullOrEmpty(invoicereportingmodel.ErrorMessage))
            {
                await _signedInvoiceRepository.Create(signedInvoice);

                return Ok(new
                {
                    ZATCA = invoicereportingmodel,
                    Res = res
                });
            }
            else
            {
                return Ok(invoicereportingmodel);
            }
        }
    }
}