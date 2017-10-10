using Microsoft.EntityFrameworkCore.Migrations;
using System;
using System.Collections.Generic;

namespace ShopTracker.Migrations
{
    public partial class UpdatePurchaseItemCurrency : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Items_Currencies_CurrencyID",
                table: "Items");

            migrationBuilder.DropIndex(
                name: "IX_Items_CurrencyID",
                table: "Items");

            migrationBuilder.DropColumn(
                name: "CurrencyID",
                table: "Items");

            migrationBuilder.AddColumn<int>(
                name: "CurrencyID",
                table: "Purchases",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AlterColumn<string>(
                name: "Name",
                table: "Items",
                type: "nvarchar(25)",
                maxLength: 25,
                nullable: false,
                oldClrType: typeof(string),
                oldNullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Description",
                table: "Items",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Purchases_CurrencyID",
                table: "Purchases",
                column: "CurrencyID");

            migrationBuilder.AddForeignKey(
                name: "FK_Purchases_Currencies_CurrencyID",
                table: "Purchases",
                column: "CurrencyID",
                principalTable: "Currencies",
                principalColumn: "CurrencyID",
                onDelete: ReferentialAction.Cascade);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Purchases_Currencies_CurrencyID",
                table: "Purchases");

            migrationBuilder.DropIndex(
                name: "IX_Purchases_CurrencyID",
                table: "Purchases");

            migrationBuilder.DropColumn(
                name: "CurrencyID",
                table: "Purchases");

            migrationBuilder.DropColumn(
                name: "Description",
                table: "Items");

            migrationBuilder.AlterColumn<string>(
                name: "Name",
                table: "Items",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(25)",
                oldMaxLength: 25);

            migrationBuilder.AddColumn<int>(
                name: "CurrencyID",
                table: "Items",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_Items_CurrencyID",
                table: "Items",
                column: "CurrencyID");

            migrationBuilder.AddForeignKey(
                name: "FK_Items_Currencies_CurrencyID",
                table: "Items",
                column: "CurrencyID",
                principalTable: "Currencies",
                principalColumn: "CurrencyID",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
