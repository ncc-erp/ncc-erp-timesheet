using Microsoft.EntityFrameworkCore.Migrations;

namespace Timesheet.Migrations
{
    public partial class Add_WorkMode_To_User_And_HistoryWorkingTime : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "WorkMode",
                table: "HistoryWorkingTimes",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "WorkMode",
                table: "AbpUsers",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "WorkMode",
                table: "HistoryWorkingTimes");

            migrationBuilder.DropColumn(
                name: "WorkMode",
                table: "AbpUsers");
        }
    }
}
