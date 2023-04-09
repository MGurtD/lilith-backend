using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Migrations
{
    public partial class UserRefreshTokens : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "UserRefreshToken",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    JwtId = table.Column<Guid>(type: "uuid", nullable: false),
                    Token = table.Column<string>(type: "varchar", maxLength: 600, nullable: false),
                    IsActive = table.Column<bool>(type: "bool", nullable: false, defaultValue: true),
                    IsRevoked = table.Column<bool>(type: "bool", nullable: false, defaultValue: false),
                    ExpiryDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatedOn = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "NOW()"),
                    UpdatedOn = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserRefreshToken", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "UK_UserRefreshToken_JwtId",
                table: "UserRefreshToken",
                column: "JwtId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "UserRefreshToken");
        }
    }
}
