using Microsoft.EntityFrameworkCore.Migrations;

namespace Timesheet.Migrations
{
    public partial class change_table_ReviewDetails : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ReviewDetails_AbpUsers_ReviewerId",
                table: "ReviewDetails");

            migrationBuilder.AlterColumn<long>(
                name: "ReviewerId",
                table: "ReviewDetails",
                nullable: true,
                oldClrType: typeof(long));

            migrationBuilder.AddColumn<bool>(
                name: "IsFullSalary",
                table: "ReviewDetails",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Salary",
                table: "ReviewDetails",
                nullable: true);

            migrationBuilder.AddColumn<byte>(
                name: "SubLevel",
                table: "ReviewDetails",
                nullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_ReviewDetails_AbpUsers_ReviewerId",
                table: "ReviewDetails",
                column: "ReviewerId",
                principalTable: "AbpUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ReviewDetails_AbpUsers_ReviewerId",
                table: "ReviewDetails");

            migrationBuilder.DropColumn(
                name: "IsFullSalary",
                table: "ReviewDetails");

            migrationBuilder.DropColumn(
                name: "Salary",
                table: "ReviewDetails");

            migrationBuilder.DropColumn(
                name: "SubLevel",
                table: "ReviewDetails");

            migrationBuilder.AlterColumn<long>(
                name: "ReviewerId",
                table: "ReviewDetails",
                nullable: false,
                oldClrType: typeof(long),
                oldNullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_ReviewDetails_AbpUsers_ReviewerId",
                table: "ReviewDetails",
                column: "ReviewerId",
                principalTable: "AbpUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
