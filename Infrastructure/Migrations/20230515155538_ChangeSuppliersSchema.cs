using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Migrations
{
    public partial class ChangeSuppliersSchema : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameTable(
                name: "SupplierTypes",
                newName: "SupplierTypes",
                newSchema: "Config");

            migrationBuilder.RenameTable(
                name: "Suppliers",
                newName: "Suppliers",
                newSchema: "Config");

            migrationBuilder.RenameTable(
                name: "SupplierContacts",
                newName: "SupplierContacts",
                newSchema: "Config");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameTable(
                name: "SupplierTypes",
                schema: "Config",
                newName: "SupplierTypes");

            migrationBuilder.RenameTable(
                name: "Suppliers",
                schema: "Config",
                newName: "Suppliers");

            migrationBuilder.RenameTable(
                name: "SupplierContacts",
                schema: "Config",
                newName: "SupplierContacts");
        }
    }
}
