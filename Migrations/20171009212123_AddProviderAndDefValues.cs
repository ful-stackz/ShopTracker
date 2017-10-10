using Microsoft.EntityFrameworkCore.Migrations;
using System;
using System.Collections.Generic;

namespace ShopTracker.Migrations
{
    public partial class AddProviderAndDefValues : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Password",
                table: "Groups");

            migrationBuilder.DropColumn(
                name: "Salt",
                table: "Groups");

            migrationBuilder.AddColumn<string>(
                name: "Provider",
                table: "Purchases",
                type: "nvarchar(max)",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Provider",
                table: "Purchases");

            migrationBuilder.AddColumn<string>(
                name: "Password",
                table: "Groups",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Salt",
                table: "Groups",
                nullable: true);
        }
    }
}
