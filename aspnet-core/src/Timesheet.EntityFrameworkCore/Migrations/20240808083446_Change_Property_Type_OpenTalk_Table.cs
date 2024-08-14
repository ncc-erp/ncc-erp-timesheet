using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Timesheet.Migrations
{
    public partial class Change_Property_Type_OpenTalk_Table : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "endTime",
                table: "OpenTalk");

            migrationBuilder.RenameColumn(
                name: "startTime",
                table: "OpenTalk",
                newName: "DateAt");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "DateAt",
                table: "OpenTalk",
                newName: "startTime");

            migrationBuilder.AddColumn<DateTime>(
                name: "endTime",
                table: "OpenTalk",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));
        }
    }
}
