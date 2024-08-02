using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Migrations
{
    public partial class Add_ReferenceCategoryAndServiceImpl : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "ServiceReferenceId",
                table: "WorkOrderPhase",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "TransportCost",
                table: "WorkOrderPhase",
                type: "numeric(18,4)",
                precision: 18,
                scale: 4,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<Guid>(
                name: "ServiceReferenceId",
                table: "WorkMasterPhase",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "TransportCost",
                table: "WorkMasterPhase",
                type: "numeric(18,4)",
                precision: 18,
                scale: 4,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "ServiceCost",
                table: "SalesOrderDetail",
                type: "numeric(18,4)",
                precision: 18,
                scale: 4,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "TransportCost",
                table: "SalesOrderDetail",
                type: "numeric(18,4)",
                precision: 18,
                scale: 4,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<string>(
                name: "CategoryName",
                table: "References",
                type: "varchar",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "TransportAmount",
                table: "References",
                type: "numeric(18,2)",
                precision: 18,
                scale: 2,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "ServiceCost",
                table: "BudgetDetails",
                type: "numeric(18,4)",
                precision: 18,
                scale: 4,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "TransportCost",
                table: "BudgetDetails",
                type: "numeric(18,4)",
                precision: 18,
                scale: 4,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.CreateTable(
                name: "ReferenceCategories",
                columns: table => new
                {
                    Name = table.Column<string>(type: "varchar", maxLength: 50, nullable: false),
                    Description = table.Column<string>(type: "varchar", maxLength: 250, nullable: false),
                    Disabled = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ReferenceCategories", x => x.Name);
                });

            migrationBuilder.CreateIndex(
                name: "IX_WorkOrderPhase_ServiceReferenceId",
                table: "WorkOrderPhase",
                column: "ServiceReferenceId");

            migrationBuilder.CreateIndex(
                name: "IX_WorkMasterPhase_ServiceReferenceId",
                table: "WorkMasterPhase",
                column: "ServiceReferenceId");

            migrationBuilder.CreateIndex(
                name: "IX_References_CategoryName",
                table: "References",
                column: "CategoryName");

            migrationBuilder.AddForeignKey(
                name: "FK_References_ReferenceCategories_CategoryName",
                table: "References",
                column: "CategoryName",
                principalTable: "ReferenceCategories",
                principalColumn: "Name");

            migrationBuilder.AddForeignKey(
                name: "FK_WorkMasterPhase_References_ServiceReferenceId",
                table: "WorkMasterPhase",
                column: "ServiceReferenceId",
                principalTable: "References",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_WorkOrderPhase_References_ServiceReferenceId",
                table: "WorkOrderPhase",
                column: "ServiceReferenceId",
                principalTable: "References",
                principalColumn: "Id");

            // Create ReferenceCategories and set the category of the existing service references
            migrationBuilder.Sql("INSERT INTO \"ReferenceCategories\" (\"Name\", \"Description\", \"Disabled\")\r\n\tVALUES ('Service', 'Serveis', false), ('Material', 'Materies primeres', false), ('Tool', 'Eines', false), ('FinalProduct', 'Producte acabat', true);\r\n\t\r\nUPDATE \"References\" SET \"CategoryName\" = 'Service' WHERE \"IsService\" = true;");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_References_ReferenceCategories_CategoryName",
                table: "References");

            migrationBuilder.DropForeignKey(
                name: "FK_WorkMasterPhase_References_ServiceReferenceId",
                table: "WorkMasterPhase");

            migrationBuilder.DropForeignKey(
                name: "FK_WorkOrderPhase_References_ServiceReferenceId",
                table: "WorkOrderPhase");

            migrationBuilder.DropTable(
                name: "ReferenceCategories");

            migrationBuilder.DropIndex(
                name: "IX_WorkOrderPhase_ServiceReferenceId",
                table: "WorkOrderPhase");

            migrationBuilder.DropIndex(
                name: "IX_WorkMasterPhase_ServiceReferenceId",
                table: "WorkMasterPhase");

            migrationBuilder.DropIndex(
                name: "IX_References_CategoryName",
                table: "References");

            migrationBuilder.DropColumn(
                name: "ServiceReferenceId",
                table: "WorkOrderPhase");

            migrationBuilder.DropColumn(
                name: "TransportCost",
                table: "WorkOrderPhase");

            migrationBuilder.DropColumn(
                name: "ServiceReferenceId",
                table: "WorkMasterPhase");

            migrationBuilder.DropColumn(
                name: "TransportCost",
                table: "WorkMasterPhase");

            migrationBuilder.DropColumn(
                name: "ServiceCost",
                table: "SalesOrderDetail");

            migrationBuilder.DropColumn(
                name: "TransportCost",
                table: "SalesOrderDetail");

            migrationBuilder.DropColumn(
                name: "CategoryName",
                table: "References");

            migrationBuilder.DropColumn(
                name: "TransportAmount",
                table: "References");

            migrationBuilder.DropColumn(
                name: "ServiceCost",
                table: "BudgetDetails");

            migrationBuilder.DropColumn(
                name: "TransportCost",
                table: "BudgetDetails");
        }
    }
}
