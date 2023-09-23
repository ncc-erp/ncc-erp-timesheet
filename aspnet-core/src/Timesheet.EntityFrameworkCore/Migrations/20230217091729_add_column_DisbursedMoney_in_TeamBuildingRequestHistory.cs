using Microsoft.EntityFrameworkCore.Migrations;

namespace Timesheet.Migrations
{
    public partial class add_column_DisbursedMoney_in_TeamBuildingRequestHistory : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Money",
                table: "TeamBuildingRequestHistories",
                newName: "RequestMoney");

            migrationBuilder.AddColumn<float>(
                name: "DisbursedMoney",
                table: "TeamBuildingRequestHistories",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DisbursedMoney",
                table: "TeamBuildingRequestHistories");

            migrationBuilder.RenameColumn(
                name: "RequestMoney",
                table: "TeamBuildingRequestHistories",
                newName: "Money");
        }
    }
}
