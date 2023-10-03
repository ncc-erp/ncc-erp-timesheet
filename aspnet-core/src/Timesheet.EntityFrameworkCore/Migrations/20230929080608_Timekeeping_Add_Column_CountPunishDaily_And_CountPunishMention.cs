using Microsoft.EntityFrameworkCore.Migrations;

namespace Timesheet.Migrations
{
    public partial class Timekeeping_Add_Column_CountPunishDaily_And_CountPunishMention : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "CountPunishDaily",
                table: "Timekeepings",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "CountPunishMention",
                table: "Timekeepings",
                nullable: false,
                defaultValue: 0);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CountPunishDaily",
                table: "Timekeepings");

            migrationBuilder.DropColumn(
                name: "CountPunishMention",
                table: "Timekeepings");
        }
    }
}
