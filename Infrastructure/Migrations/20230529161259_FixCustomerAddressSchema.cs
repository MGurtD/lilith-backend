using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Migrations
{
    public partial class FixCustomerAddressSchema : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameTable(
                name: "CustomerContacts",
                schema: "config",
                newName: "CustomerContacts",
                newSchema: "Config");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "config");

            migrationBuilder.RenameTable(
                name: "CustomerContacts",
                schema: "Config",
                newName: "CustomerContacts",
                newSchema: "config");
        }
    }
}
