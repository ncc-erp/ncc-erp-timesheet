using System;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Timesheet.Migrations
{
    public partial class AddSomeDataInProjects : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsNoticeKMApproveChangeWorkingTime",
                table: "Projects",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsNoticeKMApproveRequestOffDate",
                table: "Projects",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsNoticeKMRequestChangeWorkingTime",
                table: "Projects",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsNoticeKMRequestOffDate",
                table: "Projects",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsNoticeKMSubmitTS",
                table: "Projects",
                nullable: false,
                defaultValue: false);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsNoticeKMApproveChangeWorkingTime",
                table: "Projects");

            migrationBuilder.DropColumn(
                name: "IsNoticeKMApproveRequestOffDate",
                table: "Projects");

            migrationBuilder.DropColumn(
                name: "IsNoticeKMRequestChangeWorkingTime",
                table: "Projects");

            migrationBuilder.DropColumn(
                name: "IsNoticeKMRequestOffDate",
                table: "Projects");

            migrationBuilder.DropColumn(
                name: "IsNoticeKMSubmitTS",
                table: "Projects");
        }
    }
}
