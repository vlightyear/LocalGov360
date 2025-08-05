using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LocalGov360.Migrations
{
    public partial class ChangeServiceIdToInt : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Drop the old ServiceId column
            migrationBuilder.DropColumn(
                name: "ServiceId",
                table: "ServicePayments");

            // Add the new ServiceId column as int
            migrationBuilder.AddColumn<int>(
                name: "ServiceId",
                table: "ServicePayments",
                type: "int",
                nullable: false,
                defaultValue: 0); // or true if you want it nullable
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Revert to Guid if needed
            migrationBuilder.DropColumn(
                name: "ServiceId",
                table: "ServicePayments");

            migrationBuilder.AddColumn<Guid>(
                name: "ServiceId",
                table: "ServicePayments",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: Guid.Empty); // or true/null depending on old config
        }
    }
}
