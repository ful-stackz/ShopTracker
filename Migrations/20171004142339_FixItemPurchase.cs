using Microsoft.EntityFrameworkCore.Migrations;
using System;
using System.Collections.Generic;

namespace ShopTracker.Migrations
{
    public partial class FixItemPurchase : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Items_Purchases_PurchaseID",
                table: "Items");

            migrationBuilder.DropIndex(
                name: "IX_Items_PurchaseID",
                table: "Items");

            migrationBuilder.DropColumn(
                name: "PurchaseID",
                table: "Items");

            migrationBuilder.AddColumn<int>(
                name: "ItemID",
                table: "Purchases",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_Purchases_ItemID",
                table: "Purchases",
                column: "ItemID",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Purchases_Items_ItemID",
                table: "Purchases",
                column: "ItemID",
                principalTable: "Items",
                principalColumn: "ItemID",
                onDelete: ReferentialAction.Cascade);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Purchases_Items_ItemID",
                table: "Purchases");

            migrationBuilder.DropIndex(
                name: "IX_Purchases_ItemID",
                table: "Purchases");

            migrationBuilder.DropColumn(
                name: "ItemID",
                table: "Purchases");

            migrationBuilder.AddColumn<int>(
                name: "PurchaseID",
                table: "Items",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_Items_PurchaseID",
                table: "Items",
                column: "PurchaseID",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Items_Purchases_PurchaseID",
                table: "Items",
                column: "PurchaseID",
                principalTable: "Purchases",
                principalColumn: "PurchaseID",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
