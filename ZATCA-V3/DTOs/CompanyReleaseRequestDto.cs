using System.ComponentModel.DataAnnotations;
using ZatcaIntegrationSDK.APIHelper;

namespace ZATCA_V3.DTOs
{
    public class CompanyReleaseRequestDto
    {
        [Required(ErrorMessage = "CommonName is required.")]
        public string CommonName { get; set; }

        [Required(ErrorMessage = "OrganizationName is required.")]
        public string OrganizationName { get; set; }

        [Required(ErrorMessage = "OrganizationUnitName is required.")]
        public string OrganizationUnitName { get; set; }

        public string CountryName { get; set; } = "SA";

        [Required(ErrorMessage = "SerialNumber is required.")]
        public string DeviceSerialNumber { get; set; }

        // OrganizationIdentifier
        [RegularExpression(@"^\d{15}$", ErrorMessage = "TaxRegistrationNumber must be a 15-digit number.")]
        [Required(ErrorMessage = "TaxRegistrationNumber is required.")]
        public string TaxRegistrationNumber { get; set; }

        [Required(ErrorMessage = "CommercialRegistrationNumber is required.")]
        public string CommercialRegistrationNumber { get; set; }

        [Required(ErrorMessage = "InvoiceType is required.")]
        [RegularExpression("1100|1000|0100", ErrorMessage = "InvoiceType must be 1100, 1000, or 0100.")]
        public string InvoiceType { get; set; }

        [Required(ErrorMessage = "BusinessCategory is required.")]
        public string BusinessIndustry { get; set; }

        [Required(ErrorMessage = "Location is required.")]
        public AddressDto Address { get; set; }

        [Required(ErrorMessage = "OTP is required.")]
        public string OTP { get; set; }

        public Mode Mode { get; set; } = Mode.developer;

        public string IdentificationCode { get; set; } = "SA";
        [Required(ErrorMessage = "EmailAddress is required.")]

        public string EmailAddress { get; set; }

       
    }
}