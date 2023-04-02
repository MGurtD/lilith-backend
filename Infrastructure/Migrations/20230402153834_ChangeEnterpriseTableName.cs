using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Migrations
{
    public partial class ChangeEnterpriseTableName : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameTable(
                name: "Enterprise",
                schema: "Config",
                newName: "Enterprises",
                newSchema: "Config");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameTable(
                name: "Enterprises",
                schema: "Config",
                newName: "Enterprise",
                newSchema: "Config");
        }
    }
}
