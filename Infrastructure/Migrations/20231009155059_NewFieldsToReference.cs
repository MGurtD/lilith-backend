using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Migrations
{
    public partial class NewFieldsToReference : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "Density",
                table: "References",
                type: "numeric(18,4)",
                precision: 18,
                scale: 4,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<int>(
                name: "FormatId",
                table: "References",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<decimal>(
                name: "LastPurchaseCost",
                table: "References",
                type: "numeric(18,4)",
                precision: 18,
                scale: 4,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<Guid>(
                name: "ReferenceTypeId",
                table: "References",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.CreateIndex(
                name: "IX_References_ReferenceTypeId",
                table: "References",
                column: "ReferenceTypeId");

            migrationBuilder.AddForeignKey(
                name: "FK_References_ReferenceTypes_ReferenceTypeId",
                table: "References",
                column: "ReferenceTypeId",
                principalTable: "ReferenceTypes",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_References_ReferenceTypes_ReferenceTypeId",
                table: "References");

            migrationBuilder.DropIndex(
                name: "IX_References_ReferenceTypeId",
                table: "References");

            migrationBuilder.DropColumn(
                name: "Density",
                table: "References");

            migrationBuilder.DropColumn(
                name: "FormatId",
                table: "References");

            migrationBuilder.DropColumn(
                name: "LastPurchaseCost",
                table: "References");

            migrationBuilder.DropColumn(
                name: "ReferenceTypeId",
                table: "References");
        }
    }
}
