using Microsoft.EntityFrameworkCore.Migrations;

namespace Timesheet.Migrations
{
    public partial class TeamBuildingRequestHistory_Add_Column_VATMoney : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<float>(
                name: "VATMoney",
                table: "TeamBuildingRequestHistories",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "VATMoney",
                table: "TeamBuildingRequestHistories");
        }
    }
}
