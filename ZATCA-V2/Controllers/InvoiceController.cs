using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using ZATCA_V2.Helpers;
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
            inv.AdditionalDocumentReferencePIH.EmbeddedDocumentBinaryObject = "NWZlY2ViNjZmZmM4NmYzOGQ5NTI3ODZjNmQ2OTZjNzljMmRiYzIzOWRkNGU5MWI0NjcyOWQ3M2EyN2ZiNTdlOQ==";
            // قيمة عداد الفاتورة
            inv.AdditionalDocumentReferenceICV.UUID = 123456; // لابد ان يكون ارقام فقط

            inv.delivery.ActualDeliveryDate = "2022-10-22";
            inv.delivery.LatestDeliveryDate = "2022-10-23";
            PaymentMeans paymentMeans = new PaymentMeans();
            paymentMeans.PaymentMeansCode = "10";//اختيارى
            PaymentMeans paymentMeans1 = new PaymentMeans();
            paymentMeans1.PaymentMeansCode = "42";//اختيارى
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
            inv.CustomerParty= customerParty;

            inv.legalMonetaryTotal.PayableAmount = 0;
            inv.legalMonetaryTotal.PrepaidAmount = 46;

            AllowanceChargeCollection allowanceCharges = new AllowanceChargeCollection();
            AllowanceCharge allowance = new AllowanceCharge();
            allowance.ChargeIndicator = false;
            //فى حالة انى هاضيف الخصم على مستوى الفاتورة بالنسبة
            allowance.MultiplierFactorNumeric = 0;
            allowance.BaseAmount = 0;

            allowance.Amount = 0;//قيمة الخصم ويكون صفر فى حالة لو هانستخدم النسبة
            allowance.AllowanceChargeReasonCode = ""; //سبب الخصم
            allowance.AllowanceChargeReason = "discount"; //سبب الخصم
            allowance.taxCategory.ID = "S";// كود الضريبة
            allowance.taxCategory.Percent = 15;// نسبة الضريبة
            //فى حالة عندى اكثر من خصم بعمل loop على الاسطر السابقة
            allowanceCharges.Add(allowance);

            InvoiceLine invline = InvoiceHelper.GetInvoiceLine("Item 1", 20, 20, 80, allowanceCharges, "S", 15, true, "", "");
            InvoiceLine invline1 = InvoiceHelper.GetAdditionalInvoiceLine("Item 1", allowanceCharges, "S", 15, "", "");

            inv.InvoiceLines.Add(invline);
            inv.InvoiceLines.Add(invline1);

            string certificateFilePath = "Utils/keys/cert.txt";
            string certificate= System.IO.File.ReadAllText(certificateFilePath);

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

            string csr= Helper.ReadFileToString(Constants.CSR_Path);

            tokenresponse = apireqlogic.GetComplianceCSIDAPI("123456", csr);
            if (string.IsNullOrEmpty(tokenresponse.ErrorMessage))
            {
                invrequestbody.invoice = res.EncodedInvoice;
                invrequestbody.invoiceHash = res.InvoiceHash;
                invrequestbody.uuid = res.UUID;
                InvoiceReportingResponse invoicereportingmodel = apireqlogic.CallComplianceInvoiceAPI(tokenresponse.BinarySecurityToken, tokenresponse.Secret, invrequestbody);
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
    }
}
