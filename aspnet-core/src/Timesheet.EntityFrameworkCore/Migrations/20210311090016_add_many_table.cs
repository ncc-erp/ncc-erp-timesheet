using System;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Timesheet.Migrations
{
    public partial class add_many_table : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsUnlockedByEmployee",
                table: "MyTimesheets",
                nullable: true);

            migrationBuilder.AddColumn<byte>(
                name: "AbsenceTime",
                table: "AbsenceDayDetails",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "AfternoonEndAt",
                table: "AbpUsers",
                maxLength: 5,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "AfternoonStartAt",
                table: "AbpUsers",
                maxLength: 5,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "AfternoonWorking",
                table: "AbpUsers",
                maxLength: 5,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "MorningEndAt",
                table: "AbpUsers",
                maxLength: 5,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "MorningStartAt",
                table: "AbpUsers",
                maxLength: 5,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "MorningWorking",
                table: "AbpUsers",
                maxLength: 5,
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "isWorkingTimeDefault",
                table: "AbpUsers",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "Funds",
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
                    Amount = table.Column<double>(nullable: false),
                    Status = table.Column<byte>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Funds", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "HistoryWorkingTimes",
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
                    UserId = table.Column<long>(nullable: false),
                    ApplyDate = table.Column<DateTime>(nullable: false),
                    MorningStartTime = table.Column<string>(maxLength: 5, nullable: true),
                    MorningEndTime = table.Column<string>(maxLength: 5, nullable: true),
                    MorningWorkingTime = table.Column<string>(maxLength: 5, nullable: true),
                    AfternoonStartTime = table.Column<string>(maxLength: 5, nullable: true),
                    AfternoonEndTime = table.Column<string>(maxLength: 5, nullable: true),
                    AfternoonWorkingTime = table.Column<string>(maxLength: 5, nullable: true),
                    Status = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_HistoryWorkingTimes", x => x.Id);
                    table.ForeignKey(
                        name: "FK_HistoryWorkingTimes_AbpUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AbpUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Timekeepings",
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
                    UserId = table.Column<long>(nullable: true),
                    UserCode = table.Column<string>(maxLength: 5, nullable: true),
                    UserName = table.Column<string>(maxLength: 256, nullable: true),
                    UserEmail = table.Column<string>(maxLength: 256, nullable: true),
                    CheckIn = table.Column<string>(maxLength: 5, nullable: true),
                    CheckOut = table.Column<string>(maxLength: 5, nullable: true),
                    RegistrationTimeStart = table.Column<string>(maxLength: 5, nullable: true),
                    RegistrationTimeEnd = table.Column<string>(maxLength: 5, nullable: true),
                    Date = table.Column<DateTime>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Timekeepings", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Timekeepings_AbpUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AbpUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "UserUnlockIms",
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
                    UserId = table.Column<long>(nullable: false),
                    Type = table.Column<byte>(nullable: false),
                    Times = table.Column<int>(nullable: false),
                    Amount = table.Column<double>(nullable: false),
                    IsPayment = table.Column<bool>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserUnlockIms", x => x.Id);
                    table.ForeignKey(
                        name: "FK_UserUnlockIms_AbpUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AbpUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_HistoryWorkingTimes_UserId",
                table: "HistoryWorkingTimes",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_Timekeepings_UserId",
                table: "Timekeepings",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_UserUnlockIms_UserId",
                table: "UserUnlockIms",
                column: "UserId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Funds");

            migrationBuilder.DropTable(
                name: "HistoryWorkingTimes");

            migrationBuilder.DropTable(
                name: "Timekeepings");

            migrationBuilder.DropTable(
                name: "UserUnlockIms");

            migrationBuilder.DropColumn(
                name: "IsUnlockedByEmployee",
                table: "MyTimesheets");

            migrationBuilder.DropColumn(
                name: "AbsenceTime",
                table: "AbsenceDayDetails");

            migrationBuilder.DropColumn(
                name: "AfternoonEndAt",
                table: "AbpUsers");

            migrationBuilder.DropColumn(
                name: "AfternoonStartAt",
                table: "AbpUsers");

            migrationBuilder.DropColumn(
                name: "AfternoonWorking",
                table: "AbpUsers");

            migrationBuilder.DropColumn(
                name: "MorningEndAt",
                table: "AbpUsers");

            migrationBuilder.DropColumn(
                name: "MorningStartAt",
                table: "AbpUsers");

            migrationBuilder.DropColumn(
                name: "MorningWorking",
                table: "AbpUsers");

            migrationBuilder.DropColumn(
                name: "isWorkingTimeDefault",
                table: "AbpUsers");
        }
    }
}
