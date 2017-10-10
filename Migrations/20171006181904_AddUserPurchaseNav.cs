using Microsoft.EntityFrameworkCore.Migrations;
using System;
using System.Collections.Generic;

namespace ShopTracker.Migrations
{
    public partial class AddUserPurchaseNav : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "UserID",
                table: "Purchases",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Purchases_UserID",
                table: "Purchases",
                column: "UserID");

            migrationBuilder.AddForeignKey(
                name: "FK_Purchases_Users_UserID",
                table: "Purchases",
                column: "UserID",
                principalTable: "Users",
                principalColumn: "UserID",
                onDelete: ReferentialAction.Restrict);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Purchases_Users_UserID",
                table: "Purchases");

            migrationBuilder.DropIndex(
                name: "IX_Purchases_UserID",
                table: "Purchases");

            migrationBuilder.DropColumn(
                name: "UserID",
                table: "Purchases");
        }
    }
}
