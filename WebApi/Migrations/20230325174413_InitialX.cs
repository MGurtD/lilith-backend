using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Api.Migrations
{
    public partial class InitialX : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "Config");

            migrationBuilder.CreateTable(
                name: "Customers",
                schema: "Config",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "varchar", maxLength: 250, nullable: false),
                    Email = table.Column<string>(type: "varchar", maxLength: 250, nullable: false),
                    Address = table.Column<string>(type: "varchar", maxLength: 500, nullable: false),
                    City = table.Column<string>(type: "varchar", maxLength: 250, nullable: false),
                    Region = table.Column<string>(type: "varchar", maxLength: 250, nullable: false),
                    PostalCode = table.Column<string>(type: "varchar", maxLength: 8, nullable: false),
                    Country = table.Column<string>(type: "varchar", maxLength: 50, nullable: false),
                    Phone = table.Column<string>(type: "varchar", maxLength: 25, nullable: false),
                    HomePage = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Customers", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Operators",
                schema: "Config",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "varchar", maxLength: 500, nullable: false),
                    Cost = table.Column<decimal>(type: "numeric(8,2)", precision: 8, scale: 2, nullable: false, defaultValue: 0m)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Operators", x => x.Id);
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Customers",
                schema: "Config");

            migrationBuilder.DropTable(
                name: "Operators",
                schema: "Config");
        }
    }
}
