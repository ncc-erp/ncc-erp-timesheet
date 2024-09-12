using Microsoft.EntityFrameworkCore.Migrations;

namespace Timesheet.Migrations
{
    public partial class add_notifyChannel_project : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "mezonUrl",
                table: "Projects",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "notifyChannel",
                table: "Projects",
                nullable: false,
                defaultValue: 0);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "mezonUrl",
                table: "Projects");

            migrationBuilder.DropColumn(
                name: "notifyChannel",
                table: "Projects");
        }
    }
}
