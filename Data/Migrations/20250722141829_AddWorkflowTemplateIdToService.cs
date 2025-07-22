using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LocalGov360.Migrations
{
    /// <inheritdoc />
    public partial class AddWorkflowTemplateIdToService : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<Guid>(
                name: "AssignedTo",
                table: "WorkflowInstanceSteps",
                type: "uniqueidentifier",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AddColumn<Guid>(
                name: "WorkflowTemplateId",
                table: "Services",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Services_WorkflowTemplateId",
                table: "Services",
                column: "WorkflowTemplateId");

            migrationBuilder.AddForeignKey(
                name: "FK_Services_WorkflowTemplates_WorkflowTemplateId",
                table: "Services",
                column: "WorkflowTemplateId",
                principalTable: "WorkflowTemplates",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Services_WorkflowTemplates_WorkflowTemplateId",
                table: "Services");

            migrationBuilder.DropIndex(
                name: "IX_Services_WorkflowTemplateId",
                table: "Services");

            migrationBuilder.DropColumn(
                name: "WorkflowTemplateId",
                table: "Services");

            migrationBuilder.AlterColumn<string>(
                name: "AssignedTo",
                table: "WorkflowInstanceSteps",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier");
        }
    }
}
