using ZatcaIntegrationSDK;

namespace ZATCA_V2.Requests;

public class BulkInvoiceRequest
{
    public int companyId { get; set; }
    public InvoiceType? InvoicesType { get; set; }
    public ICollection<InvoiceData>? Invoices { get; set; }
}

public class InvoiceData
{
    public string Id { get; set; }
    public int AddtionalId { get; set; }
    public string? InvoiceDocumentReferenceID { get; set; }
    public string IssueDate { get; set; }
    public string IssueTime { get; set; }
    public string ActualDeliveryDate { get; set; }
    public string LatestDeliveryDate { get; set; }
    public PaymentDetails PaymentDetails { get; set; }
    public CustomerInformation CustomerInformation { get; set; }
    public allowanceCharge AllowanceCharge { get; set; }
    public LegalTotal LegalTotal { get; set; }
    public ICollection<InvoiceItem>? InvoiceItems { get; set; }
}

public class InvoiceItem
{
    public string Name { get; set; }
    public decimal Quantity { get; set; }
    public decimal BaseQuantity { get; set; }
    public decimal Price { get; set; }
    public string VatCategory { get; set; }
    public decimal VatPercentage { get; set; }
    public bool IsIncludingVat { get; set; }
    public string? TaxExemptionReasonCode { get; set; }
    public string? TaxExemptionReason { get; set; }
}

public class InvoiceType
{
    public int Id { get; set; } = 388;
    public string Name { get; set; } = "0100000";
    public string DocumentCurrencyCode { get; set; } = "SAR";
    public string TaxCurrencyCode { get; set; } = "SAR";
}

public class PaymentDetails
{
    public string Type { get; set; } = "10";
    public string? InstructionNote { get; set; }
}

public class CustomerInformation
{
    public string? CommercialNumber { get; set; }
    public string? CommercialNumberType { get; set; } = "CRN";
    public Address? Address { get; set; }
    public string RegistrationName { get; set; }
    public string RegistrationNumber { get; set; }
}

public class allowanceCharge
{
    public decimal Amount { get; set; }
    public string Reason { get; set; }
    public string TaxCategoryId { get; set; }
    public decimal TaxCategoryPercent { get; set; }
}

public class LegalTotal
{
    public decimal PrepaidAmount { get; set; } = 0;
}

public class Address
{
    
    public string StreetName { get; set; }
    public string? AdditionalStreetName { get; set; }
    public string BuildingNumber { get; set; }
    public string PlotIdentification { get; set; }
    public string CityName { get; set; }
    public string PostalZone { get; set; }
    public string CountrySubentity { get; set; }
    public string CitySubdivisionName { get; set; }
    public string IdentificationCode { get; set; }   
}