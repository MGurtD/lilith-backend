using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Migrations
{
    public partial class DatabaseCreation : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "Config");

            migrationBuilder.CreateTable(
                name: "Enterprises",
                schema: "Config",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "varchar", maxLength: 10, nullable: false),
                    Description = table.Column<string>(type: "varchar", maxLength: 250, nullable: false),
                    IsActive = table.Column<bool>(type: "bool", nullable: false, defaultValue: false),
                    CreatedOn = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "NOW()"),
                    UpdatedOn = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "NOW()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Enterprise", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "UserRefreshTokens",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    JwtId = table.Column<Guid>(type: "uuid", nullable: false),
                    Token = table.Column<Guid>(type: "uuid", nullable: false),
                    Used = table.Column<bool>(type: "bool", nullable: false, defaultValue: false),
                    Revoked = table.Column<bool>(type: "bool", nullable: false, defaultValue: false),
                    ExpiryDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatedOn = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "NOW()"),
                    UpdatedOn = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "NOW()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserRefreshToken", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Users",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Username = table.Column<string>(type: "varchar", maxLength: 250, nullable: false),
                    Email = table.Column<string>(type: "text", nullable: false),
                    Password = table.Column<string>(type: "varchar", maxLength: 250, nullable: false),
                    FirstName = table.Column<string>(type: "varchar", maxLength: 250, nullable: false),
                    LastName = table.Column<string>(type: "varchar", maxLength: 250, nullable: false),
                    Disabled = table.Column<bool>(type: "bool", nullable: false, defaultValue: false),
                    CreatedOn = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "NOW()"),
                    UpdatedOn = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "NOW()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Users", x => x.Id);
                });

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
                    EnterpriseId = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedOn = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "NOW()"),
                    UpdatedOn = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "NOW()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Site", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Sites_Enterprises_EnterpriseId",
                        column: x => x.EnterpriseId,
                        principalSchema: "Config",
                        principalTable: "Enterprises",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Roles",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "varchar", maxLength: 250, nullable: false),
                    Description = table.Column<string>(type: "varchar", maxLength: 250, nullable: false),
                    Disabled = table.Column<bool>(type: "bool", nullable: false, defaultValue: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: true),
                    CreatedOn = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "NOW()"),
                    UpdatedOn = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "NOW()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Role", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Roles_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateIndex(
                name: "UK_Enterprise_Name",
                schema: "Config",
                table: "Enterprises",
                column: "Name");

            migrationBuilder.CreateIndex(
                name: "IX_Roles_UserId",
                table: "Roles",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "UK_Role_Name",
                table: "Roles",
                column: "Name");

            migrationBuilder.CreateIndex(
                name: "IX_Sites_EnterpriseId",
                schema: "Config",
                table: "Sites",
                column: "EnterpriseId");

            migrationBuilder.CreateIndex(
                name: "UK_Site_Name",
                schema: "Config",
                table: "Sites",
                column: "Name");

            migrationBuilder.CreateIndex(
                name: "UK_UserRefreshToken_JwtId",
                table: "UserRefreshTokens",
                column: "JwtId");

            migrationBuilder.CreateIndex(
                name: "UK_Users_Username",
                table: "Users",
                column: "Username");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Roles");

            migrationBuilder.DropTable(
                name: "Sites",
                schema: "Config");

            migrationBuilder.DropTable(
                name: "UserRefreshTokens");

            migrationBuilder.DropTable(
                name: "Users");

            migrationBuilder.DropTable(
                name: "Enterprises",
                schema: "Config");
        }
    }
}
