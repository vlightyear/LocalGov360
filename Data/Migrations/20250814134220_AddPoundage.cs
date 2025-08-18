using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LocalGov360.Migrations
{
    /// <inheritdoc />
    public partial class AddPoundage : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "ValuationAmount",
                table: "CouncilProperties",
                newName: "Poundage");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Poundage",
                table: "CouncilProperties",
                newName: "ValuationAmount");
        }
    }
}
