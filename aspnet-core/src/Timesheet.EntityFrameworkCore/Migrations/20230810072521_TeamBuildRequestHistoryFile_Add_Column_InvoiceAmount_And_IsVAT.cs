using Microsoft.EntityFrameworkCore.Migrations;

namespace Timesheet.Migrations
{
    public partial class TeamBuildRequestHistoryFile_Add_Column_InvoiceAmount_And_IsVAT : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<float>(
                name: "InvoiceAmount",
                table: "TeamBuildingRequestHistoryFiles",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsVAT",
                table: "TeamBuildingRequestHistoryFiles",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "InvoiceAmount",
                table: "TeamBuildingRequestHistoryFiles");

            migrationBuilder.DropColumn(
                name: "IsVAT",
                table: "TeamBuildingRequestHistoryFiles");
        }
    }
}
