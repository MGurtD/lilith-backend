using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Migrations
{
    public partial class CustomerContact : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "config");

            migrationBuilder.CreateTable(
                name: "CustomerTypes",
                schema: "Config",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "varchar", maxLength: 250, nullable: false),
                    Disabled = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedOn = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "NOW()"),
                    UpdatedOn = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "NOW()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CustomerType", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Customers",
                schema: "Config",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Code = table.Column<string>(type: "varchar", maxLength: 10, nullable: false),
                    ComercialName = table.Column<string>(type: "varchar", maxLength: 500, nullable: false),
                    TaxName = table.Column<string>(type: "varchar", maxLength: 500, nullable: false),
                    VatNumber = table.Column<string>(type: "varchar", maxLength: 20, nullable: false),
                    Web = table.Column<string>(type: "varchar", maxLength: 150, nullable: false),
                    Disabled = table.Column<bool>(type: "bool", nullable: false, defaultValue: false),
                    CustomerTypeId = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedOn = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "NOW()"),
                    UpdatedOn = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "NOW()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Customer", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Customers_CustomerTypes_CustomerTypeId",
                        column: x => x.CustomerTypeId,
                        principalSchema: "Config",
                        principalTable: "CustomerTypes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "CustomerAddress",
                schema: "Config",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "varchar", maxLength: 250, nullable: false),
                    MainAddress = table.Column<bool>(type: "bool", nullable: false, defaultValue: false),
                    Address = table.Column<string>(type: "varchar", maxLength: 250, nullable: false),
                    AddressExtraInfo = table.Column<string>(type: "text", nullable: false),
                    Country = table.Column<string>(type: "varchar", maxLength: 250, nullable: false),
                    Region = table.Column<string>(type: "varchar", maxLength: 250, nullable: false),
                    City = table.Column<string>(type: "varchar", maxLength: 250, nullable: false),
                    PostalCode = table.Column<string>(type: "varchar", maxLength: 10, nullable: false),
                    Disabled = table.Column<bool>(type: "bool", nullable: false, defaultValue: false),
                    CustomerId = table.Column<Guid>(type: "uuid", nullable: true),
                    CreatedOn = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "NOW()"),
                    UpdatedOn = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "NOW()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CustomerAddress", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CustomerAddress_Customers_CustomerId",
                        column: x => x.CustomerId,
                        principalSchema: "Config",
                        principalTable: "Customers",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "CustomerContacts",
                schema: "config",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    FirstName = table.Column<string>(type: "varchar", maxLength: 250, nullable: false),
                    LastName = table.Column<string>(type: "varchar", maxLength: 250, nullable: false),
                    Charge = table.Column<string>(type: "varchar", maxLength: 250, nullable: false),
                    Email = table.Column<string>(type: "varchar", maxLength: 250, nullable: false),
                    PhoneNumber = table.Column<string>(type: "varchar", maxLength: 20, nullable: false),
                    Disabled = table.Column<bool>(type: "bool", nullable: false, defaultValue: false),
                    MainContact = table.Column<bool>(type: "bool", nullable: false, defaultValue: false),
                    CustomerId = table.Column<Guid>(type: "uuid", nullable: false),
                    CustomerAddressId = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedOn = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "NOW()"),
                    UpdatedOn = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "NOW()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CustomerContact", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CustomerContacts_Customers_CustomerId",
                        column: x => x.CustomerId,
                        principalSchema: "Config",
                        principalTable: "Customers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_CustomerAddress_CustomerId",
                schema: "Config",
                table: "CustomerAddress",
                column: "CustomerId");

            migrationBuilder.CreateIndex(
                name: "UK_CustomerAddress_Name",
                schema: "Config",
                table: "CustomerAddress",
                column: "Name");

            migrationBuilder.CreateIndex(
                name: "IX_CustomerContacts_CustomerId",
                schema: "config",
                table: "CustomerContacts",
                column: "CustomerId");

            migrationBuilder.CreateIndex(
                name: "IX_Customers_CustomerTypeId",
                schema: "Config",
                table: "Customers",
                column: "CustomerTypeId");

            migrationBuilder.CreateIndex(
                name: "UK_Customer_Name",
                schema: "Config",
                table: "Customers",
                column: "ComercialName",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "UK_Customer_VatNumber",
                schema: "Config",
                table: "Customers",
                column: "VatNumber",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "UK_CustomerType_Name",
                schema: "Config",
                table: "CustomerTypes",
                column: "Name",
                unique: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CustomerAddress",
                schema: "Config");

            migrationBuilder.DropTable(
                name: "CustomerContacts",
                schema: "config");

            migrationBuilder.DropTable(
                name: "Customers",
                schema: "Config");

            migrationBuilder.DropTable(
                name: "CustomerTypes",
                schema: "Config");
        }
    }
}
