using Microsoft.EntityFrameworkCore;

namespace ZATCA_V2.Models
{
    [Index(nameof(OrganizationIdentifier), IsUnique = true)]

    public class Company
    {
        public int Id { get; set; }
        public string CommonName { get; set; }
        public string OrganizationIdentifier { get; set; }
        public string OrganizationUnitName { get; set; }
        public string OrganizationName { get; set; }
        public string CountryName { get; set; } = "SA";
        public string LocationAddress { get; set; }
        public string IndustryBusinessCategory { get; set; }

        public CompanyCredentials? CompanyCredentials { get; set; } // Make it nullable with the '?' symbol
    }
}