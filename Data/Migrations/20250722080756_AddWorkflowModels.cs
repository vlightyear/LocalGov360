using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace LocalGov360.Migrations
{
    /// <inheritdoc />
    public partial class AddWorkflowModels : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Workflows",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: false),
                    Status = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    InitiatedBy = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()"),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ContextData = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Workflows", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "WorkflowSteps",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Type = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Status = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Order = table.Column<int>(type: "int", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: false),
                    StartedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CompletedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    AssignedTo = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    Comments = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    WorkflowId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    StepTypeDiscriminator = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    StepData = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WorkflowSteps", x => x.Id);
                    table.ForeignKey(
                        name: "FK_WorkflowSteps_Workflows_WorkflowId",
                        column: x => x.WorkflowId,
                        principalTable: "Workflows",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.InsertData(
                table: "Workflows",
                columns: new[] { "Id", "ContextData", "CreatedAt", "Description", "InitiatedBy", "Name", "Status", "UpdatedAt" },
                values: new object[] { new Guid("b9c59159-1e89-4333-b705-823997c15500"), "{\"InitialAmount\": 1500.00}", new DateTime(2025, 7, 22, 8, 7, 55, 773, DateTimeKind.Utc).AddTicks(1696), "Sample workflow for testing", "system@company.com", "Sample Purchase Workflow", "Pending", null });

            migrationBuilder.InsertData(
                table: "WorkflowSteps",
                columns: new[] { "Id", "AssignedTo", "Comments", "CompletedAt", "Description", "Name", "Order", "StartedAt", "Status", "StepData", "StepTypeDiscriminator", "Type", "WorkflowId" },
                values: new object[,]
                {
                    { new Guid("083844cf-327a-4144-af00-26dd8ff71e68"), "finance@company.com", null, null, "Finance team approval", "Finance Approval", 2, null, "Pending", "{\"RequiredApprovers\":[\"finance@company.com\",\"cfo@company.com\"],\"ActualApprovers\":[],\"RequiresAllApprovers\":false,\"MinimumApprovals\":1}", "ApprovalStep", "Approval", new Guid("b9c59159-1e89-4333-b705-823997c15500") },
                    { new Guid("258fa5db-c6f0-4c39-92f1-87db970db0eb"), "accounts@company.com", null, null, "Process vendor payment", "Process Payment", 3, null, "Pending", "{\"Amount\":1500.00,\"Currency\":\"ZMW\",\"PaymentMethod\":null,\"TransactionId\":null}", "PaymentStep", "Payment", new Guid("b9c59159-1e89-4333-b705-823997c15500") },
                    { new Guid("40df7175-3657-4314-aefb-98ce42a6821a"), "manager@company.com", null, null, "Manager approval required", "Manager Approval", 1, null, "Pending", "{\"RequiredApprovers\":[\"manager@company.com\"],\"ActualApprovers\":[],\"RequiresAllApprovers\":true,\"MinimumApprovals\":1}", "ApprovalStep", "Approval", new Guid("b9c59159-1e89-4333-b705-823997c15500") }
                });

            migrationBuilder.CreateIndex(
                name: "IX_WorkflowSteps_Status",
                table: "WorkflowSteps",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_WorkflowSteps_WorkflowId_Order",
                table: "WorkflowSteps",
                columns: new[] { "WorkflowId", "Order" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "WorkflowSteps");

            migrationBuilder.DropTable(
                name: "Workflows");
        }
    }
}
