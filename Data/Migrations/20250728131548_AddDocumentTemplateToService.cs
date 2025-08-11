using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LocalGov360.Migrations
{
    /// <inheritdoc />
    public partial class AddDocumentTemplateToService : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "DocumentTemplateId",
                table: "Services",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Services_DocumentTemplateId",
                table: "Services",
                column: "DocumentTemplateId");

            migrationBuilder.CreateIndex(
                name: "IX_Services_OrganisationId",
                table: "Services",
                column: "OrganisationId");

            migrationBuilder.CreateIndex(
                name: "IX_DocumentTemplates_OrganisationId",
                table: "DocumentTemplates",
                column: "OrganisationId");

            migrationBuilder.AddForeignKey(
                name: "FK_DocumentTemplates_Organisations_OrganisationId",
                table: "DocumentTemplates",
                column: "OrganisationId",
                principalTable: "Organisations",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Services_DocumentTemplates_DocumentTemplateId",
                table: "Services",
                column: "DocumentTemplateId",
                principalTable: "DocumentTemplates",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Services_Organisations_OrganisationId",
                table: "Services",
                column: "OrganisationId",
                principalTable: "Organisations",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_DocumentTemplates_Organisations_OrganisationId",
                table: "DocumentTemplates");

            migrationBuilder.DropForeignKey(
                name: "FK_Services_DocumentTemplates_DocumentTemplateId",
                table: "Services");

            migrationBuilder.DropForeignKey(
                name: "FK_Services_Organisations_OrganisationId",
                table: "Services");

            migrationBuilder.DropIndex(
                name: "IX_Services_DocumentTemplateId",
                table: "Services");

            migrationBuilder.DropIndex(
                name: "IX_Services_OrganisationId",
                table: "Services");

            migrationBuilder.DropIndex(
                name: "IX_DocumentTemplates_OrganisationId",
                table: "DocumentTemplates");

            migrationBuilder.DropColumn(
                name: "DocumentTemplateId",
                table: "Services");
        }
    }
}
