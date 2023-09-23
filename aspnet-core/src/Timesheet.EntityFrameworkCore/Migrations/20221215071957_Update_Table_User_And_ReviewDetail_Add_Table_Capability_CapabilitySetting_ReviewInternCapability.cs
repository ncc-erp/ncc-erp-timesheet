using System;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Timesheet.Migrations
{
    public partial class Update_Table_User_And_ReviewDetail_Add_Table_Capability_CapabilitySetting_ReviewInternCapability : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<float>(
                name: "RateStar",
                table: "ReviewDetails",
                nullable: true,
                oldClrType: typeof(int),
                oldNullable: true);

            migrationBuilder.AddColumn<long>(
                name: "PositionId",
                table: "AbpUsers",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "Capabilities",
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
                    Name = table.Column<string>(nullable: true),
                    Type = table.Column<int>(nullable: false),
                    Note = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Capabilities", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "CapabilitySettings",
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
                    CapabilityId = table.Column<long>(nullable: false),
                    PositionId = table.Column<long>(nullable: true),
                    GuildeLine = table.Column<string>(nullable: true),
                    Coefficient = table.Column<float>(nullable: false),
                    UserType = table.Column<byte>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CapabilitySettings", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CapabilitySettings_Capabilities_CapabilityId",
                        column: x => x.CapabilityId,
                        principalTable: "Capabilities",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_CapabilitySettings_Positions_PositionId",
                        column: x => x.PositionId,
                        principalTable: "Positions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ReviewInternCapabilities",
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
                    ReviewDetailId = table.Column<long>(nullable: false),
                    CapabilityId = table.Column<long>(nullable: false),
                    Point = table.Column<float>(nullable: false),
                    Coefficient = table.Column<float>(nullable: false),
                    Note = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ReviewInternCapabilities", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ReviewInternCapabilities_Capabilities_CapabilityId",
                        column: x => x.CapabilityId,
                        principalTable: "Capabilities",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ReviewInternCapabilities_ReviewDetails_ReviewDetailId",
                        column: x => x.ReviewDetailId,
                        principalTable: "ReviewDetails",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AbpUsers_PositionId",
                table: "AbpUsers",
                column: "PositionId");

            migrationBuilder.CreateIndex(
                name: "IX_CapabilitySettings_CapabilityId",
                table: "CapabilitySettings",
                column: "CapabilityId");

            migrationBuilder.CreateIndex(
                name: "IX_CapabilitySettings_PositionId",
                table: "CapabilitySettings",
                column: "PositionId");

            migrationBuilder.CreateIndex(
                name: "IX_ReviewInternCapabilities_CapabilityId",
                table: "ReviewInternCapabilities",
                column: "CapabilityId");

            migrationBuilder.CreateIndex(
                name: "IX_ReviewInternCapabilities_ReviewDetailId",
                table: "ReviewInternCapabilities",
                column: "ReviewDetailId");

            migrationBuilder.AddForeignKey(
                name: "FK_AbpUsers_Positions_PositionId",
                table: "AbpUsers",
                column: "PositionId",
                principalTable: "Positions",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AbpUsers_Positions_PositionId",
                table: "AbpUsers");

            migrationBuilder.DropTable(
                name: "CapabilitySettings");

            migrationBuilder.DropTable(
                name: "ReviewInternCapabilities");

            migrationBuilder.DropTable(
                name: "Capabilities");

            migrationBuilder.DropIndex(
                name: "IX_AbpUsers_PositionId",
                table: "AbpUsers");

            migrationBuilder.DropColumn(
                name: "PositionId",
                table: "AbpUsers");

            migrationBuilder.AlterColumn<int>(
                name: "RateStar",
                table: "ReviewDetails",
                nullable: true,
                oldClrType: typeof(float),
                oldNullable: true);
        }
    }
}
