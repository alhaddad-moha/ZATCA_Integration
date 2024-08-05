using System.ComponentModel.DataAnnotations;
using ZATCA_V2.CustomValidators;
using ZATCA_V2.DTOs;
using ZatcaIntegrationSDK;

namespace ZATCA_V2.Requests;

public class BulkInvoiceRequest
{
    [Required] public int companyId { get; set; }
    [Required] public InvoiceType InvoicesType { get; set; }
    [Required] public ICollection<InvoiceData> Invoices { get; set; }
}

public class InvoiceData
{
    [Required] public string Id { get; set; }
    public string? InvoiceDocumentReferenceID { get; set; }

    [Required]
    [ValidDateFormat("yyyy-MM-dd", ErrorMessage = "IssueDate must be a valid date in the format yyyy-MM-dd.")]
    public string IssueDate { get; set; }

    [Required]
    [ValidDateFormat("HH:mm:ss", ErrorMessage = "IssueTime must be a valid time in the format HH:mm:ss.")]
    public string IssueTime { get; set; }

    [Required]
    [ValidDateFormat("yyyy-MM-dd", ErrorMessage = "ActualDeliveryDate must be a valid date in the format yyyy-MM-dd.")]
    public string ActualDeliveryDate { get; set; }

    [Required]
    [ValidDateFormat("yyyy-MM-dd", ErrorMessage = "LatestDeliveryDate must be a valid date in the format yyyy-MM-dd.")]
    public string LatestDeliveryDate { get; set; }

    [Required] public PaymentDetails PaymentDetails { get; set; }
    public CustomerInformation? CustomerInformation { get; set; }

    public AllowanceCharge? AllowanceCharge { get; set; }

    /*
    [Required] public LegalTotal LegalTotal { get; set; }
    */

    [Required] public ICollection<InvoiceItem> InvoiceItems { get; set; }
}

public class InvoiceItem
{
    [Required] public string Name { get; set; }

    [Range(0.01, double.MaxValue, ErrorMessage = "Quantity must be greater than zero.")]
    public decimal Quantity { get; set; }

    [Range(0.01, double.MaxValue, ErrorMessage = "Base Quantity must be greater than zero.")]
    public decimal BaseQuantity { get; set; }

    [Range(0.01, double.MaxValue, ErrorMessage = "Price must be greater than zero.")]
    public decimal Price { get; set; }

    [Required]
    [RegularExpression("O|S|E|Z", ErrorMessage = "TaxCategory must be 'S', 'O', 'E', or 'Z'.")]
    public string TaxCategory { get; set; }

    [Range(0, 100, ErrorMessage = "Vat Percentage must be between 0 and 100.")]
    public decimal VatPercentage { get; set; }

    public bool IsIncludingVat { get; set; }
    public string? TaxExemptionReasonCode { get; set; }
    public string? TaxExemptionReason { get; set; }
}

public class InvoiceType
{
    [RegularExpression("388|383|381",
        ErrorMessage = "Name must be '388' (invoice) or '383' (debit note) or '381' (credit note).")]
    public int Id { get; set; } = 388;

    [Required]
    [RegularExpression("0100000|0200000",
        ErrorMessage = "Name must be '0100000' (standard invoice) or '0200000' (simplified invoice).")]
    public string Name { get; set; } = "0100000";

    [StringLength(3, ErrorMessage = "DocumentCurrencyCode must be 3 characters long.")]
    public string DocumentCurrencyCode { get; set; } = "SAR";

    [StringLength(3, ErrorMessage = "TaxCurrencyCode must be 3 characters long.")]
    public string TaxCurrencyCode { get; set; } = "SAR";
}

public class PaymentDetails
{
    [RegularExpression("10|30|42|48",
        ErrorMessage =
            "Type must be '10' (on cash) or '30' (on credit) or '42' (bank account payment) or '48' (bank card payment).")]
    public string Type { get; set; } = "10";

    public string? InstructionNote { get; set; }
}

public class CustomerInformation
{
    [Required(ErrorMessage = "CommercialRegistrationNumber is required.")]
    public string CommercialRegistrationNumber { get; set; }

    public string CommercialNumberType { get; set; } = "CRN";

    public AddressDto Address { get; set; }

    [Required] public string RegistrationName { get; set; }

    [RegularExpression(@"^\d{15}$", ErrorMessage = "TaxRegistrationNumber must be a 15-digit number.")]
    [Required(ErrorMessage = "TaxRegistrationNumber is required.")]

    public string TaxRegistrationNumber { get; set; }
}

public class AllowanceCharge
{
    public decimal Amount { get; set; }

    public string Reason { get; set; }

    [RegularExpression("O|S|E|Z", ErrorMessage = "TaxCategoryId must be 'S', 'O', 'E', or 'Z'.")]
    public string TaxCategory { get; set; }

    [Range(0, 100, ErrorMessage = "Tax Category Percent must be between 0 and 100.")]
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
    public string CityName { get; set; }
    public string PostalZone { get; set; }
    public string CountrySubentity { get; set; }
    public string CitySubdivisionName { get; set; }
    public string IdentificationCode { get; set; }
}