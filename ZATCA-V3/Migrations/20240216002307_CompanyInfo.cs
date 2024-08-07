using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ZATCA_V3.Migrations
{
    public partial class CompanyInfo : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "CompanyInfos",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    SchemeID = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    StreetName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    AdditionalStreetName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    BuildingNumber = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    PlotIdentification = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CityName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    PostalZone = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CountrySubentity = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CitySubdivisionName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    IdentificationCode = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    RegistrationName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    taxRegistrationNumber = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CompanyId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CompanyInfos", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CompanyInfos_Companies_CompanyId",
                        column: x => x.CompanyId,
                        principalTable: "Companies",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_CompanyInfos_CompanyId",
                table: "CompanyInfos",
                column: "CompanyId",
                unique: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CompanyInfos");
        }
    }
}
