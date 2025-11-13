using System;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Timesheet.Migrations
{
    public partial class Add_UserPunishmentBalance_And_UserPunishmentRefund_And_IsPaid_Column : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsPaid",
                table: "UserPunishments",
                nullable: false,
                defaultValue: false);

            migrationBuilder.CreateTable(
                name: "UserPunishmentBalances",
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
                    TotalPunishmentMoney = table.Column<int>(nullable: false),
                    RemainPoints = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserPunishmentBalances", x => x.Id);
                    table.ForeignKey(
                        name: "FK_UserPunishmentBalances_AbpUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AbpUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "UserPunishmentRefunds",
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
                    UserPunishmentId = table.Column<long>(nullable: true),
                    Points = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserPunishmentRefunds", x => x.Id);
                    table.ForeignKey(
                        name: "FK_UserPunishmentRefunds_AbpUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AbpUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_UserPunishmentRefunds_UserPunishments_UserPunishmentId",
                        column: x => x.UserPunishmentId,
                        principalTable: "UserPunishments",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_UserPunishmentBalances_UserId",
                table: "UserPunishmentBalances",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_UserPunishmentRefunds_UserId",
                table: "UserPunishmentRefunds",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_UserPunishmentRefunds_UserPunishmentId",
                table: "UserPunishmentRefunds",
                column: "UserPunishmentId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "UserPunishmentBalances");

            migrationBuilder.DropTable(
                name: "UserPunishmentRefunds");

            migrationBuilder.DropColumn(
                name: "IsPaid",
                table: "UserPunishments");
        }
    }
}
