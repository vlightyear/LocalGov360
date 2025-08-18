using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LocalGov360.Migrations
{
    /// <inheritdoc />
    public partial class AddAccountingStatusOnly : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Only add AccountingStatus columns - remove all other operations
            migrationBuilder.AddColumn<string>(
                name: "AccountingStatus",
                table: "ServiceInvoices",
                type: "nvarchar(50)",
                nullable: true,
                defaultValue: "Pending");

     
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AccountingStatus",
                table: "ServiceInvoices");

         
        }
    }
}