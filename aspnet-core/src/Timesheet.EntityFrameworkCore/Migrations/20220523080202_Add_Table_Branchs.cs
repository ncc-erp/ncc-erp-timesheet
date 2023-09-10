using System;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Timesheet.Migrations
{
    public partial class Add_Table_Branchs : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Branch",
                table: "AbpUsers",
                newName: "BranchOld");

            migrationBuilder.AlterColumn<byte>(
                name: "Branch",
                table: "DayOffSettings",
                nullable: true,
                oldClrType: typeof(byte));

            migrationBuilder.AddColumn<long>(
                name: "BranchId",
                table: "AbpUsers",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "Branchs",
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
                    DisplayName = table.Column<string>(nullable: true),
                    MorningWorking = table.Column<double>(maxLength: 5, nullable: false),
                    MorningStartAt = table.Column<string>(maxLength: 5, nullable: true),
                    MorningEndAt = table.Column<string>(maxLength: 5, nullable: true),
                    AfternoonWorking = table.Column<double>(maxLength: 5, nullable: false),
                    AfternoonStartAt = table.Column<string>(maxLength: 5, nullable: true),
                    AfternoonEndAt = table.Column<string>(maxLength: 5, nullable: true),
                    Color = table.Column<string>(nullable: true),
                    Code = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Branchs", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AbpUsers_BranchId",
                table: "AbpUsers",
                column: "BranchId");

        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Branchs");

            migrationBuilder.DropIndex(
                name: "IX_AbpUsers_BranchId",
                table: "AbpUsers");

            migrationBuilder.DropColumn(
                name: "BranchId",
                table: "AbpUsers");

            migrationBuilder.RenameColumn(
                name: "BranchOld",
                table: "AbpUsers",
                newName: "Branch");

            migrationBuilder.AlterColumn<byte>(
                name: "Branch",
                table: "DayOffSettings",
                nullable: false,
                oldClrType: typeof(byte),
                oldNullable: true);
        }
    }
}
