using Microsoft.EntityFrameworkCore.Migrations;

namespace Timesheet.Migrations
{
    public partial class changeTableTimeKeepings : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "RegistrationTimeEnd",
                table: "Timekeepings");

            migrationBuilder.DropColumn(
                name: "UserName",
                table: "Timekeepings");

            migrationBuilder.RenameColumn(
                name: "UserCode",
                table: "Timekeepings",
                newName: "RegisterCheckOut");

            migrationBuilder.RenameColumn(
                name: "RegistrationTimeStart",
                table: "Timekeepings",
                newName: "RegisterCheckIn");

            migrationBuilder.RenameColumn(
                name: "Date",
                table: "Timekeepings",
                newName: "DateAt");

            migrationBuilder.AlterColumn<string>(
                name: "UserEmail",
                table: "Timekeepings",
                nullable: true,
                oldClrType: typeof(string),
                oldMaxLength: 256,
                oldNullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "RegisterCheckOut",
                table: "Timekeepings",
                newName: "UserCode");

            migrationBuilder.RenameColumn(
                name: "RegisterCheckIn",
                table: "Timekeepings",
                newName: "RegistrationTimeStart");

            migrationBuilder.RenameColumn(
                name: "DateAt",
                table: "Timekeepings",
                newName: "Date");

            migrationBuilder.AlterColumn<string>(
                name: "UserEmail",
                table: "Timekeepings",
                maxLength: 256,
                nullable: true,
                oldClrType: typeof(string),
                oldNullable: true);

            migrationBuilder.AddColumn<string>(
                name: "RegistrationTimeEnd",
                table: "Timekeepings",
                maxLength: 5,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "UserName",
                table: "Timekeepings",
                maxLength: 256,
                nullable: true);
        }
    }
}
