using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LocalGov360.Migrations
{
    public partial class AddServiceIdToWorkflowInstance : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Add nullable ServiceId column
            migrationBuilder.AddColumn<int>(
                name: "ServiceId",
                table: "WorkflowInstances",
                type: "int",
                nullable: true);

            // Create index on ServiceId for FK performance
            migrationBuilder.CreateIndex(
                name: "IX_WorkflowInstances_ServiceId",
                table: "WorkflowInstances",
                column: "ServiceId");

            // Add foreign key constraint linking to Services table
            migrationBuilder.AddForeignKey(
                name: "FK_WorkflowInstances_Services_ServiceId",
                table: "WorkflowInstances",
                column: "ServiceId",
                principalTable: "Services",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict); // or SetNull if you prefer
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Drop FK constraint
            migrationBuilder.DropForeignKey(
                name: "FK_WorkflowInstances_Services_ServiceId",
                table: "WorkflowInstances");

            // Drop index on ServiceId
            migrationBuilder.DropIndex(
                name: "IX_WorkflowInstances_ServiceId",
                table: "WorkflowInstances");

            // Drop ServiceId column
            migrationBuilder.DropColumn(
                name: "ServiceId",
                table: "WorkflowInstances");
        }
    }
}
