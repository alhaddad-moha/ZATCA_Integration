using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ZATCA_V2.Migrations
{
    public partial class SignedInovice : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "SignedInvoice",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    InvoiceHash = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    SingedXML = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    EncodedInvoice = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    UUID = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    QRCode = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    SingedXMLFileName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    InvoiceType = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Amount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Tax = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Status = table.Column<int>(type: "int", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CompanyId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SignedInvoice", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SignedInvoice_Companies_CompanyId",
                        column: x => x.CompanyId,
                        principalTable: "Companies",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_SignedInvoice_CompanyId",
                table: "SignedInvoice",
                column: "CompanyId");

            migrationBuilder.CreateIndex(
                name: "IX_SignedInvoice_InvoiceHash",
                table: "SignedInvoice",
                column: "InvoiceHash",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_SignedInvoice_UUID",
                table: "SignedInvoice",
                column: "UUID",
                unique: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "SignedInvoice");
        }
    }
}
