using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Depanneur.App.Migrations
{
    public partial class purchasesfromsubscription : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsFromSubscription",
                table: "Transactions",
                nullable: true);

                migrationBuilder.Sql("UPDATE Transactions SET IsFromSubscription = 0 WHERE Discriminator = 'Purchase'");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsFromSubscription",
                table: "Transactions");
        }
    }
}
