using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LocalGov360.Migrations
{
    /// <inheritdoc />
    public partial class AddPoundageTablesClean : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Invoice-related changes
            migrationBuilder.AddColumn<Guid>(
                name: "ServiceInvoiceId",
                table: "ServicePayments",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AlterColumn<Guid>(
                name: "ServicePaymentId",
                table: "ServiceInvoices",
                type: "uniqueidentifier",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier");

            migrationBuilder.AddColumn<Guid>(
                name: "OrganisationId",
                table: "ServiceInvoices",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "ServiceId",
                table: "ServiceInvoices",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "WorkflowInstanceId",
                table: "ServiceInvoices",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "WorkflowStepId",
                table: "ServiceInvoices",
                type: "uniqueidentifier",
                nullable: true);

            // Poundage-related changes - FIXED
            migrationBuilder.AddColumn<decimal>(
                name: "PoundageRate",
                table: "CouncilProperties",
                type: "decimal(10,6)",
                nullable: true);

            // CRITICAL FIX: Make PropertyTypeId nullable initially
            migrationBuilder.AddColumn<int>(
                name: "PropertyTypeId",
                table: "CouncilProperties",
                type: "int",
                nullable: true); // Changed from nullable: false

            // Create PropertyTypes table
            migrationBuilder.CreateTable(
                name: "PropertyTypes",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ShortName = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    SortOrder = table.Column<int>(type: "int", nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PropertyTypes", x => x.Id);
                });

            // Create PoundageRates table
            migrationBuilder.CreateTable(
                name: "PoundageRates",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PropertyTypeId = table.Column<int>(type: "int", nullable: false),
                    ValuationRollId = table.Column<int>(type: "int", nullable: true),
                    OrganisationId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    Rate = table.Column<decimal>(type: "decimal(10,6)", nullable: false),
                    EffectiveFrom = table.Column<DateTime>(type: "datetime2", nullable: false),
                    EffectiveTo = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsCurrent = table.Column<bool>(type: "bit", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ModifiedBy = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    ModifiedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Notes = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PoundageRates", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PoundageRates_CouncilValuationRolls_ValuationRollId",
                        column: x => x.ValuationRollId,
                        principalTable: "CouncilValuationRolls",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_PoundageRates_PropertyTypes_PropertyTypeId",
                        column: x => x.PropertyTypeId,
                        principalTable: "PropertyTypes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            // Create indexes
            migrationBuilder.CreateIndex(
                name: "IX_ServicePayments_ServiceInvoiceId",
                table: "ServicePayments",
                column: "ServiceInvoiceId");

            // ADDED: Missing unique index on PropertyTypes.ShortName
            migrationBuilder.CreateIndex(
                name: "IX_PropertyTypes_ShortName",
                table: "PropertyTypes",
                column: "ShortName",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_PoundageRates_PropertyTypeId",
                table: "PoundageRates",
                column: "PropertyTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_PoundageRates_ValuationRollId",
                table: "PoundageRates",
                column: "ValuationRollId");

            // Invoice foreign key (keeping this)
            migrationBuilder.AddForeignKey(
                name: "FK_ServicePayments_ServiceInvoices_ServiceInvoiceId",
                table: "ServicePayments",
                column: "ServiceInvoiceId",
                principalTable: "ServiceInvoices",
                principalColumn: "Id");

            // REMOVED: Foreign key constraint for PropertyTypeId (will add later after seeding data)
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Remove invoice foreign key
            migrationBuilder.DropForeignKey(
                name: "FK_ServicePayments_ServiceInvoices_ServiceInvoiceId",
                table: "ServicePayments");

            // Drop poundage tables
            migrationBuilder.DropTable(
                name: "PoundageRates");

            migrationBuilder.DropTable(
                name: "PropertyTypes");

            // Remove indexes
            migrationBuilder.DropIndex(
                name: "IX_ServicePayments_ServiceInvoiceId",
                table: "ServicePayments");

            // Remove invoice columns
            migrationBuilder.DropColumn(
                name: "ServiceInvoiceId",
                table: "ServicePayments");

            migrationBuilder.DropColumn(
                name: "OrganisationId",
                table: "ServiceInvoices");

            migrationBuilder.DropColumn(
                name: "ServiceId",
                table: "ServiceInvoices");

            migrationBuilder.DropColumn(
                name: "WorkflowInstanceId",
                table: "ServiceInvoices");

            migrationBuilder.DropColumn(
                name: "WorkflowStepId",
                table: "ServiceInvoices");

            // Remove poundage columns
            migrationBuilder.DropColumn(
                name: "PoundageRate",
                table: "CouncilProperties");

            migrationBuilder.DropColumn(
                name: "PropertyTypeId",
                table: "CouncilProperties");

            // Restore invoice column
            migrationBuilder.AlterColumn<Guid>(
                name: "ServicePaymentId",
                table: "ServiceInvoices",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier",
                oldNullable: true);
        }
    }
}