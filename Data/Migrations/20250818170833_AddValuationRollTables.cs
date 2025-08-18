using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace LocalGov360.Migrations
{
    /// <inheritdoc />
    public partial class AddValuationRollTables : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_CouncilPoundageRates_CouncilPropertyTypes_PropertyTypeId",
                table: "CouncilPoundageRates");

            migrationBuilder.DropForeignKey(
                name: "FK_CouncilPoundageRates_CouncilValuationRoll_ValuationRollId",
                table: "CouncilPoundageRates");

            migrationBuilder.DropForeignKey(
                name: "FK_CouncilProperties_CouncilValuationRoll_ValuationRollId",
                table: "CouncilProperties");

            migrationBuilder.DropForeignKey(
                name: "FK_PropertyEvaluations_CouncilProperties_PropertyId",
                table: "PropertyEvaluations");

            migrationBuilder.DropForeignKey(
                name: "FK_PropertyEvaluations_CouncilValuationRoll_ValuationRollId",
                table: "PropertyEvaluations");

            migrationBuilder.DropIndex(
                name: "IX_CouncilPoundageRates_PropertyTypeId",
                table: "CouncilPoundageRates");

            migrationBuilder.DropPrimaryKey(
                name: "PK_PropertyEvaluations",
                table: "PropertyEvaluations");

            migrationBuilder.DropPrimaryKey(
                name: "PK_CouncilValuationRoll",
                table: "CouncilValuationRoll");

            migrationBuilder.DropIndex(
                name: "IX_CouncilValuationRoll_Council_Year",
                table: "CouncilValuationRoll");

            migrationBuilder.RenameTable(
                name: "PropertyEvaluations",
                newName: "CouncilPropertyEvaluations");

            migrationBuilder.RenameTable(
                name: "CouncilValuationRoll",
                newName: "CouncilValuationRolls");

            migrationBuilder.RenameIndex(
                name: "IX_PropertyEvaluations_ValuationRollId",
                table: "CouncilPropertyEvaluations",
                newName: "IX_CouncilPropertyEvaluations_ValuationRollId");

            migrationBuilder.RenameIndex(
                name: "IX_PropertyEvaluations_PropertyId_Year_Period",
                table: "CouncilPropertyEvaluations",
                newName: "IX_CouncilPropertyEvaluations_PropertyId_Year_Period");

            migrationBuilder.AlterColumn<bool>(
                name: "IsActive",
                table: "CouncilPropertyTypes",
                type: "bit",
                nullable: false,
                defaultValue: true,
                oldClrType: typeof(bool),
                oldType: "bit");

            migrationBuilder.AlterColumn<DateTime>(
                name: "CreatedDate",
                table: "CouncilPropertyTypes",
                type: "datetime2",
                nullable: false,
                defaultValueSql: "GETUTCDATE()",
                oldClrType: typeof(DateTime),
                oldType: "datetime2");

            migrationBuilder.AlterColumn<DateTime>(
                name: "CreatedDate",
                table: "CouncilProperties",
                type: "datetime2",
                nullable: false,
                defaultValueSql: "GETUTCDATE()",
                oldClrType: typeof(DateTime),
                oldType: "datetime2");

            migrationBuilder.AlterColumn<bool>(
                name: "IsCurrent",
                table: "CouncilPoundageRates",
                type: "bit",
                nullable: false,
                defaultValue: true,
                oldClrType: typeof(bool),
                oldType: "bit");

            migrationBuilder.AlterColumn<DateTime>(
                name: "CreatedDate",
                table: "CouncilPoundageRates",
                type: "datetime2",
                nullable: false,
                defaultValueSql: "GETUTCDATE()",
                oldClrType: typeof(DateTime),
                oldType: "datetime2");

            migrationBuilder.AlterColumn<bool>(
                name: "IsPaid",
                table: "CouncilPropertyEvaluations",
                type: "bit",
                nullable: false,
                defaultValue: false,
                oldClrType: typeof(bool),
                oldType: "bit");

            migrationBuilder.AlterColumn<DateTime>(
                name: "CreatedDate",
                table: "CouncilPropertyEvaluations",
                type: "datetime2",
                nullable: false,
                defaultValueSql: "GETUTCDATE()",
                oldClrType: typeof(DateTime),
                oldType: "datetime2");

            migrationBuilder.AlterColumn<DateTime>(
                name: "CreatedDate",
                table: "CouncilValuationRolls",
                type: "datetime2",
                nullable: false,
                defaultValueSql: "GETUTCDATE()",
                oldClrType: typeof(DateTime),
                oldType: "datetime2");

            migrationBuilder.AddPrimaryKey(
                name: "PK_CouncilPropertyEvaluations",
                table: "CouncilPropertyEvaluations",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_CouncilValuationRolls",
                table: "CouncilValuationRolls",
                column: "Id");

            migrationBuilder.InsertData(
                table: "CouncilPropertyTypes",
                columns: new[] { "Id", "CreatedDate", "Description", "IsActive", "Name", "ShortName", "SortOrder" },
                values: new object[,]
                {
                    { 1, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, true, "Residential", "RES", 1 },
                    { 2, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, true, "Commercial", "COM", 2 },
                    { 3, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, true, "Industrial", "IND", 3 },
                    { 4, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, true, "Hospitality", "HOS", 4 },
                    { 5, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, true, "Institutional", "INS", 5 },
                    { 6, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, true, "Power Transmission", "PWR", 6 },
                    { 7, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, true, "Properties Owned by Zambia Airport Corporation Limited", "APT", 7 }
                });

            migrationBuilder.InsertData(
                table: "CouncilPoundageRates",
                columns: new[] { "Id", "CreatedBy", "CreatedDate", "EffectiveFrom", "EffectiveTo", "IsCurrent", "ModifiedBy", "ModifiedDate", "Notes", "OrganisationId", "PropertyTypeId", "Rate", "ValuationRollId" },
                values: new object[,]
                {
                    { 1, null, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, true, null, null, "Approved rate for Residential", null, 1, 0.001m, null },
                    { 2, null, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, true, null, null, "Approved rate for Commercial", null, 2, 0.002m, null },
                    { 3, null, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, true, null, null, "Approved rate for Industrial", null, 3, 0.002m, null },
                    { 4, null, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, true, null, null, "Approved rate for Hospitality", null, 4, 0.002m, null },
                    { 5, null, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, true, null, null, "Approved rate for Institutional", null, 5, 0.002m, null },
                    { 6, null, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, true, null, null, "Approved rate for Power Transmission", null, 6, 0.0015m, null },
                    { 7, null, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, true, null, null, "Approved rate for Airport", null, 7, 0.0015m, null }
                });

            migrationBuilder.CreateIndex(
                name: "IX_CouncilPropertyTypes_IsActive",
                table: "CouncilPropertyTypes",
                column: "IsActive");

            migrationBuilder.CreateIndex(
                name: "IX_CouncilProperties_PropertyNumber",
                table: "CouncilProperties",
                column: "PropertyNumber");

            migrationBuilder.CreateIndex(
                name: "IX_CouncilProperties_TotalRateableValue",
                table: "CouncilProperties",
                column: "TotalRateableValue");

            migrationBuilder.CreateIndex(
                name: "IX_CouncilProperties_Use",
                table: "CouncilProperties",
                column: "Use");

            migrationBuilder.CreateIndex(
                name: "IX_CouncilPoundageRates_IsCurrent",
                table: "CouncilPoundageRates",
                column: "IsCurrent");

            migrationBuilder.CreateIndex(
                name: "IX_CouncilPoundageRates_PropertyTypeId_EffectiveFrom",
                table: "CouncilPoundageRates",
                columns: new[] { "PropertyTypeId", "EffectiveFrom" });

            migrationBuilder.CreateIndex(
                name: "IX_CouncilPropertyEvaluations_InvoiceNumber",
                table: "CouncilPropertyEvaluations",
                column: "InvoiceNumber");

            migrationBuilder.CreateIndex(
                name: "IX_CouncilPropertyEvaluations_IsPaid",
                table: "CouncilPropertyEvaluations",
                column: "IsPaid");

            migrationBuilder.CreateIndex(
                name: "IX_CouncilPropertyEvaluations_Year_Period",
                table: "CouncilPropertyEvaluations",
                columns: new[] { "Year", "Period" });

            migrationBuilder.CreateIndex(
                name: "IX_CouncilValuationRolls_Council_Year",
                table: "CouncilValuationRolls",
                columns: new[] { "Council", "Year" });

            migrationBuilder.CreateIndex(
                name: "IX_CouncilValuationRolls_RollNumber",
                table: "CouncilValuationRolls",
                column: "RollNumber");

            migrationBuilder.AddForeignKey(
                name: "FK_CouncilPoundageRates_CouncilPropertyTypes_PropertyTypeId",
                table: "CouncilPoundageRates",
                column: "PropertyTypeId",
                principalTable: "CouncilPropertyTypes",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_CouncilPoundageRates_CouncilValuationRolls_ValuationRollId",
                table: "CouncilPoundageRates",
                column: "ValuationRollId",
                principalTable: "CouncilValuationRolls",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_CouncilProperties_CouncilValuationRolls_ValuationRollId",
                table: "CouncilProperties",
                column: "ValuationRollId",
                principalTable: "CouncilValuationRolls",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_CouncilPropertyEvaluations_CouncilProperties_PropertyId",
                table: "CouncilPropertyEvaluations",
                column: "PropertyId",
                principalTable: "CouncilProperties",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_CouncilPropertyEvaluations_CouncilValuationRolls_ValuationRollId",
                table: "CouncilPropertyEvaluations",
                column: "ValuationRollId",
                principalTable: "CouncilValuationRolls",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_CouncilPoundageRates_CouncilPropertyTypes_PropertyTypeId",
                table: "CouncilPoundageRates");

            migrationBuilder.DropForeignKey(
                name: "FK_CouncilPoundageRates_CouncilValuationRolls_ValuationRollId",
                table: "CouncilPoundageRates");

            migrationBuilder.DropForeignKey(
                name: "FK_CouncilProperties_CouncilValuationRolls_ValuationRollId",
                table: "CouncilProperties");

            migrationBuilder.DropForeignKey(
                name: "FK_CouncilPropertyEvaluations_CouncilProperties_PropertyId",
                table: "CouncilPropertyEvaluations");

            migrationBuilder.DropForeignKey(
                name: "FK_CouncilPropertyEvaluations_CouncilValuationRolls_ValuationRollId",
                table: "CouncilPropertyEvaluations");

            migrationBuilder.DropIndex(
                name: "IX_CouncilPropertyTypes_IsActive",
                table: "CouncilPropertyTypes");

            migrationBuilder.DropIndex(
                name: "IX_CouncilProperties_PropertyNumber",
                table: "CouncilProperties");

            migrationBuilder.DropIndex(
                name: "IX_CouncilProperties_TotalRateableValue",
                table: "CouncilProperties");

            migrationBuilder.DropIndex(
                name: "IX_CouncilProperties_Use",
                table: "CouncilProperties");

            migrationBuilder.DropIndex(
                name: "IX_CouncilPoundageRates_IsCurrent",
                table: "CouncilPoundageRates");

            migrationBuilder.DropIndex(
                name: "IX_CouncilPoundageRates_PropertyTypeId_EffectiveFrom",
                table: "CouncilPoundageRates");

            migrationBuilder.DropPrimaryKey(
                name: "PK_CouncilValuationRolls",
                table: "CouncilValuationRolls");

            migrationBuilder.DropIndex(
                name: "IX_CouncilValuationRolls_Council_Year",
                table: "CouncilValuationRolls");

            migrationBuilder.DropIndex(
                name: "IX_CouncilValuationRolls_RollNumber",
                table: "CouncilValuationRolls");

            migrationBuilder.DropPrimaryKey(
                name: "PK_CouncilPropertyEvaluations",
                table: "CouncilPropertyEvaluations");

            migrationBuilder.DropIndex(
                name: "IX_CouncilPropertyEvaluations_InvoiceNumber",
                table: "CouncilPropertyEvaluations");

            migrationBuilder.DropIndex(
                name: "IX_CouncilPropertyEvaluations_IsPaid",
                table: "CouncilPropertyEvaluations");

            migrationBuilder.DropIndex(
                name: "IX_CouncilPropertyEvaluations_Year_Period",
                table: "CouncilPropertyEvaluations");

            migrationBuilder.DeleteData(
                table: "CouncilPoundageRates",
                keyColumn: "Id",
                keyValue: 1);

            migrationBuilder.DeleteData(
                table: "CouncilPoundageRates",
                keyColumn: "Id",
                keyValue: 2);

            migrationBuilder.DeleteData(
                table: "CouncilPoundageRates",
                keyColumn: "Id",
                keyValue: 3);

            migrationBuilder.DeleteData(
                table: "CouncilPoundageRates",
                keyColumn: "Id",
                keyValue: 4);

            migrationBuilder.DeleteData(
                table: "CouncilPoundageRates",
                keyColumn: "Id",
                keyValue: 5);

            migrationBuilder.DeleteData(
                table: "CouncilPoundageRates",
                keyColumn: "Id",
                keyValue: 6);

            migrationBuilder.DeleteData(
                table: "CouncilPoundageRates",
                keyColumn: "Id",
                keyValue: 7);

            migrationBuilder.DeleteData(
                table: "CouncilPropertyTypes",
                keyColumn: "Id",
                keyValue: 1);

            migrationBuilder.DeleteData(
                table: "CouncilPropertyTypes",
                keyColumn: "Id",
                keyValue: 2);

            migrationBuilder.DeleteData(
                table: "CouncilPropertyTypes",
                keyColumn: "Id",
                keyValue: 3);

            migrationBuilder.DeleteData(
                table: "CouncilPropertyTypes",
                keyColumn: "Id",
                keyValue: 4);

            migrationBuilder.DeleteData(
                table: "CouncilPropertyTypes",
                keyColumn: "Id",
                keyValue: 5);

            migrationBuilder.DeleteData(
                table: "CouncilPropertyTypes",
                keyColumn: "Id",
                keyValue: 6);

            migrationBuilder.DeleteData(
                table: "CouncilPropertyTypes",
                keyColumn: "Id",
                keyValue: 7);

            migrationBuilder.RenameTable(
                name: "CouncilValuationRolls",
                newName: "CouncilValuationRoll");

            migrationBuilder.RenameTable(
                name: "CouncilPropertyEvaluations",
                newName: "PropertyEvaluations");

            migrationBuilder.RenameIndex(
                name: "IX_CouncilPropertyEvaluations_ValuationRollId",
                table: "PropertyEvaluations",
                newName: "IX_PropertyEvaluations_ValuationRollId");

            migrationBuilder.RenameIndex(
                name: "IX_CouncilPropertyEvaluations_PropertyId_Year_Period",
                table: "PropertyEvaluations",
                newName: "IX_PropertyEvaluations_PropertyId_Year_Period");

            migrationBuilder.AlterColumn<bool>(
                name: "IsActive",
                table: "CouncilPropertyTypes",
                type: "bit",
                nullable: false,
                oldClrType: typeof(bool),
                oldType: "bit",
                oldDefaultValue: true);

            migrationBuilder.AlterColumn<DateTime>(
                name: "CreatedDate",
                table: "CouncilPropertyTypes",
                type: "datetime2",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "datetime2",
                oldDefaultValueSql: "GETUTCDATE()");

            migrationBuilder.AlterColumn<DateTime>(
                name: "CreatedDate",
                table: "CouncilProperties",
                type: "datetime2",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "datetime2",
                oldDefaultValueSql: "GETUTCDATE()");

            migrationBuilder.AlterColumn<bool>(
                name: "IsCurrent",
                table: "CouncilPoundageRates",
                type: "bit",
                nullable: false,
                oldClrType: typeof(bool),
                oldType: "bit",
                oldDefaultValue: true);

            migrationBuilder.AlterColumn<DateTime>(
                name: "CreatedDate",
                table: "CouncilPoundageRates",
                type: "datetime2",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "datetime2",
                oldDefaultValueSql: "GETUTCDATE()");

            migrationBuilder.AlterColumn<DateTime>(
                name: "CreatedDate",
                table: "CouncilValuationRoll",
                type: "datetime2",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "datetime2",
                oldDefaultValueSql: "GETUTCDATE()");

            migrationBuilder.AlterColumn<bool>(
                name: "IsPaid",
                table: "PropertyEvaluations",
                type: "bit",
                nullable: false,
                oldClrType: typeof(bool),
                oldType: "bit",
                oldDefaultValue: false);

            migrationBuilder.AlterColumn<DateTime>(
                name: "CreatedDate",
                table: "PropertyEvaluations",
                type: "datetime2",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "datetime2",
                oldDefaultValueSql: "GETUTCDATE()");

            migrationBuilder.AddPrimaryKey(
                name: "PK_CouncilValuationRoll",
                table: "CouncilValuationRoll",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_PropertyEvaluations",
                table: "PropertyEvaluations",
                column: "Id");

            migrationBuilder.CreateIndex(
                name: "IX_CouncilPoundageRates_PropertyTypeId",
                table: "CouncilPoundageRates",
                column: "PropertyTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_CouncilValuationRoll_Council_Year",
                table: "CouncilValuationRoll",
                columns: new[] { "Council", "Year" },
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_CouncilPoundageRates_CouncilPropertyTypes_PropertyTypeId",
                table: "CouncilPoundageRates",
                column: "PropertyTypeId",
                principalTable: "CouncilPropertyTypes",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_CouncilPoundageRates_CouncilValuationRoll_ValuationRollId",
                table: "CouncilPoundageRates",
                column: "ValuationRollId",
                principalTable: "CouncilValuationRoll",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_CouncilProperties_CouncilValuationRoll_ValuationRollId",
                table: "CouncilProperties",
                column: "ValuationRollId",
                principalTable: "CouncilValuationRoll",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_PropertyEvaluations_CouncilProperties_PropertyId",
                table: "PropertyEvaluations",
                column: "PropertyId",
                principalTable: "CouncilProperties",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_PropertyEvaluations_CouncilValuationRoll_ValuationRollId",
                table: "PropertyEvaluations",
                column: "ValuationRollId",
                principalTable: "CouncilValuationRoll",
                principalColumn: "Id");
        }
    }
}
