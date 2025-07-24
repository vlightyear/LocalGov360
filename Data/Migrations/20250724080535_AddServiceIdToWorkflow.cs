using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LocalGov360.Migrations
{
    /// <inheritdoc />
    public partial class AddServiceIdToWorkflow : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "ServiceId",
                table: "WorkflowInstances",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_WorkflowInstances_ServiceId",
                table: "WorkflowInstances",
                column: "ServiceId");

            migrationBuilder.AddForeignKey(
                name: "FK_WorkflowInstances_Services_ServiceId",
                table: "WorkflowInstances",
                column: "ServiceId",
                principalTable: "Services",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_WorkflowInstances_Services_ServiceId",
                table: "WorkflowInstances");

            migrationBuilder.DropIndex(
                name: "IX_WorkflowInstances_ServiceId",
                table: "WorkflowInstances");

            migrationBuilder.DropColumn(
                name: "ServiceId",
                table: "WorkflowInstances");
        }
    }
}
