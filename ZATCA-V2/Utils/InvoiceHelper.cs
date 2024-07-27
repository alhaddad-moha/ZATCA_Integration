using ZATCA_V2.Models;
using ZatcaIntegrationSDK;

namespace ZATCA_V2.Utils
{
    public class InvoiceHelper
    {
        public static AccountingSupplierParty CreateSupplierParty(CompanyInfo companyInfo)
        {
            // Create an instance of the SupplierParty class
            AccountingSupplierParty supplierParty = new AccountingSupplierParty
            {
                partyIdentification = new PartyIdentification
                {
                    ID = companyInfo.PartyId,
                    schemeID = companyInfo.SchemeID
                },
                postalAddress = new PostalAddress
                {
                    StreetName = companyInfo.StreetName,
                    AdditionalStreetName = companyInfo.AdditionalStreetName,
                    BuildingNumber = companyInfo.BuildingNumber,
                    PlotIdentification = companyInfo.PlotIdentification,
                    CityName = companyInfo.CityName,
                    PostalZone = companyInfo.PostalZone,
                    CountrySubentity = companyInfo.CountrySubentity,
                    CitySubdivisionName = companyInfo.CitySubdivisionName,
                    country = new Country
                    {
                        IdentificationCode = companyInfo.IdentificationCode
                    }
                },
                partyLegalEntity = new PartyLegalEntity
                {
                    RegistrationName = companyInfo.RegistrationName
                },
                partyTaxScheme = new PartyTaxScheme
                {
                    CompanyID = companyInfo.taxRegistrationNumber
                }
            };

            return supplierParty;
        }

        public static AccountingCustomerParty CreateCustomerParty(
            string partyId, string schemeId, string streetName, string? additionalStreetName,
            string buildingNumber, string? plotIdentification, string cityName, string postalZone,
            string? countrySubentity, string? citySubdivisionName, string countryIdentificationCode,
            string registrationName, string companyID)
        {
            AccountingCustomerParty customerParty = new AccountingCustomerParty
            {
                partyIdentification = new PartyIdentification
                {
                    ID = partyId,
                    schemeID = schemeId
                },
                postalAddress = new PostalAddress
                {
                    StreetName = streetName,
                    AdditionalStreetName = additionalStreetName,
                    BuildingNumber = buildingNumber,
                    PlotIdentification = plotIdentification,
                    CityName = cityName,
                    PostalZone = postalZone,
                    CountrySubentity = countrySubentity,
                    CitySubdivisionName = citySubdivisionName,
                    country = new Country
                    {
                        IdentificationCode = countryIdentificationCode
                    }
                },
                partyLegalEntity = new PartyLegalEntity
                {
                    RegistrationName = registrationName
                },
                partyTaxScheme = new PartyTaxScheme
                {
                    CompanyID = companyID
                }
            };

            return customerParty;
        }

