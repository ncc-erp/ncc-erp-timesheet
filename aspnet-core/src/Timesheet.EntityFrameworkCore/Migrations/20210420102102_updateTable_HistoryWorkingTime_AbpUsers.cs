using Microsoft.EntityFrameworkCore.Migrations;

namespace Timesheet.Migrations
{
    public partial class updateTable_HistoryWorkingTime_AbpUsers : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<double>(
                name: "MorningWorkingTime",
                table: "HistoryWorkingTimes",
                maxLength: 5,
                nullable: false,
                oldClrType: typeof(string),
                oldMaxLength: 5,
                oldNullable: true);

            migrationBuilder.AlterColumn<double>(
                name: "AfternoonWorkingTime",
                table: "HistoryWorkingTimes",
                maxLength: 5,
                nullable: false,
                oldClrType: typeof(string),
                oldMaxLength: 5,
                oldNullable: true);

            migrationBuilder.AlterColumn<double>(
                name: "MorningWorking",
                table: "AbpUsers",
                maxLength: 5,
                nullable: true,
                oldClrType: typeof(string),
                oldMaxLength: 5,
                oldNullable: true);

            migrationBuilder.AlterColumn<double>(
                name: "AfternoonWorking",
                table: "AbpUsers",
                maxLength: 5,
                nullable: true,
                oldClrType: typeof(string),
                oldMaxLength: 5,
                oldNullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "MorningWorkingTime",
                table: "HistoryWorkingTimes",
                maxLength: 5,
                nullable: true,
                oldClrType: typeof(double),
                oldMaxLength: 5);

            migrationBuilder.AlterColumn<string>(
                name: "AfternoonWorkingTime",
                table: "HistoryWorkingTimes",
                maxLength: 5,
                nullable: true,
                oldClrType: typeof(double),
                oldMaxLength: 5);

            migrationBuilder.AlterColumn<string>(
                name: "MorningWorking",
                table: "AbpUsers",
                maxLength: 5,
                nullable: true,
                oldClrType: typeof(double),
                oldMaxLength: 5,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "AfternoonWorking",
                table: "AbpUsers",
                maxLength: 5,
                nullable: true,
                oldClrType: typeof(double),
                oldMaxLength: 5,
                oldNullable: true);
        }
    }
}
