using Microsoft.EntityFrameworkCore.Migrations;

namespace Timesheet.Migrations
{
    public partial class add_mezonuser_id_to_user_table : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "MezonId",
                table: "AbpUsers",
                newName: "MezonUserId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "MezonUserId",
                table: "AbpUsers",
                newName: "MezonId");
        }
    }
}
