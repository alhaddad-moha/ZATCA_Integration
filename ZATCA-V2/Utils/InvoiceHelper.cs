﻿using ZatcaIntegrationSDK;

namespace ZATCA_V2.Utils
{
    public class InvoiceHelper
    {
        public static AccountingSupplierParty CreateSupplierParty(
        string partyId, string schemeId, string streetName, string additionalStreetName,
        string buildingNumber, string plotIdentification, string cityName, string postalZone,
        string countrySubentity, string citySubdivisionName, string countryIdentificationCode,
        string registrationName, string companyID)
        {
            // Create an instance of the SupplierParty class
            AccountingSupplierParty supplierParty = new AccountingSupplierParty
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

            return supplierParty;
        }

        public static AccountingCustomerParty CreateCustomerParty(
              string partyId, string schemeId, string streetName, string additionalStreetName,
              string buildingNumber, string plotIdentification, string cityName, string postalZone,
              string countrySubentity, string citySubdivisionName, string countryIdentificationCode,
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

        public static InvoiceLine GetInvoiceLine(string itemname, decimal invoicedquantity, decimal basequantity, decimal itemprice, AllowanceChargeCollection allowanceCharges, string vatcategory, decimal vatpercentage, bool includingvat = false, string TaxExemptionReasonCode = "", string TaxExemptionReason = "")
        {
            InvoiceLine invline = new InvoiceLine();
            invline.item.Name = itemname;
            invline.InvoiceQuantity = invoicedquantity; // 
            invline.price.BaseQuantity = basequantity;
            invline.price.PriceAmount = itemprice;// سعر المنتج  

            invline.item.classifiedTaxCategory.ID = vatcategory;// كود الضريبة
            invline.item.classifiedTaxCategory.Percent = vatpercentage;// نسبة الضريبة
            invline.allowanceCharges = allowanceCharges;
            //if the price is including vat set EncludingVat=true;
            invline.price.EncludingVat = includingvat;

            invline.taxTotal.TaxSubtotal.taxCategory.ID = vatcategory;//كود الضريبة
            invline.taxTotal.TaxSubtotal.taxCategory.Percent = vatpercentage;//نسبة الضريبة
            if (vatcategory != "S")
            {
                invline.taxTotal.TaxSubtotal.taxCategory.TaxExemptionReason = TaxExemptionReason;
                invline.taxTotal.TaxSubtotal.taxCategory.TaxExemptionReasonCode = TaxExemptionReasonCode;
            }

            return invline;
        }

        public static InvoiceLine GetAdditionalInvoiceLine(string itemname, AllowanceChargeCollection allowanceCharges, string vatcategory, decimal vatpercentage, string TaxExemptionReasonCode = "", string TaxExemptionReason = "")
        {
            InvoiceLine invline = new InvoiceLine();
            invline.item.Name = "Prepayment adjustment";
            invline.InvoiceQuantity = 1; // 
            invline.price.PriceAmount = 0;// سعر المنتج  

            invline.item.classifiedTaxCategory.ID = vatcategory;// كود الضريبة
            invline.item.classifiedTaxCategory.Percent = vatpercentage;// نسبة الضريبة
            invline.allowanceCharges = allowanceCharges;

            invline.taxTotal.TaxSubtotal.taxCategory.ID = vatcategory;//كود الضريبة
            invline.taxTotal.TaxSubtotal.taxCategory.Percent = vatpercentage;//نسبة الضريبة
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
