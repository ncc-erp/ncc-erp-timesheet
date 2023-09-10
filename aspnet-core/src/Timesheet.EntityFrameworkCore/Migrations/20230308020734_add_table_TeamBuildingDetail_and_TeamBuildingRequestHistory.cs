using System;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Timesheet.Migrations
{
    public partial class add_table_TeamBuildingDetail_and_TeamBuildingRequestHistory : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsAllowTeamBuilding",
                table: "Projects",
                nullable: false,
                defaultValue: true);

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
                    RequesterId = table.Column<long>(nullable: false),
                    TitleRequest = table.Column<string>(nullable: true),
                    RequestMoney = table.Column<float>(nullable: false),
                    DisbursedMoney = table.Column<float>(nullable: true),
                    RemainingMoney = table.Column<float>(nullable: true),
                    RemainingMoneyStatus = table.Column<byte>(nullable: false),
                    Status = table.Column<byte>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TeamBuildingRequestHistories", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TeamBuildingRequestHistories_AbpUsers_RequesterId",
                        column: x => x.RequesterId,
                        principalTable: "AbpUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
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
                    EmployeeId = table.Column<long>(nullable: false),
                    ApplyMonth = table.Column<DateTime>(nullable: false),
                    TeamBuildingRequestHistoryId = table.Column<long>(nullable: true),
                    Money = table.Column<float>(nullable: false),
                    Status = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TeamBuildingDetails", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TeamBuildingDetails_AbpUsers_EmployeeId",
                        column: x => x.EmployeeId,
                        principalTable: "AbpUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
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
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_TeamBuildingDetails_EmployeeId",
                table: "TeamBuildingDetails",
                column: "EmployeeId");

            migrationBuilder.CreateIndex(
                name: "IX_TeamBuildingDetails_ProjectId",
                table: "TeamBuildingDetails",
                column: "ProjectId");

            migrationBuilder.CreateIndex(
                name: "IX_TeamBuildingDetails_TeamBuildingRequestHistoryId",
                table: "TeamBuildingDetails",
                column: "TeamBuildingRequestHistoryId");

            migrationBuilder.CreateIndex(
                name: "IX_TeamBuildingRequestHistories_RequesterId",
                table: "TeamBuildingRequestHistories",
                column: "RequesterId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "TeamBuildingDetails");

            migrationBuilder.DropTable(
                name: "TeamBuildingRequestHistories");

            migrationBuilder.DropColumn(
                name: "IsAllowTeamBuilding",
                table: "Projects");
        }
    }
}
