using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LocalGov360.Migrations
{
    /// <inheritdoc />
    public partial class servicerevenueclass : Migration
    {
        /// <inheritdoc />

        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Since the original tables were deleted manually, we'll create the new tables from scratch

            // Create CouncilPropertyTypes table
            migrationBuilder.CreateTable(
                name: "CouncilPropertyTypes",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ShortName = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    SortOrder = table.Column<int>(type: "int", nullable: false, defaultValue: 0),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CouncilPropertyTypes", x => x.Id);
                });

            // Create CouncilValuationRoll table
            migrationBuilder.CreateTable(
                name: "CouncilValuationRoll",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    OrganisationId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    Council = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    ValuationDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Year = table.Column<int>(type: "int", nullable: false),
                    RollNumber = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CouncilValuationRoll", x => x.Id);
                });

            // Create CouncilPoundageRates table
            migrationBuilder.CreateTable(
                name: "CouncilPoundageRates",
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
                    IsCurrent = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    CreatedBy = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()"),
                    ModifiedBy = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    ModifiedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Notes = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CouncilPoundageRates", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CouncilPoundageRates_CouncilPropertyTypes_PropertyTypeId",
                        column: x => x.PropertyTypeId,
                        principalTable: "CouncilPropertyTypes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_CouncilPoundageRates_CouncilValuationRoll_ValuationRollId",
                        column: x => x.ValuationRollId,
                        principalTable: "CouncilValuationRoll",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            // Create CouncilProperties table
            migrationBuilder.CreateTable(
                name: "CouncilProperties",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ValuationRollId = table.Column<int>(type: "int", nullable: false),
                    PropertyTypeId = table.Column<int>(type: "int", nullable: false),
                    PropertyNumber = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    StreetAddress = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    Location = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    Leaseholder = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    Use = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false, defaultValue: "RES"),
                    LandExtHa = table.Column<decimal>(type: "decimal(10,4)", nullable: true),
                    LandValue = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    ValueOfImprovements = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    TotalRateableValue = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Remarks = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    Poundage = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    PoundageRate = table.Column<decimal>(type: "decimal(10,6)", nullable: true),
                    EvaluationAmount = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CouncilProperties", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CouncilProperties_CouncilPropertyTypes_PropertyTypeId",
                        column: x => x.PropertyTypeId,
                        principalTable: "CouncilPropertyTypes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_CouncilProperties_CouncilValuationRoll_ValuationRollId",
                        column: x => x.ValuationRollId,
                        principalTable: "CouncilValuationRoll",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            // Create PropertyEvaluations table
            migrationBuilder.CreateTable(
                name: "PropertyEvaluations",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PropertyId = table.Column<int>(type: "int", nullable: false),
                    ValuationRollId = table.Column<int>(type: "int", nullable: false),
                    Year = table.Column<int>(type: "int", nullable: false),
                    Period = table.Column<int>(type: "int", nullable: false),
                    RateableValue = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    PoundageRate = table.Column<decimal>(type: "decimal(10,6)", nullable: false),
                    EvaluationAmount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    AmountPaid = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    Balance = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    PaymentDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsPaid = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    InvoiceNumber = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    InvoiceDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedBy = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()"),
                    ModifiedBy = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    ModifiedDate = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PropertyEvaluations", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PropertyEvaluations_CouncilProperties_PropertyId",
                        column: x => x.PropertyId,
                        principalTable: "CouncilProperties",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_PropertyEvaluations_CouncilValuationRoll_ValuationRollId",
                        column: x => x.ValuationRollId,
                        principalTable: "CouncilValuationRoll",
                        principalColumn: "Id");
                });

            // Create ServiceRevenueClasses table
            migrationBuilder.CreateTable(
                name: "ServiceRevenueClasses",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    SystemCode = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Code = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    ParentCode = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: true),
                    SpecialCode = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    OrganisationId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Bills = table.Column<bool>(type: "bit", nullable: false),
                    SelfService = table.Column<bool>(type: "bit", nullable: false),
                    ReqUploads = table.Column<bool>(type: "bit", nullable: false),
                    BudgetLine = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Status = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ServiceRevenueClasses", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ServiceRevenueClasses_Organisations_OrganisationId",
                        column: x => x.OrganisationId,
                        principalTable: "Organisations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            // Create indexes
            migrationBuilder.CreateIndex(
                name: "IX_CouncilPropertyTypes_ShortName",
                table: "CouncilPropertyTypes",
                column: "ShortName");

            migrationBuilder.CreateIndex(
                name: "IX_CouncilValuationRoll_Council_Year",
                table: "CouncilValuationRoll",
                columns: new[] { "Council", "Year" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_CouncilPoundageRates_PropertyTypeId",
                table: "CouncilPoundageRates",
                column: "PropertyTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_CouncilPoundageRates_ValuationRollId",
                table: "CouncilPoundageRates",
                column: "ValuationRollId");

            migrationBuilder.CreateIndex(
                name: "IX_CouncilProperties_ValuationRollId_PropertyNumber",
                table: "CouncilProperties",
                columns: new[] { "ValuationRollId", "PropertyNumber" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_PropertyEvaluations_PropertyId_Year_Period",
                table: "PropertyEvaluations",
                columns: new[] { "PropertyId", "Year", "Period" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_PropertyEvaluations_ValuationRollId",
                table: "PropertyEvaluations",
                column: "ValuationRollId");

            migrationBuilder.CreateIndex(
                name: "IX_ServiceRevenueClasses_OrganisationId",
                table: "ServiceRevenueClasses",
                column: "OrganisationId");
        }
        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_CouncilPoundageRates_CouncilPropertyTypes_PropertyTypeId",
                table: "CouncilPoundageRates");

            migrationBuilder.DropForeignKey(
                name: "FK_CouncilPoundageRates_CouncilValuationRoll_ValuationRollId",
                table: "CouncilPoundageRates");

            migrationBuilder.DropForeignKey(
                name: "FK_CouncilProperties_CouncilPropertyTypes_PropertyTypeId",
                table: "CouncilProperties");

            migrationBuilder.DropForeignKey(
                name: "FK_CouncilProperties_CouncilValuationRoll_ValuationRollId",
                table: "CouncilProperties");

            migrationBuilder.DropTable(
                name: "PropertyEvaluations");

            migrationBuilder.DropTable(
                name: "ServiceRevenueClasses");

            migrationBuilder.DropIndex(
                name: "IX_CouncilProperties_ValuationRollId_PropertyNumber",
                table: "CouncilProperties");

            migrationBuilder.DropPrimaryKey(
                name: "PK_CouncilValuationRoll",
                table: "CouncilValuationRoll");

            migrationBuilder.DropIndex(
                name: "IX_CouncilValuationRoll_Council_Year",
                table: "CouncilValuationRoll");

            migrationBuilder.DropPrimaryKey(
                name: "PK_CouncilPropertyTypes",
                table: "CouncilPropertyTypes");

            migrationBuilder.DropPrimaryKey(
                name: "PK_CouncilPoundageRates",
                table: "CouncilPoundageRates");

            migrationBuilder.DropIndex(
                name: "IX_CouncilPoundageRates_PropertyTypeId",
                table: "CouncilPoundageRates");

            migrationBuilder.DropColumn(
                name: "EvaluationAmount",
                table: "CouncilProperties");

            migrationBuilder.RenameTable(
                name: "CouncilValuationRoll",
                newName: "CouncilValuationRolls");

            migrationBuilder.RenameTable(
                name: "CouncilPropertyTypes",
                newName: "PropertyTypes");

            migrationBuilder.RenameTable(
                name: "CouncilPoundageRates",
                newName: "PoundageRates");

            migrationBuilder.RenameIndex(
                name: "IX_CouncilPropertyTypes_ShortName",
                table: "PropertyTypes",
                newName: "IX_PropertyTypes_ShortName");

            migrationBuilder.RenameIndex(
                name: "IX_CouncilPoundageRates_ValuationRollId",
                table: "PoundageRates",
                newName: "IX_PoundageRates_ValuationRollId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_CouncilValuationRolls",
                table: "CouncilValuationRolls",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_PropertyTypes",
                table: "PropertyTypes",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_PoundageRates",
                table: "PoundageRates",
                column: "Id");

            migrationBuilder.CreateIndex(
                name: "IX_CouncilProperties_Leaseholder",
                table: "CouncilProperties",
                column: "Leaseholder");

            migrationBuilder.CreateIndex(
                name: "IX_CouncilProperties_PropertyNumber",
                table: "CouncilProperties",
                column: "PropertyNumber");

            migrationBuilder.CreateIndex(
                name: "IX_CouncilProperties_Use",
                table: "CouncilProperties",
                column: "Use");

            migrationBuilder.CreateIndex(
                name: "IX_CouncilProperties_ValuationRollId",
                table: "CouncilProperties",
                column: "ValuationRollId");

            migrationBuilder.CreateIndex(
                name: "IX_CouncilProperties_ValuationRollId_PropertyTypeId",
                table: "CouncilProperties",
                columns: new[] { "ValuationRollId", "PropertyTypeId" });

            migrationBuilder.CreateIndex(
                name: "IX_CouncilValuationRolls_OrganisationId",
                table: "CouncilValuationRolls",
                column: "OrganisationId");

            migrationBuilder.CreateIndex(
                name: "IX_CouncilValuationRolls_OrganisationId_Year",
                table: "CouncilValuationRolls",
                columns: new[] { "OrganisationId", "Year" });

            migrationBuilder.CreateIndex(
                name: "IX_CouncilValuationRolls_Year",
                table: "CouncilValuationRolls",
                column: "Year");

            migrationBuilder.CreateIndex(
                name: "IX_PoundageRates_EffectiveFrom",
                table: "PoundageRates",
                column: "EffectiveFrom");

            migrationBuilder.CreateIndex(
                name: "IX_PoundageRates_PropertyTypeId_IsCurrent",
                table: "PoundageRates",
                columns: new[] { "PropertyTypeId", "IsCurrent" });

            migrationBuilder.AddForeignKey(
                name: "FK_CouncilProperties_CouncilValuationRolls_ValuationRollId",
                table: "CouncilProperties",
                column: "ValuationRollId",
                principalTable: "CouncilValuationRolls",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_CouncilProperties_PropertyTypes_PropertyTypeId",
                table: "CouncilProperties",
                column: "PropertyTypeId",
                principalTable: "PropertyTypes",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_PoundageRates_CouncilValuationRolls_ValuationRollId",
                table: "PoundageRates",
                column: "ValuationRollId",
                principalTable: "CouncilValuationRolls",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_PoundageRates_PropertyTypes_PropertyTypeId",
                table: "PoundageRates",
                column: "PropertyTypeId",
                principalTable: "PropertyTypes",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
