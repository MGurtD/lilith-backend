using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Migrations
{
    public partial class ChangeCustomerAddressColumnNames : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_CustomerAddress_Customers_CustomerId",
                schema: "Config",
                table: "CustomerAddress");

            migrationBuilder.RenameColumn(
                name: "MainAddress",
                schema: "Config",
                table: "CustomerAddress",
                newName: "Default");

            migrationBuilder.RenameColumn(
                name: "AddressExtraInfo",
                schema: "Config",
                table: "CustomerAddress",
                newName: "Observations");

            migrationBuilder.AlterColumn<Guid>(
                name: "CustomerId",
                schema: "Config",
                table: "CustomerAddress",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
                oldClrType: typeof(Guid),
                oldType: "uuid",
                oldNullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_CustomerAddress_Customers_CustomerId",
                schema: "Config",
                table: "CustomerAddress",
                column: "CustomerId",
                principalSchema: "Config",
                principalTable: "Customers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_CustomerAddress_Customers_CustomerId",
                schema: "Config",
                table: "CustomerAddress");

            migrationBuilder.RenameColumn(
                name: "Observations",
                schema: "Config",
                table: "CustomerAddress",
                newName: "AddressExtraInfo");

            migrationBuilder.RenameColumn(
                name: "Default",
                schema: "Config",
                table: "CustomerAddress",
                newName: "MainAddress");

            migrationBuilder.AlterColumn<Guid>(
                name: "CustomerId",
                schema: "Config",
                table: "CustomerAddress",
                type: "uuid",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "uuid");

            migrationBuilder.AddForeignKey(
                name: "FK_CustomerAddress_Customers_CustomerId",
                schema: "Config",
                table: "CustomerAddress",
                column: "CustomerId",
                principalSchema: "Config",
                principalTable: "Customers",
                principalColumn: "Id");
        }
    }
}
