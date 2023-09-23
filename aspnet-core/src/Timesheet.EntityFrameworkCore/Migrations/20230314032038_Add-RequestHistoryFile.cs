using System;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Timesheet.Migrations
{
    public partial class AddRequestHistoryFile : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "TeamBuildingRequestHistoryFiles",
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
                    Url = table.Column<string>(nullable: true),
                    TeamBuildingRequestHistoryId = table.Column<long>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TeamBuildingRequestHistoryFiles", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TeamBuildingRequestHistoryFiles_TeamBuildingRequestHistories_TeamBuildingRequestHistoryId",
                        column: x => x.TeamBuildingRequestHistoryId,
                        principalTable: "TeamBuildingRequestHistories",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_TeamBuildingRequestHistoryFiles_TeamBuildingRequestHistoryId",
                table: "TeamBuildingRequestHistoryFiles",
                column: "TeamBuildingRequestHistoryId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "TeamBuildingRequestHistoryFiles");
        }
    }
}
