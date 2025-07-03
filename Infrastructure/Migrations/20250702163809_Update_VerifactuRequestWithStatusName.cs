using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Migrations
{
    public partial class Update_VerifactuRequestWithStatusName : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "Status",
                table: "SalesInvoiceVerifactuRequest",
                type: "varchar",
                maxLength: 512,
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(bool),
                oldType: "boolean",
                oldNullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "Success",
                table: "SalesInvoiceVerifactuRequest",
                type: "boolean",
                nullable: false,
                defaultValue: false);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Success",
                table: "SalesInvoiceVerifactuRequest");

            migrationBuilder.AlterColumn<bool>(
                name: "Status",
                table: "SalesInvoiceVerifactuRequest",
                type: "boolean",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "varchar",
                oldMaxLength: 512);
        }
    }
}
