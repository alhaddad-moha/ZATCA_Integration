using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ZATCA_V3.Migrations
{
    public partial class AddCredList : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_CompanyCredentials_CompanyId",
                table: "CompanyCredentials");

            migrationBuilder.CreateIndex(
                name: "IX_CompanyCredentials_CompanyId",
                table: "CompanyCredentials",
                column: "CompanyId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_CompanyCredentials_CompanyId",
                table: "CompanyCredentials");

            migrationBuilder.CreateIndex(
                name: "IX_CompanyCredentials_CompanyId",
                table: "CompanyCredentials",
                column: "CompanyId",
                unique: true);
        }
    }
}
