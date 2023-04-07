using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Migrations
{
    public partial class EnterpiseSiteRelationship : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "EnterpriseId",
                schema: "Config",
                table: "Sites",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.CreateIndex(
                name: "IX_Sites_EnterpriseId",
                schema: "Config",
                table: "Sites",
                column: "EnterpriseId");

            migrationBuilder.AddForeignKey(
                name: "FK_Sites_Enterprises_EnterpriseId",
                schema: "Config",
                table: "Sites",
                column: "EnterpriseId",
                principalSchema: "Config",
                principalTable: "Enterprises",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Sites_Enterprises_EnterpriseId",
                schema: "Config",
                table: "Sites");

            migrationBuilder.DropIndex(
                name: "IX_Sites_EnterpriseId",
                schema: "Config",
                table: "Sites");

            migrationBuilder.DropColumn(
                name: "EnterpriseId",
                schema: "Config",
                table: "Sites");
        }
    }
}
