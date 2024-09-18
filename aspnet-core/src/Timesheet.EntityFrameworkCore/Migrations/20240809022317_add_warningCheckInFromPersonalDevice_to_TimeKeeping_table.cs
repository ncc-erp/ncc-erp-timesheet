using Microsoft.EntityFrameworkCore.Migrations;

namespace Timesheet.Migrations
{
    public partial class add_warningCheckInFromPersonalDevice_to_TimeKeeping_table : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "warningCheckInFromPersonalDevice",
                table: "Timekeepings",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "warningCheckInFromPersonalDevice",
                table: "Timekeepings");
        }
    }
}
