using Microsoft.EntityFrameworkCore.Migrations;

namespace Timesheet.Migrations
{
    public partial class AddUniqueIndex_UserPunishmentBalance_UserId : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_UserPunishmentBalances_UserId",
                table: "UserPunishmentBalances");

            migrationBuilder.CreateIndex(
                name: "IX_UserPunishmentBalances_UserId",
                table: "UserPunishmentBalances",
                column: "UserId",
                unique: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_UserPunishmentBalances_UserId",
                table: "UserPunishmentBalances");

            migrationBuilder.CreateIndex(
                name: "IX_UserPunishmentBalances_UserId",
                table: "UserPunishmentBalances",
                column: "UserId");
        }
    }
}
