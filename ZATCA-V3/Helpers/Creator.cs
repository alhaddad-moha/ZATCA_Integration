using ZATCA_V3.DTOs;
using ZATCA_V3.Models;

namespace ZATCA_V3.Helpers;

public class Creator
{
    public static void WriteConfigurationFile(string filePath, CertificateConfiguration certificateConfig)
    {
        string? directory = Path.GetDirectoryName(filePath);
        if (!Directory.Exists(directory))
        {
            if (directory != null) Directory.CreateDirectory(directory);
        }

        using (StreamWriter writer = File.CreateText(filePath))
        {
            writer.WriteLine("oid_section= OIDS");
            writer.WriteLine("[ OIDS ]");
            writer.WriteLine("certificateTemplateName= 1.3.6.1.4.1.311.20.2");

            writer.WriteLine("[req]");
            writer.WriteLine("default_bits=2048");
            writer.WriteLine($"emailAddress=={certificateConfig.EmailAddress}");
            writer.WriteLine("req_extensions=v3_req");
            writer.WriteLine("x509_extensions=v3_Ca");
            writer.WriteLine("prompt=no");
            writer.WriteLine("default_md=sha256");
            writer.WriteLine("req_extensions=req_ext");
            writer.WriteLine("distinguished_name=req_distinguished_name");
            writer.WriteLine("");


            writer.WriteLine("[req_distinguished_name]");
            writer.WriteLine($"C={certificateConfig.C}");
            writer.WriteLine($"OU={certificateConfig.OU}");
            writer.WriteLine($"O={certificateConfig.O}");
            writer.WriteLine($"CN={certificateConfig.CN}");
            writer.WriteLine("");

            writer.WriteLine("[v3_req]");
            writer.WriteLine("basicConstraints = CA:FALSE");
            writer.WriteLine("keyUsage = nonRepudiation, digitalSignature, keyEncipherment");
            writer.WriteLine("");
            writer.WriteLine("");

            writer.WriteLine("[req_ext]");
            writer.WriteLine(
                $"certificateTemplateName = ASN1:PRINTABLESTRING:{certificateConfig.CertificateTemplateName}");
            writer.WriteLine($"subjectAltName = dirName:alt_names");
            writer.WriteLine("");


            writer.WriteLine("[alt_names]");
            writer.WriteLine($"SN={certificateConfig.SN}");
            writer.WriteLine($"UID={certificateConfig.UID}");
            writer.WriteLine($"title={certificateConfig.Title}");
            writer.WriteLine($"registeredAddress={certificateConfig.RegisteredAddress}");
            writer.WriteLine($"businessCategory={certificateConfig.BusinessCategory}");
        }

        Console.WriteLine($"Generated configuration file: {filePath}");
    }

    public static CompanyDto MapToCompanyDto(Company company)
    {
        return new CompanyDto
        {
            Id = company.Id,
            SchemeId = company.SchemeId,
            CommercialRegistrationNumber = company.CommercialRegistrationNumber,
            CommonName = company.CommonName,
            TaxRegistrationNumber = company.TaxRegistrationNumber,
            OrganizationUnitName = company.OrganizationUnitName,
            OrganizationName = company.OrganizationName,
            BusinessIndustry = company.BusinessIndustry,
            InvoiceType = company.InvoiceType,
            CountryName = company.CountryName,
            IdentificationCode = company.IdentificationCode,
            StreetName = company.StreetName,
            AdditionalStreetName = company.AdditionalStreetName,
            BuildingNumber = company.BuildingNumber,
            CityName = company.CityName,
            PostalZone = company.PostalZone,
            CountrySubentity = company.CountrySubentity,
            CitySubdivisionName = company.CitySubdivisionName,
            EmailAddress = company.EmailAddress,
            DeviceSerialNumber = company.DeviceSerialNumber
        };
    }

    public static Company GenerateCompanyData(CompanyReleaseRequestDto companyReleaseRequest)
    {
        return new Company
        {
            CommonName = companyReleaseRequest.CommonName,
            CommercialRegistrationNumber = companyReleaseRequest.CommercialRegistrationNumber,
            TaxRegistrationNumber = companyReleaseRequest.TaxRegistrationNumber,
            OrganizationUnitName = companyReleaseRequest.OrganizationUnitName,
            OrganizationName = companyReleaseRequest.OrganizationName,
            InvoiceType = companyReleaseRequest.InvoiceType,
            EmailAddress = companyReleaseRequest.EmailAddress,
            BusinessIndustry = companyReleaseRequest.BusinessIndustry,
            CountryName = companyReleaseRequest.CountryName,
            StreetName = companyReleaseRequest.Address.StreetName,
            AdditionalStreetName = companyReleaseRequest.Address.AdditionalStreetName,
            CountrySubentity = companyReleaseRequest.Address.CountrySubentity,
            CityName = companyReleaseRequest.Address.CityName,
            CitySubdivisionName = companyReleaseRequest.Address.CitySubdivisionName,
            BuildingNumber = companyReleaseRequest.Address.BuildingNumber,
            PostalZone = companyReleaseRequest.Address.PostalZone,
            IdentificationCode = companyReleaseRequest.IdentificationCode,
            DeviceSerialNumber = companyReleaseRequest.DeviceSerialNumber,
            CompanyCredentials = new List<CompanyCredentials>() // Initialize the CompanyCredentials list
        };
    }

    public static Company UpdateCompanyData(Company existingCompany, CompanyUpdateRequestDto companyUpdateRequest)
    {
        existingCompany.CommonName = companyUpdateRequest.CommonName;
        existingCompany.CommercialRegistrationNumber = companyUpdateRequest.CommercialRegistrationNumber;
        existingCompany.TaxRegistrationNumber = companyUpdateRequest.TaxRegistrationNumber;
        existingCompany.OrganizationUnitName = companyUpdateRequest.OrganizationUnitName;
        existingCompany.OrganizationName = companyUpdateRequest.OrganizationName;
        existingCompany.InvoiceType = companyUpdateRequest.InvoiceType;
        existingCompany.EmailAddress = companyUpdateRequest.EmailAddress;
        existingCompany.BusinessIndustry = companyUpdateRequest.BusinessIndustry;
        existingCompany.CountryName = companyUpdateRequest.CountryName;
        existingCompany.StreetName = companyUpdateRequest.Address.StreetName;
        existingCompany.AdditionalStreetName = companyUpdateRequest.Address.AdditionalStreetName;
        existingCompany.CountrySubentity = companyUpdateRequest.Address.CountrySubentity;
        existingCompany.CityName = companyUpdateRequest.Address.CityName;
        existingCompany.CitySubdivisionName = companyUpdateRequest.Address.CitySubdivisionName;
        existingCompany.BuildingNumber = companyUpdateRequest.Address.BuildingNumber;
        existingCompany.PostalZone = companyUpdateRequest.Address.PostalZone;
        existingCompany.IdentificationCode = companyUpdateRequest.IdentificationCode;
        existingCompany.DeviceSerialNumber = companyUpdateRequest.DeviceSerialNumber;

        return existingCompany;
    }
}