        public static InvoiceLine GetInvoiceLine(string itemName, decimal invoiceQuantity, decimal baseQuantity,
            decimal itemPrice, AllowanceChargeCollection allowanceCharges, string vatCategory,
            decimal vatPercentage, bool includingVat = false, string taxExemptionReasonCode = "",
            string taxExemptionReason = "")
        {
            InvoiceLine invline = new InvoiceLine();
            invline.item.Name = itemName;
            invline.InvoiceQuantity = invoiceQuantity; // 
            invline.price.BaseQuantity = baseQuantity;
            invline.price.PriceAmount = itemPrice; // سعر المنتج  

            invline.item.classifiedTaxCategory.ID = vatCategory; // كود الضريبة
            invline.item.classifiedTaxCategory.Percent = vatPercentage; // نسبة الضريبة
            invline.allowanceCharges = allowanceCharges;
            invline.price.EncludingVat = includingVat;

            invline.taxTotal.TaxSubtotal.taxCategory.ID = vatCategory; //كود الضريبة
            invline.taxTotal.TaxSubtotal.taxCategory.Percent = vatPercentage; //نسبة الضريبة
            if (vatCategory != "S")
            {
                invline.taxTotal.TaxSubtotal.taxCategory.TaxExemptionReason = taxExemptionReason;
                invline.taxTotal.TaxSubtotal.taxCategory.TaxExemptionReasonCode = taxExemptionReasonCode;
            }

            return invline;
        }
        public static InvoiceLine CreateInvoiceLine(string itemName, decimal invoiceQuantity, decimal baseQuantity,
            decimal itemPrice, AllowanceChargeCollection allowanceCharges, string vatCategory,
            decimal vatPercentage, bool includingVat = false, string? taxExemptionReasonCode = "",
            string? taxExemptionReason = "")
        {
            InvoiceLine invline = new InvoiceLine();
            invline.item.Name = itemName;
            invline.InvoiceQuantity = invoiceQuantity; // 
            invline.price.BaseQuantity = baseQuantity;
            invline.price.PriceAmount = itemPrice; // سعر المنتج  

            invline.item.classifiedTaxCategory.ID = vatCategory; // كود الضريبة
            invline.item.classifiedTaxCategory.Percent = vatPercentage; // نسبة الضريبة
            invline.allowanceCharges = allowanceCharges;
            invline.price.EncludingVat = includingVat;

            invline.taxTotal.TaxSubtotal.taxCategory.ID = vatCategory; //كود الضريبة
            invline.taxTotal.TaxSubtotal.taxCategory.Percent = vatPercentage; //نسبة الضريبة
            if (vatCategory != "S")
            {
                invline.taxTotal.TaxSubtotal.taxCategory.TaxExemptionReason = taxExemptionReason;
                invline.taxTotal.TaxSubtotal.taxCategory.TaxExemptionReasonCode = taxExemptionReasonCode;
            }

            return invline;
        }

        public static InvoiceLine GetAdditionalInvoiceLine(string itemname, AllowanceChargeCollection allowanceCharges,
            string vatcategory, decimal vatpercentage, string TaxExemptionReasonCode = "",
            string TaxExemptionReason = "")
        {
            InvoiceLine invline = new InvoiceLine();
            invline.item.Name = "Prepayment adjustment";
            invline.InvoiceQuantity = 1; // 
            invline.price.PriceAmount = 0; // سعر المنتج  

            invline.item.classifiedTaxCategory.ID = vatcategory; // كود الضريبة
            invline.item.classifiedTaxCategory.Percent = vatpercentage; // نسبة الضريبة
            invline.allowanceCharges = allowanceCharges;

            invline.taxTotal.TaxSubtotal.taxCategory.ID = vatcategory; //كود الضريبة
            invline.taxTotal.TaxSubtotal.taxCategory.Percent = vatpercentage; //نسبة الضريبة
            if (vatcategory != "S")
            {
                invline.taxTotal.TaxSubtotal.taxCategory.TaxExemptionReason = TaxExemptionReason;
                invline.taxTotal.TaxSubtotal.taxCategory.TaxExemptionReasonCode = TaxExemptionReasonCode;
            }

            invline.taxTotal.TaxSubtotal.TaxableAmount = 40m;
            invline.taxTotal.TaxSubtotal.TaxAmount = 6m;
            DocumentReference documentReference = new DocumentReference();
            documentReference.ID = "1253m";
            documentReference.IssueDate = "2023-05-10";
            documentReference.IssueTime = "11:25:55";
            documentReference.DocumentTypeCode = 386;

            DocumentReference documentReference1 = new DocumentReference();
            documentReference1.ID = "1252523m";
            documentReference1.IssueDate = "2023-05-11";
            documentReference1.IssueTime = "11:25:55";
            documentReference1.DocumentTypeCode = 386;
            invline.documentReferences.Add(documentReference);
            invline.documentReferences.Add(documentReference1);
            return invline;
        }
    }
}