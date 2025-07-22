using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace LocalGov360.Migrations
{
    /// <inheritdoc />
    public partial class RevisedWorkflowModels : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "WorkflowSteps");

            migrationBuilder.DropTable(
                name: "Workflows");

            migrationBuilder.CreateTable(
                name: "WorkflowTemplates",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WorkflowTemplates", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "WorkflowInstances",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    InitiatedBy = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Status = table.Column<int>(type: "int", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    WorkflowTemplateId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ContextJson = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WorkflowInstances", x => x.Id);
                    table.ForeignKey(
                        name: "FK_WorkflowInstances_WorkflowTemplates_WorkflowTemplateId",
                        column: x => x.WorkflowTemplateId,
                        principalTable: "WorkflowTemplates",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "WorkflowTemplateSteps",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Order = table.Column<int>(type: "int", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    WorkflowTemplateId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Discriminator = table.Column<string>(type: "nvarchar(21)", maxLength: 21, nullable: false),
                    RequiredApprovers = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    RequiresAll = table.Column<bool>(type: "bit", nullable: true),
                    MinimumApprovals = table.Column<int>(type: "int", nullable: true),
                    Amount = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    Currency = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WorkflowTemplateSteps", x => x.Id);
                    table.ForeignKey(
                        name: "FK_WorkflowTemplateSteps_WorkflowTemplates_WorkflowTemplateId",
                        column: x => x.WorkflowTemplateId,
                        principalTable: "WorkflowTemplates",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "WorkflowInstanceSteps",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Order = table.Column<int>(type: "int", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Type = table.Column<int>(type: "int", nullable: false),
                    Status = table.Column<int>(type: "int", nullable: false),
                    StartedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CompletedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    AssignedTo = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Comments = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    WorkflowInstanceId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Discriminator = table.Column<string>(type: "nvarchar(21)", maxLength: 21, nullable: false),
                    RequiredApprovers = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ActualApprovers = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    RequiresAll = table.Column<bool>(type: "bit", nullable: true),
                    MinimumApprovals = table.Column<int>(type: "int", nullable: true),
                    Amount = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    Currency = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    TransactionId = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WorkflowInstanceSteps", x => x.Id);
                    table.ForeignKey(
                        name: "FK_WorkflowInstanceSteps_WorkflowInstances_WorkflowInstanceId",
                        column: x => x.WorkflowInstanceId,
                        principalTable: "WorkflowInstances",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_WorkflowInstances_WorkflowTemplateId",
                table: "WorkflowInstances",
                column: "WorkflowTemplateId");

            migrationBuilder.CreateIndex(
                name: "IX_WorkflowInstanceSteps_WorkflowInstanceId",
                table: "WorkflowInstanceSteps",
                column: "WorkflowInstanceId");

            migrationBuilder.CreateIndex(
                name: "IX_WorkflowTemplateSteps_WorkflowTemplateId",
                table: "WorkflowTemplateSteps",
                column: "WorkflowTemplateId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "WorkflowInstanceSteps");

            migrationBuilder.DropTable(
                name: "WorkflowTemplateSteps");

            migrationBuilder.DropTable(
                name: "WorkflowInstances");

            migrationBuilder.DropTable(
                name: "WorkflowTemplates");

            migrationBuilder.CreateTable(
                name: "Workflows",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ContextData = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()"),
                    Description = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: false),
                    InitiatedBy = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    Name = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Status = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
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
                    WorkflowId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    AssignedTo = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    Comments = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CompletedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Description = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: false),
                    Name = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Order = table.Column<int>(type: "int", nullable: false),
                    StartedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Status = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    StepData = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    StepTypeDiscriminator = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Type = table.Column<string>(type: "nvarchar(max)", nullable: false)
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
    }
}
