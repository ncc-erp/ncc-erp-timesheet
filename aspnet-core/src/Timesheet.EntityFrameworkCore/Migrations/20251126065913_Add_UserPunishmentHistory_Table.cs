using System;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Timesheet.Migrations
{
    public partial class Add_UserPunishmentHistory_Table : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "UserPunishmentHistories",
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
                    DateAt = table.Column<DateTime>(nullable: false),
                    UserId = table.Column<long>(nullable: false),
                    UserPunishmentId = table.Column<long>(nullable: false),
                    PunishmentSystemId = table.Column<long>(nullable: false),
                    Type = table.Column<int>(nullable: false),
                    Count = table.Column<int>(nullable: false),
                    TotalMoney = table.Column<int>(nullable: false),
                    UserNote = table.Column<string>(maxLength: 1000, nullable: true),
                    NoteReply = table.Column<string>(maxLength: 1000, nullable: true),
                    IsPaid = table.Column<bool>(nullable: false),
                    UserPunishmentPaidId = table.Column<long>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserPunishmentHistories", x => x.Id);
                    table.ForeignKey(
                        name: "FK_UserPunishmentHistories_PunishmentSystems_PunishmentSystemId",
                        column: x => x.PunishmentSystemId,
                        principalTable: "PunishmentSystems",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_UserPunishmentHistories_AbpUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AbpUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_UserPunishmentHistories_UserPunishments_UserPunishmentId",
                        column: x => x.UserPunishmentId,
                        principalTable: "UserPunishments",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.NoAction);
                    table.ForeignKey(
                        name: "FK_UserPunishmentHistories_UserPunishmentPaids_UserPunishmentPaidId",
                        column: x => x.UserPunishmentPaidId,
                        principalTable: "UserPunishmentPaids",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_UserPunishmentHistories_PunishmentSystemId",
                table: "UserPunishmentHistories",
                column: "PunishmentSystemId");

            migrationBuilder.CreateIndex(
                name: "IX_UserPunishmentHistories_UserId",
                table: "UserPunishmentHistories",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_UserPunishmentHistories_UserPunishmentId",
                table: "UserPunishmentHistories",
                column: "UserPunishmentId");

            migrationBuilder.CreateIndex(
                name: "IX_UserPunishmentHistories_UserPunishmentPaidId",
                table: "UserPunishmentHistories",
                column: "UserPunishmentPaidId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "UserPunishmentHistories");
        }
    }
}
