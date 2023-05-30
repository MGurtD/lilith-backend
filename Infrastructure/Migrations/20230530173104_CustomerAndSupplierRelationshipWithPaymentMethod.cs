using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Migrations
{
    public partial class CustomerAndSupplierRelationshipWithPaymentMethod : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "PaymentMethodId",
                schema: "Config",
                table: "Suppliers",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<string>(
                name: "AccountNumber",
                schema: "Config",
                table: "Customers",
                type: "varchar",
                maxLength: 25,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Observations",
                schema: "Config",
                table: "Customers",
                type: "varchar",
                nullable: false,
                defaultValue: "4000");

            migrationBuilder.AddColumn<Guid>(
                name: "PaymentMethodId",
                schema: "Config",
                table: "Customers",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.CreateIndex(
                name: "IX_Suppliers_PaymentMethodId",
                schema: "Config",
                table: "Suppliers",
                column: "PaymentMethodId");

            migrationBuilder.CreateIndex(
                name: "IX_Customers_PaymentMethodId",
                schema: "Config",
                table: "Customers",
                column: "PaymentMethodId");

            migrationBuilder.AddForeignKey(
                name: "FK_Customers_PaymentMethods_PaymentMethodId",
                schema: "Config",
                table: "Customers",
                column: "PaymentMethodId",
                principalSchema: "Config",
                principalTable: "PaymentMethods",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Suppliers_PaymentMethods_PaymentMethodId",
                schema: "Config",
                table: "Suppliers",
                column: "PaymentMethodId",
                principalSchema: "Config",
                principalTable: "PaymentMethods",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Customers_PaymentMethods_PaymentMethodId",
                schema: "Config",
                table: "Customers");

            migrationBuilder.DropForeignKey(
                name: "FK_Suppliers_PaymentMethods_PaymentMethodId",
                schema: "Config",
                table: "Suppliers");

            migrationBuilder.DropIndex(
                name: "IX_Suppliers_PaymentMethodId",
                schema: "Config",
                table: "Suppliers");

            migrationBuilder.DropIndex(
                name: "IX_Customers_PaymentMethodId",
                schema: "Config",
                table: "Customers");

            migrationBuilder.DropColumn(
                name: "PaymentMethodId",
                schema: "Config",
                table: "Suppliers");

            migrationBuilder.DropColumn(
                name: "AccountNumber",
                schema: "Config",
                table: "Customers");

            migrationBuilder.DropColumn(
                name: "Observations",
                schema: "Config",
                table: "Customers");

            migrationBuilder.DropColumn(
                name: "PaymentMethodId",
                schema: "Config",
                table: "Customers");
        }
    }
}
