using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Migrations
{
    public partial class AddLanguageEntity : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Languages",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Code = table.Column<string>(type: "varchar", maxLength: 10, nullable: false),
                    Name = table.Column<string>(type: "varchar", maxLength: 100, nullable: false),
                    Icon = table.Column<string>(type: "varchar", maxLength: 255, nullable: false),
                    IsDefault = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    SortOrder = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    CreatedOn = table.Column<DateTime>(type: "timestamp without time zone", nullable: false, defaultValueSql: "NOW()"),
                    UpdatedOn = table.Column<DateTime>(type: "timestamp without time zone", nullable: false, defaultValueSql: "NOW()"),
                    Disabled = table.Column<bool>(type: "bool", nullable: false, defaultValue: false)
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

            // Add default languages using InsertData
            migrationBuilder.InsertData(
                table: "Languages",
                columns: ["Id", "Code", "Name", "Icon", "IsDefault", "SortOrder", "CreatedOn", "UpdatedOn", "Disabled"],
                values: new object[,]
                {
                    { new Guid("11111111-1111-1111-1111-111111111111"), "ca", "Català", "https://www.stevenskelton.ca/flag-icon/svg/es/catalonia.svg", true, 1, DateTime.UtcNow, DateTime.UtcNow, false },
                    { new Guid("22222222-2222-2222-2222-222222222222"), "es", "Español", "https://www.stevenskelton.ca/flag-icon/svg/country-4x3/es.svg", false, 2, DateTime.UtcNow, DateTime.UtcNow, false },
                    { new Guid("33333333-3333-3333-3333-333333333333"), "en", "English", "https://www.stevenskelton.ca/flag-icon/svg/country-4x3/gb.svg", false, 3, DateTime.UtcNow, DateTime.UtcNow, false }
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Languages");
        }
    }
}
