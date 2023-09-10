using Microsoft.EntityFrameworkCore.Migrations;

namespace Timesheet.Migrations
{
    public partial class update_table_TimeKeeping : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsLocked",
                table: "Timekeepings",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsPunishedCheckIn",
                table: "Timekeepings",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsPunishedCheckOut",
                table: "Timekeepings",
                nullable: false,
                defaultValue: false);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsLocked",
                table: "Timekeepings");

            migrationBuilder.DropColumn(
                name: "IsPunishedCheckIn",
                table: "Timekeepings");

            migrationBuilder.DropColumn(
                name: "IsPunishedCheckOut",
                table: "Timekeepings");
        }
    }
}
