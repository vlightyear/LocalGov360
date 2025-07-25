using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LocalGov360.Migrations
{
    /// <inheritdoc />
    public partial class AlterInitiatedBy : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "InitiatedBy",
                table: "WorkflowInstances");

            migrationBuilder.AddColumn<string>(
                name: "InitiatedById",
                table: "WorkflowInstances",
                type: "nvarchar(450)",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_WorkflowInstances_InitiatedById",
                table: "WorkflowInstances",
                column: "InitiatedById");

            migrationBuilder.AddForeignKey(
                name: "FK_WorkflowInstances_Users_InitiatedById",
                table: "WorkflowInstances",
                column: "InitiatedById",
                principalTable: "Users",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_WorkflowInstances_Users_InitiatedById",
                table: "WorkflowInstances");

            migrationBuilder.DropIndex(
                name: "IX_WorkflowInstances_InitiatedById",
                table: "WorkflowInstances");

            migrationBuilder.DropColumn(
                name: "InitiatedById",
                table: "WorkflowInstances");

            migrationBuilder.AddColumn<string>(
                name: "InitiatedBy",
                table: "WorkflowInstances",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }
    }
}
