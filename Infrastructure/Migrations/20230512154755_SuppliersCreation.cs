using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Migrations
{
    public partial class SuppliersCreation : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Suppliers",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ComercialName = table.Column<string>(type: "varchar", maxLength: 500, nullable: false),
                    TaxName = table.Column<string>(type: "varchar", maxLength: 500, nullable: false),
                    CIF = table.Column<string>(type: "varchar", maxLength: 20, nullable: false),
                    Country = table.Column<string>(type: "varchar", maxLength: 250, nullable: false),
                    Region = table.Column<string>(type: "varchar", maxLength: 250, nullable: false),
                    PostalCode = table.Column<string>(type: "varchar", maxLength: 10, nullable: false),
                    Address = table.Column<string>(type: "varchar", maxLength: 500, nullable: false),
                    Phone = table.Column<string>(type: "varchar", maxLength: 20, nullable: false),
                    Observations = table.Column<string>(type: "text", nullable: false),
                    Disabled = table.Column<bool>(type: "bool", nullable: false, defaultValue: false),
                    SupplierTypeId = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedOn = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "NOW()"),
                    UpdatedOn = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "NOW()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Supplier", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Suppliers_SupplierTypes_SupplierTypeId",
                        column: x => x.SupplierTypeId,
                        principalTable: "SupplierTypes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "SupplierContacts",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    FirstName = table.Column<string>(type: "varchar", maxLength: 250, nullable: false),
                    LastName = table.Column<string>(type: "varchar", maxLength: 250, nullable: false),
                    Charge = table.Column<string>(type: "varchar", maxLength: 250, nullable: false),
                    Email = table.Column<string>(type: "varchar", maxLength: 250, nullable: false),
                    Phone = table.Column<string>(type: "varchar", maxLength: 20, nullable: false),
                    PhoneExtension = table.Column<string>(type: "varchar", maxLength: 5, nullable: false),
                    Observations = table.Column<string>(type: "text", nullable: false),
                    Disabled = table.Column<bool>(type: "bool", nullable: false, defaultValue: false),
                    SupplierId = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedOn = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "NOW()"),
                    UpdatedOn = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "NOW()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SupplierContact", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SupplierContacts_Suppliers_SupplierId",
                        column: x => x.SupplierId,
                        principalTable: "Suppliers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_SupplierContacts_SupplierId",
                table: "SupplierContacts",
                column: "SupplierId");

            migrationBuilder.CreateIndex(
                name: "IX_Suppliers_SupplierTypeId",
                table: "Suppliers",
                column: "SupplierTypeId");

            migrationBuilder.CreateIndex(
                name: "UK_Supplier_Name",
                table: "Suppliers",
                column: "ComercialName",
                unique: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "SupplierContacts");

            migrationBuilder.DropTable(
                name: "Suppliers");
        }
    }
}
