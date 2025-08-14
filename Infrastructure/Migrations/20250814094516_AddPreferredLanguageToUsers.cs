using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Migrations
{
    public partial class AddPreferredLanguageToUsers : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Languages");

            migrationBuilder.AddColumn<string>(
                name: "PreferredLanguage",
                table: "Users",
                type: "varchar",
                maxLength: 10,
                nullable: false,
                defaultValue: "ca");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "PreferredLanguage",
                table: "Users");

            migrationBuilder.CreateTable(
                name: "Languages",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Code = table.Column<string>(type: "varchar", maxLength: 10, nullable: false),
                    CreatedOn = table.Column<DateTime>(type: "timestamp without time zone", nullable: false, defaultValueSql: "NOW()"),
                    Disabled = table.Column<bool>(type: "bool", nullable: false, defaultValue: false),
                    Icon = table.Column<string>(type: "varchar", maxLength: 255, nullable: false),
                    IsDefault = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    Name = table.Column<string>(type: "varchar", maxLength: 100, nullable: false),
                    SortOrder = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    UpdatedOn = table.Column<DateTime>(type: "timestamp without time zone", nullable: false, defaultValueSql: "NOW()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Languages", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "UK_Languages_Code",
                table: "Languages",
                column: "Code",
                unique: true);
        }
    }
}
