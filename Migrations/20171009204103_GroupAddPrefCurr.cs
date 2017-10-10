using Microsoft.EntityFrameworkCore.Migrations;
using System;
using System.Collections.Generic;

namespace ShopTracker.Migrations
{
    public partial class GroupAddPrefCurr : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "PrefCurrID",
                table: "Groups",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Groups_PrefCurrID",
                table: "Groups",
                column: "PrefCurrID");

            migrationBuilder.AddForeignKey(
                name: "FK_Groups_Currencies_PrefCurrID",
                table: "Groups",
                column: "PrefCurrID",
                principalTable: "Currencies",
                principalColumn: "CurrencyID",
                onDelete: ReferentialAction.Restrict);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Groups_Currencies_PrefCurrID",
                table: "Groups");

            migrationBuilder.DropIndex(
                name: "IX_Groups_PrefCurrID",
                table: "Groups");

            migrationBuilder.DropColumn(
                name: "PrefCurrID",
                table: "Groups");
        }
    }
}
