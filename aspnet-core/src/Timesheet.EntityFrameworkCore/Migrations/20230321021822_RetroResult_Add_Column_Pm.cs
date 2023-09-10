using Microsoft.EntityFrameworkCore.Migrations;

namespace Timesheet.Migrations
{
    public partial class RetroResult_Add_Column_Pm : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<long>(
                name: "PmId",
                table: "RetroResults",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_RetroResults_PmId",
                table: "RetroResults",
                column: "PmId");

            migrationBuilder.AddForeignKey(
                name: "FK_RetroResults_AbpUsers_PmId",
                table: "RetroResults",
                column: "PmId",
                principalTable: "AbpUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_RetroResults_AbpUsers_PmId",
                table: "RetroResults");

            migrationBuilder.DropIndex(
                name: "IX_RetroResults_PmId",
                table: "RetroResults");

            migrationBuilder.DropColumn(
                name: "PmId",
                table: "RetroResults");
        }
    }
}
