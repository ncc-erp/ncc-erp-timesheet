using Microsoft.EntityFrameworkCore.Migrations;

namespace Timesheet.Migrations
{
    public partial class Add_UserPunishmentPaidId_To_UserPunishment : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<long>(
                name: "UserPunishmentPaidId",
                table: "UserPunishments",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_UserPunishments_UserPunishmentPaidId",
                table: "UserPunishments",
                column: "UserPunishmentPaidId");

            migrationBuilder.AddForeignKey(
                name: "FK_UserPunishments_UserPunishmentPaids_UserPunishmentPaidId",
                table: "UserPunishments",
                column: "UserPunishmentPaidId",
                principalTable: "UserPunishmentPaids",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_UserPunishments_UserPunishmentPaids_UserPunishmentPaidId",
                table: "UserPunishments");

            migrationBuilder.DropIndex(
                name: "IX_UserPunishments_UserPunishmentPaidId",
                table: "UserPunishments");

            migrationBuilder.DropColumn(
                name: "UserPunishmentPaidId",
                table: "UserPunishments");
        }
    }
}
