using Microsoft.EntityFrameworkCore.Migrations;

namespace Timesheet.Migrations
{
    public partial class Update_Database_Add_Column_KomuUserId_KomuChannelId_and_IsNotifyToKomu : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsNotifyToKomu",
                table: "Projects",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "KomuChannelId",
                table: "Projects",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "KomuUserId",
                table: "AbpUsers",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsNotifyToKomu",
                table: "Projects");

            migrationBuilder.DropColumn(
                name: "KomuChannelId",
                table: "Projects");

            migrationBuilder.DropColumn(
                name: "KomuUserId",
                table: "AbpUsers");
        }
    }
}
