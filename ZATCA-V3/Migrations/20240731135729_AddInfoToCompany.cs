using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ZATCA_V3.Migrations
{
    public partial class AddInfoToCompany : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "OrganizationIdentifier",
                table: "Companies",
                newName: "TaxRegistrationNumber");

            migrationBuilder.RenameColumn(
                name: "LocationAddress",
                table: "Companies",
                newName: "StreetName");

            migrationBuilder.RenameColumn(
                name: "IndustryBusinessCategory",
                table: "Companies",
                newName: "SchemeId");

            migrationBuilder.RenameIndex(
                name: "IX_Companies_OrganizationIdentifier",
                table: "Companies",
                newName: "IX_Companies_TaxRegistrationNumber");

            migrationBuilder.AddColumn<string>(
                name: "AdditionalStreetName",
                table: "Companies",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "BuildingNumber",
                table: "Companies",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "BusinessIndustry",
                table: "Companies",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "CityName",
                table: "Companies",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "CitySubdivisionName",
                table: "Companies",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CommercialRegistrationNumber",
                table: "Companies",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "CountrySubentity",
                table: "Companies",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DeviceSerialNumber",
                table: "Companies",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "EmailAddress",
                table: "Companies",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "IdentificationCode",
                table: "Companies",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "InvoiceType",
                table: "Companies",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "PlotIdentification",
                table: "Companies",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PostalZone",
                table: "Companies",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AdditionalStreetName",
                table: "Companies");

            migrationBuilder.DropColumn(
                name: "BuildingNumber",
                table: "Companies");

            migrationBuilder.DropColumn(
                name: "BusinessIndustry",
                table: "Companies");

            migrationBuilder.DropColumn(
                name: "CityName",
                table: "Companies");

            migrationBuilder.DropColumn(
                name: "CitySubdivisionName",
                table: "Companies");

            migrationBuilder.DropColumn(
                name: "CommercialRegistrationNumber",
                table: "Companies");

            migrationBuilder.DropColumn(
                name: "CountrySubentity",
                table: "Companies");

            migrationBuilder.DropColumn(
                name: "DeviceSerialNumber",
                table: "Companies");

            migrationBuilder.DropColumn(
                name: "EmailAddress",
                table: "Companies");

            migrationBuilder.DropColumn(
                name: "IdentificationCode",
                table: "Companies");

            migrationBuilder.DropColumn(
                name: "InvoiceType",
                table: "Companies");

            migrationBuilder.DropColumn(
                name: "PlotIdentification",
                table: "Companies");

            migrationBuilder.DropColumn(
                name: "PostalZone",
                table: "Companies");

            migrationBuilder.RenameColumn(
                name: "TaxRegistrationNumber",
                table: "Companies",
                newName: "OrganizationIdentifier");

            migrationBuilder.RenameColumn(
                name: "StreetName",
                table: "Companies",
                newName: "LocationAddress");

            migrationBuilder.RenameColumn(
                name: "SchemeId",
                table: "Companies",
                newName: "IndustryBusinessCategory");

            migrationBuilder.RenameIndex(
                name: "IX_Companies_TaxRegistrationNumber",
                table: "Companies",
                newName: "IX_Companies_OrganizationIdentifier");
        }
    }
}
