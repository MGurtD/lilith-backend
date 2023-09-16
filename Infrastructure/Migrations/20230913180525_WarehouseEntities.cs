using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Migrations
{
    public partial class WarehouseEntities : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Areas_Sites_SiteId",
                table: "Areas");

            migrationBuilder.DropForeignKey(
                name: "FK_Workcenters_Areas_AreaId",
                table: "Workcenters");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Area",
                table: "Areas");

            migrationBuilder.RenameTable(
                name: "Areas",
                newName: "Warehouses");

            migrationBuilder.RenameIndex(
                name: "IX_Areas_SiteId",
                table: "Warehouses",
                newName: "IX_Warehouses_SiteId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Warehouse",
                table: "Warehouses",
                column: "Id");

            migrationBuilder.CreateTable(
                name: "RawMaterialTypes",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "varchar", maxLength: 50, nullable: false),
                    Description = table.Column<string>(type: "varchar", maxLength: 250, nullable: false),
                    CreatedOn = table.Column<DateTime>(type: "timestamp without time zone", nullable: false, defaultValueSql: "NOW()"),
                    UpdatedOn = table.Column<DateTime>(type: "timestamp without time zone", nullable: false, defaultValueSql: "NOW()"),
                    Disabled = table.Column<bool>(type: "bool", nullable: false, defaultValue: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RawMaterialType", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "UK_Warehouse_Name",
                table: "Warehouses",
                column: "Name");

            migrationBuilder.CreateIndex(
                name: "UK_RawMaterialType_Name",
                table: "RawMaterialTypes",
                column: "Name");

            migrationBuilder.AddForeignKey(
                name: "FK_Warehouses_Sites_SiteId",
                table: "Warehouses",
                column: "SiteId",
                principalTable: "Sites",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Workcenters_Warehouses_AreaId",
                table: "Workcenters",
                column: "AreaId",
                principalTable: "Warehouses",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Warehouses_Sites_SiteId",
                table: "Warehouses");

            migrationBuilder.DropForeignKey(
                name: "FK_Workcenters_Warehouses_AreaId",
                table: "Workcenters");

            migrationBuilder.DropTable(
                name: "RawMaterialTypes");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Warehouse",
                table: "Warehouses");

            migrationBuilder.DropIndex(
                name: "UK_Warehouse_Name",
                table: "Warehouses");

            migrationBuilder.RenameTable(
                name: "Warehouses",
                newName: "Areas");

            migrationBuilder.RenameIndex(
                name: "IX_Warehouses_SiteId",
                table: "Areas",
                newName: "IX_Areas_SiteId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Area",
                table: "Areas",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Areas_Sites_SiteId",
                table: "Areas",
                column: "SiteId",
                principalTable: "Sites",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Workcenters_Areas_AreaId",
                table: "Workcenters",
                column: "AreaId",
                principalTable: "Areas",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
