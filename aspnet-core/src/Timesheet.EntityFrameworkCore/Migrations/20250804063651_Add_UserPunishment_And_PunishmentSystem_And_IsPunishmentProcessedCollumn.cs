using System;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Timesheet.Migrations
{
    public partial class Add_UserPunishment_And_PunishmentSystem_And_IsPunishmentProcessedCollumn : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsPunishmentProcessed",
                table: "ReviewInterns",
                nullable: false,
                defaultValue: false);

            migrationBuilder.CreateTable(
                name: "PunishmentSystems",
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
                    Name = table.Column<string>(maxLength: 256, nullable: false),
                    Description = table.Column<string>(maxLength: 1000, nullable: true),
                    Type = table.Column<int>(nullable: false),
                    Money = table.Column<int>(nullable: false),
                    IsActive = table.Column<bool>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PunishmentSystems", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "UserPunishments",
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
                    PunishmentSystemId = table.Column<long>(nullable: false),
                    Type = table.Column<int>(nullable: false),
                    Count = table.Column<int>(nullable: false),
                    TotalMoney = table.Column<int>(nullable: false),
                    UserNote = table.Column<string>(maxLength: 1000, nullable: true),
                    NoteReply = table.Column<string>(maxLength: 1000, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserPunishments", x => x.Id);
                    table.ForeignKey(
                        name: "FK_UserPunishments_PunishmentSystems_PunishmentSystemId",
                        column: x => x.PunishmentSystemId,
                        principalTable: "PunishmentSystems",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_UserPunishments_AbpUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AbpUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_UserPunishments_PunishmentSystemId",
                table: "UserPunishments",
                column: "PunishmentSystemId");

            migrationBuilder.CreateIndex(
                name: "IX_UserPunishments_UserId",
                table: "UserPunishments",
                column: "UserId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "UserPunishments");

            migrationBuilder.DropTable(
                name: "PunishmentSystems");

            migrationBuilder.DropColumn(
                name: "IsPunishmentProcessed",
                table: "ReviewInterns");
        }
    }
}
