using Microsoft.EntityFrameworkCore.Migrations;

namespace Timesheet.Migrations
{
    public partial class Timekeeping_Add_Column_MoneyPunish_TrackerTime_StatusPunish : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "MoneyPunish",
                table: "Timekeepings",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "StatusPunish",
                table: "Timekeepings",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "TrackerTime",
                table: "Timekeepings",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "MoneyPunish",
                table: "Timekeepings");

            migrationBuilder.DropColumn(
                name: "StatusPunish",
                table: "Timekeepings");

            migrationBuilder.DropColumn(
                name: "TrackerTime",
                table: "Timekeepings");
        }
    }
}
