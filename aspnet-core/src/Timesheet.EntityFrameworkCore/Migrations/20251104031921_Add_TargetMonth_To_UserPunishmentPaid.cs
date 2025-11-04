using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Timesheet.Migrations
{
    public partial class Add_TargetMonth_To_UserPunishmentPaid : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "TargetMonth",
                table: "UserPunishmentPaids",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.Sql(@"
                UPDATE UserPunishmentPaids 
                SET TargetMonth = DATEFROMPARTS(YEAR(DateAt), MONTH(DateAt), 1)
                WHERE TargetMonth = '0001-01-01 00:00:00.0000000' -- Default datetime value
            ");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "TargetMonth",
                table: "UserPunishmentPaids");
        }
    }
}
