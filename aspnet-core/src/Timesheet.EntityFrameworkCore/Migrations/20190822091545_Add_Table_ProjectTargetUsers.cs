using System;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Timesheet.Migrations
{
    public partial class Add_Table_ProjectTargetUsers : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<long>(
                name: "ProjectTargetUserId",
                table: "MyTimesheets",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "ProjectTargetUsers",
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
                    ProjectId = table.Column<long>(nullable: false),
                    RoleName = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProjectTargetUsers", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ProjectTargetUsers_Projects_ProjectId",
                        column: x => x.ProjectId,
                        principalTable: "Projects",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ProjectTargetUsers_AbpUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AbpUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_MyTimesheets_ProjectTargetUserId",
                table: "MyTimesheets",
                column: "ProjectTargetUserId");

            migrationBuilder.CreateIndex(
                name: "IX_ProjectTargetUsers_ProjectId",
                table: "ProjectTargetUsers",
                column: "ProjectId");

            migrationBuilder.CreateIndex(
                name: "IX_ProjectTargetUsers_UserId",
                table: "ProjectTargetUsers",
                column: "UserId");

            migrationBuilder.AddForeignKey(
                name: "FK_MyTimesheets_ProjectTargetUsers_ProjectTargetUserId",
                table: "MyTimesheets",
                column: "ProjectTargetUserId",
                principalTable: "ProjectTargetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_MyTimesheets_ProjectTargetUsers_ProjectTargetUserId",
                table: "MyTimesheets");

            migrationBuilder.DropTable(
                name: "ProjectTargetUsers");

            migrationBuilder.DropIndex(
                name: "IX_MyTimesheets_ProjectTargetUserId",
                table: "MyTimesheets");

            migrationBuilder.DropColumn(
                name: "ProjectTargetUserId",
                table: "MyTimesheets");
        }
    }
}
