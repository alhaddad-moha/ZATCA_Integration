using Microsoft.EntityFrameworkCore;

namespace ZATCA_V2.Models
{
    [Index(nameof(TaxRegistrationNumber), IsUnique = true)]
    public class Company
    {
        public int Id { get; set; }
        public string SchemeId { get; set; } = "CRN";

        public string CommercialRegistrationNumber { get; set; }
        public string CommonName { get; set; }
        public string TaxRegistrationNumber { get; set; }
        public string OrganizationUnitName { get; set; }
        public string OrganizationName { get; set; }

        public string BusinessIndustry { get; set; }
        public string InvoiceType { get; set; }


        public string CountryName { get; set; } = "SA";
        public string IdentificationCode { get; set; } = "SA";
        public string StreetName { get; set; }
        public string? AdditionalStreetName { get; set; }
        public string BuildingNumber { get; set; }
        public string? PlotIdentification { get; set; }
        public string CityName { get; set; }
        public string PostalZone { get; set; }
        public string? CountrySubentity { get; set; }
        public string? CitySubdivisionName { get; set; }
        public string? EmailAddress { get; set; }
        public string? DeviceSerialNumber { get; set; }


        public ICollection<CompanyCredentials>? CompanyCredentials { get; set; }
        public CompanyInfo? CompanyInfo { get; set; }
        public ICollection<SignedInvoice> SignedInvoices { get; set; } = new List<SignedInvoice>();
    }
}