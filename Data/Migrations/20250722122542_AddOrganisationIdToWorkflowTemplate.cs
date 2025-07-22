using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LocalGov360.Migrations
{
    /// <inheritdoc />
    public partial class AddOrganisationIdToWorkflowTemplate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "OrganisationId",
                table: "WorkflowTemplates",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_WorkflowTemplates_OrganisationId",
                table: "WorkflowTemplates",
                column: "OrganisationId");

            migrationBuilder.AddForeignKey(
                name: "FK_WorkflowTemplates_Organisations_OrganisationId",
                table: "WorkflowTemplates",
                column: "OrganisationId",
                principalTable: "Organisations",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_WorkflowTemplates_Organisations_OrganisationId",
                table: "WorkflowTemplates");

            migrationBuilder.DropIndex(
                name: "IX_WorkflowTemplates_OrganisationId",
                table: "WorkflowTemplates");

            migrationBuilder.DropColumn(
                name: "OrganisationId",
                table: "WorkflowTemplates");
        }
    }
}
