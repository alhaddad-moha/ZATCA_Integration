using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ZATCA_V3.Migrations
{
    public partial class EditCompanyInfo3 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "PartyId",
                table: "CompanyInfos",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "PartyId",
                table: "CompanyInfos");
        }
    }
}
