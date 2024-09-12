﻿using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Migrations
{
    public partial class Addpurchaseorderidtoworkorderphase : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "PurchaseOrderId",
                table: "WorkMasterPhase",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_WorkMasterPhase_PurchaseOrderId",
                table: "WorkMasterPhase",
                column: "PurchaseOrderId");

            migrationBuilder.AddForeignKey(
                name: "FK_WorkMasterPhase_PurchaseOrders_PurchaseOrderId",
                table: "WorkMasterPhase",
                column: "PurchaseOrderId",
                principalTable: "PurchaseOrders",
                principalColumn: "Id");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_WorkMasterPhase_PurchaseOrders_PurchaseOrderId",
                table: "WorkMasterPhase");

            migrationBuilder.DropIndex(
                name: "IX_WorkMasterPhase_PurchaseOrderId",
                table: "WorkMasterPhase");

            migrationBuilder.DropColumn(
                name: "PurchaseOrderId",
                table: "WorkMasterPhase");
        }
    }
}
