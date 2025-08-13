using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LocalGov360.Migrations
{
    public partial class AddServiceIdToWorkflowInstance : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Everything already exists in the database — do nothing
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // No rollback needed since Up did nothing
        }
    }
}
