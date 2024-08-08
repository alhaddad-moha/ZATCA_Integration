using System.ComponentModel.DataAnnotations;
using ZatcaIntegrationSDK;
using ZatcaIntegrationSDK.APIHelper;

namespace ZATCA_V3.DTOs
{
    public class AddressDto
    {
        [Required(ErrorMessage = "StreetName is required.")]
        public string StreetName { get; set; }

        public string AdditionalStreetName { get; set; }

        [Required(ErrorMessage = "BuildingNumber is required.")]
        [RegularExpression(@"^\d{4}$", ErrorMessage = "BuildingNumber must be a 4-digit number.")]
        public string BuildingNumber { get; set; }


        [Required(ErrorMessage = "CityName is required.")]
        public string CityName { get; set; }

        [Required(ErrorMessage = "PostalZone is required.")]
        [RegularExpression(@"^\d{5}$", ErrorMessage = "PostalZone must be a 5-digit number.")]
        public string PostalZone { get; set; }
        
        public string CountrySubentity { get; set; }

        [Required(ErrorMessage = "CitySubdivisionName is required.")]
        public string CitySubdivisionName { get; set; }

        public string IdentificationCode { get; set; } = "SA";
    }
}