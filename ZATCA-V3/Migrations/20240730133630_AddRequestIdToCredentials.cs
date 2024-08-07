using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ZATCA_V3.Migrations
{
    public partial class AddRequestIdToCredentials : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "PartyId",
                table: "CompanyInfos",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AddColumn<int>(
                name: "requestId",
                table: "CompanyCredentials",
                type: "int",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "requestId",
                table: "CompanyCredentials");

            migrationBuilder.AlterColumn<int>(
                name: "PartyId",
                table: "CompanyInfos",
                type: "int",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");
        }
    }
}
