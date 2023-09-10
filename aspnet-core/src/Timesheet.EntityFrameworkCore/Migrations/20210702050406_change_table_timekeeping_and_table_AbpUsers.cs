using Microsoft.EntityFrameworkCore.Migrations;

namespace Timesheet.Migrations
{
    public partial class change_table_timekeeping_and_table_AbpUsers : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "NoteReply",
                table: "Timekeepings",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "UserNote",
                table: "Timekeepings",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsStopWork",
                table: "AbpUsers",
                nullable: false,
                defaultValue: false);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "NoteReply",
                table: "Timekeepings");

            migrationBuilder.DropColumn(
                name: "UserNote",
                table: "Timekeepings");

            migrationBuilder.DropColumn(
                name: "IsStopWork",
                table: "AbpUsers");
        }
    }
}
