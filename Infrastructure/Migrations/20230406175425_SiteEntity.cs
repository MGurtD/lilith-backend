using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Migrations
{
    public partial class SiteEntity : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Sites",
                schema: "Config",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "varchar", maxLength: 50, nullable: false),
                    Description = table.Column<string>(type: "varchar", maxLength: 250, nullable: false),
                    IsActive = table.Column<bool>(type: "bool", nullable: false, defaultValue: false),
                    Address = table.Column<string>(type: "varchar", maxLength: 250, nullable: false),
                    City = table.Column<string>(type: "varchar", maxLength: 250, nullable: false),
                    PostalCode = table.Column<string>(type: "varchar", maxLength: 250, nullable: false),
                    Region = table.Column<string>(type: "varchar", maxLength: 250, nullable: false),
                    Country = table.Column<string>(type: "varchar", maxLength: 250, nullable: false),
                    PhoneNumber = table.Column<string>(type: "varchar", maxLength: 25, nullable: false),
                    Email = table.Column<string>(type: "varchar", maxLength: 250, nullable: false),
                    VatNumber = table.Column<string>(type: "varchar", maxLength: 12, nullable: false),
                    CreatedOn = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "NOW()"),
                    UpdatedOn = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Site", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "UK_Site_Name",
                schema: "Config",
                table: "Sites",
                column: "Name");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Sites",
                schema: "Config");
        }
    }
}
