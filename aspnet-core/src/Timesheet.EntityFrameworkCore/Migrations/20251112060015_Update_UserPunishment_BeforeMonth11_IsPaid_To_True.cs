using Microsoft.EntityFrameworkCore.Migrations;

namespace Timesheet.Migrations
{
    public partial class Update_UserPunishment_BeforeMonth11_IsPaid_To_True : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
                UPDATE UserPunishments 
                SET IsPaid = 1 
                WHERE DateAt < '2025-11-01'
                AND IsDeleted = 0
            ");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
                UPDATE UserPunishments 
                SET IsPaid = 0 
                WHERE DateAt < '2025-11-01'
                AND IsDeleted = 0
            ");
        }
    }
}
