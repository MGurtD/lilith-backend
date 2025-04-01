using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Migrations
{
    public partial class Add_SalesInvoiceHash : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "SalesInvoiceHash",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    SalesInvoiceID = table.Column<Guid>(type: "uuid", maxLength: 512, nullable: false),
                    IDEmisorFactura = table.Column<string>(type: "varchar", maxLength: 512, nullable: false),
                    NumSerieFactura = table.Column<string>(type: "varchar", maxLength: 512, nullable: false),
                    FechaExpedicionFactura = table.Column<string>(type: "varchar", maxLength: 512, nullable: false),
                    TipoFactura = table.Column<string>(type: "varchar", maxLength: 512, nullable: false),
                    CuotaTotal = table.Column<string>(type: "varchar", maxLength: 512, nullable: false),
                    ImporteTotal = table.Column<string>(type: "varchar", maxLength: 512, nullable: false),
                    Huella = table.Column<string>(type: "varchar", maxLength: 512, nullable: false),
                    FechaHoraHusoGenRegistro = table.Column<DateTime>(type: "timestamp", maxLength: 512, nullable: false),
                    IDEmisorFacturaAnulada = table.Column<string>(type: "varchar", maxLength: 512, nullable: false),
                    NumSerieFacturaAnulada = table.Column<string>(type: "varchar", maxLength: 512, nullable: false),
                    FechaExpedicionFacturaAnulada = table.Column<DateTime>(type: "timestamp", maxLength: 512, nullable: false),
                    Response = table.Column<string>(type: "varchar", nullable: false),
                    Status = table.Column<string>(type: "text", nullable: false),
                    TimeStampResponse = table.Column<DateTime>(type: "timestamp", nullable: false),
                    CreatedOn = table.Column<DateTime>(type: "timestamp without time zone", nullable: false, defaultValueSql: "NOW()"),
                    UpdatedOn = table.Column<DateTime>(type: "timestamp without time zone", nullable: false, defaultValueSql: "NOW()"),
                    Disabled = table.Column<bool>(type: "bool", nullable: false, defaultValue: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SalesInvoiceHash", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SalesInvoiceHash_SalesInvoice_SalesInvoiceID",
                        column: x => x.SalesInvoiceID,
                        principalTable: "SalesInvoice",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_SalesInvoiceHash_SalesInvoiceID",
                table: "SalesInvoiceHash",
                column: "SalesInvoiceID",
                unique: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "SalesInvoiceHash");
        }
    }
}
