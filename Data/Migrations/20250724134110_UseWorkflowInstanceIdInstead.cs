using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LocalGov360.Migrations
{
    /// <inheritdoc />
    public partial class UseWorkflowInstanceIdInstead : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ApplicationUserId",
                table: "ServiceSubmissions");

            migrationBuilder.AddColumn<Guid>(
                name: "WorkflowInstanceId",
                table: "ServiceSubmissions",
                type: "uniqueidentifier",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "WorkflowInstanceId",
                table: "ServiceSubmissions");

            migrationBuilder.AddColumn<string>(
                name: "ApplicationUserId",
                table: "ServiceSubmissions",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }
    }
}
