using Microsoft.EntityFrameworkCore.Migrations;
using System;
using System.Collections.Generic;

namespace ShopTracker.Migrations
{
    public partial class FixItemPurchaseRelation : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Purchases_ItemID",
                table: "Purchases");

            migrationBuilder.CreateIndex(
                name: "IX_Purchases_ItemID",
                table: "Purchases",
                column: "ItemID");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Purchases_ItemID",
                table: "Purchases");

            migrationBuilder.CreateIndex(
                name: "IX_Purchases_ItemID",
                table: "Purchases",
                column: "ItemID",
                unique: true);
        }
    }
}
