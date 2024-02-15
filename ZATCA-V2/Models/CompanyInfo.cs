namespace ZATCA_V2.Models;

public class CompanyInfo
{
    public string ID { get; set; }
    public string SchemeID { get; set; }
    public string StreetName { get; set; }
    public string? AdditionalStreetName { get; set; }
    public string BuildingNumber { get; set; }
    public string? PlotIdentification { get; set; }
    public string CityName { get; set; }
    public string PostalZone { get; set; }
    public string? CountrySubentity { get; set; }
    public string CitySubdivisionName { get; set; }
    public string IdentificationCode { get; set; } = "SA";
    public string RegistrationName { get; set; }
    public string taxRegistrationNumber { get; set; }
    public Company Company { get; set; }
}