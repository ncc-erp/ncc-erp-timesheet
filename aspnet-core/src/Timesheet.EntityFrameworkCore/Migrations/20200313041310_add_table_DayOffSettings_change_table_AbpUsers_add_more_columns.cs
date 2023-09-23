using System;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Timesheet.Migrations
{
    public partial class add_table_DayOffSettings_change_table_AbpUsers_add_more_columns : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<double>(
                name: "AllowedLeaveDay",
                table: "AbpUsers",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "AvatarPath",
                table: "AbpUsers",
                nullable: true);

            migrationBuilder.AddColumn<byte>(
                name: "Branch",
                table: "AbpUsers",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "JobTitle",
                table: "AbpUsers",
                nullable: true);

            migrationBuilder.AddColumn<byte>(
                name: "Level",
                table: "AbpUsers",
                nullable: true);

            migrationBuilder.AddColumn<long>(
                name: "ManagerId",
                table: "AbpUsers",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "RegisterWorkDay",
                table: "AbpUsers",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Salary",
                table: "AbpUsers",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "SalaryAt",
                table: "AbpUsers",
                nullable: true);

            migrationBuilder.AddColumn<byte>(
                name: "Sex",
                table: "AbpUsers",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "StartDateAt",
                table: "AbpUsers",
                nullable: true);

            migrationBuilder.AddColumn<byte>(
                name: "Type",
                table: "AbpUsers",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "UserCode",
                table: "AbpUsers",
                maxLength: 5,
                nullable: true);

            migrationBuilder.CreateTable(
                name: "DayOffSettings",
                columns: table => new
                {
                    Id = table.Column<long>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    CreationTime = table.Column<DateTime>(nullable: false),
                    CreatorUserId = table.Column<long>(nullable: true),
                    LastModificationTime = table.Column<DateTime>(nullable: true),
                    LastModifierUserId = table.Column<long>(nullable: true),
                    IsDeleted = table.Column<bool>(nullable: false),
                    DeleterUserId = table.Column<long>(nullable: true),
                    DeletionTime = table.Column<DateTime>(nullable: true),
                    DayOff = table.Column<DateTime>(nullable: false),
                    Name = table.Column<string>(nullable: true),
                    Coefficient = table.Column<double>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DayOffSettings", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AbpUsers_ManagerId",
                table: "AbpUsers",
                column: "ManagerId");

            migrationBuilder.AddForeignKey(
                name: "FK_AbpUsers_AbpUsers_ManagerId",
                table: "AbpUsers",
                column: "ManagerId",
                principalTable: "AbpUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AbpUsers_AbpUsers_ManagerId",
                table: "AbpUsers");

            migrationBuilder.DropTable(
                name: "DayOffSettings");

            migrationBuilder.DropIndex(
                name: "IX_AbpUsers_ManagerId",
                table: "AbpUsers");

            migrationBuilder.DropColumn(
                name: "AllowedLeaveDay",
                table: "AbpUsers");

            migrationBuilder.DropColumn(
                name: "AvatarPath",
                table: "AbpUsers");

            migrationBuilder.DropColumn(
                name: "Branch",
                table: "AbpUsers");

            migrationBuilder.DropColumn(
                name: "JobTitle",
                table: "AbpUsers");

            migrationBuilder.DropColumn(
                name: "Level",
                table: "AbpUsers");

            migrationBuilder.DropColumn(
                name: "ManagerId",
                table: "AbpUsers");

            migrationBuilder.DropColumn(
                name: "RegisterWorkDay",
                table: "AbpUsers");

            migrationBuilder.DropColumn(
                name: "Salary",
                table: "AbpUsers");

            migrationBuilder.DropColumn(
                name: "SalaryAt",
                table: "AbpUsers");

            migrationBuilder.DropColumn(
                name: "Sex",
                table: "AbpUsers");

            migrationBuilder.DropColumn(
                name: "StartDateAt",
                table: "AbpUsers");

            migrationBuilder.DropColumn(
                name: "Type",
                table: "AbpUsers");

            migrationBuilder.DropColumn(
                name: "UserCode",
                table: "AbpUsers");
        }
    }
}
