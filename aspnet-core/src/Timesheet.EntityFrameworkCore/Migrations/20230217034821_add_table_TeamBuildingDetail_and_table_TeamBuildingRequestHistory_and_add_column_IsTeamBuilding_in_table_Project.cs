using System;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Timesheet.Migrations
{
    public partial class add_table_TeamBuildingDetail_and_table_TeamBuildingRequestHistory_and_add_column_IsTeamBuilding_in_table_Project : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsTeamBuilding",
                table: "Projects",
                nullable: false,
                defaultValue: false);

            migrationBuilder.CreateTable(
                name: "TeamBuildingRequestHistories",
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
                    EmailAddressPM = table.Column<string>(nullable: true),
                    TitleRequest = table.Column<string>(nullable: true),
                    Money = table.Column<float>(nullable: false),
                    Status = table.Column<int>(nullable: false),
                    CreatedTime = table.Column<DateTime>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TeamBuildingRequestHistories", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "TeamBuildingDetails",
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
                    ProjectId = table.Column<long>(nullable: false),
                    EmailAddressPM = table.Column<string>(nullable: true),
                    UserId = table.Column<long>(nullable: false),
                    TeamBuildingRequestHistoryId = table.Column<long>(nullable: false),
                    Money = table.Column<float>(nullable: false),
                    DisbursedMoney = table.Column<float>(nullable: false),
                    RemainingMoney = table.Column<float>(nullable: false),
                    EmailAddressRequester = table.Column<string>(nullable: true),
                    Status = table.Column<int>(nullable: false),
                    CreatedTime = table.Column<DateTime>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TeamBuildingDetails", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TeamBuildingDetails_Projects_ProjectId",
                        column: x => x.ProjectId,
                        principalTable: "Projects",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_TeamBuildingDetails_TeamBuildingRequestHistories_TeamBuildingRequestHistoryId",
                        column: x => x.TeamBuildingRequestHistoryId,
                        principalTable: "TeamBuildingRequestHistories",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_TeamBuildingDetails_AbpUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AbpUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_TeamBuildingDetails_ProjectId",
                table: "TeamBuildingDetails",
                column: "ProjectId");

            migrationBuilder.CreateIndex(
                name: "IX_TeamBuildingDetails_TeamBuildingRequestHistoryId",
                table: "TeamBuildingDetails",
                column: "TeamBuildingRequestHistoryId");

            migrationBuilder.CreateIndex(
                name: "IX_TeamBuildingDetails_UserId",
                table: "TeamBuildingDetails",
                column: "UserId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "TeamBuildingDetails");

            migrationBuilder.DropTable(
                name: "TeamBuildingRequestHistories");

            migrationBuilder.DropColumn(
                name: "IsTeamBuilding",
                table: "Projects");
        }
    }
}
