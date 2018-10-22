using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Depanneur.App.Migrations
{
    public partial class purchase_productname : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ProductName",
                table: "Transactions",
                nullable: true);

            migrationBuilder.Sql("UPDATE Transactions SET ProductName = (SELECT Name FROM Products p WHERE p.Id = ProductId) WHERE Discriminator = 'Purchase'");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ProductName",
                table: "Transactions");
        }
    }
}
