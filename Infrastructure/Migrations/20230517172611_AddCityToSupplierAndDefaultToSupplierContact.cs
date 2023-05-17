using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Migrations
{
    public partial class AddCityToSupplierAndDefaultToSupplierContact : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "City",
                schema: "Config",
                table: "Suppliers",
                type: "varchar",
                maxLength: 250,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<bool>(
                name: "Default",
                schema: "Config",
                table: "SupplierContacts",
                type: "bool",
                nullable: false,
                defaultValue: false);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "City",
                schema: "Config",
                table: "Suppliers");

            migrationBuilder.DropColumn(
                name: "Default",
                schema: "Config",
                table: "SupplierContacts");
        }
    }
}
