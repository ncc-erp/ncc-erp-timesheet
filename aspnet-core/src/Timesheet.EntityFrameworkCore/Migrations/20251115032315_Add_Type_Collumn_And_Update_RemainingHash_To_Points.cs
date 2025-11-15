using Microsoft.EntityFrameworkCore.Migrations;

namespace Timesheet.Migrations
{
    public partial class Add_Type_Collumn_And_Update_RemainingHash_To_Points : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<long>(
                name: "UserPunishmentId",
                table: "UserPunishmentRefunds",
                nullable: true,
                oldClrType: typeof(long));

            migrationBuilder.AddColumn<int>(
                name: "Type",
                table: "UserPunishmentRefunds",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.Sql(@"
                INSERT INTO UserPunishmentRefunds (UserId, UserPunishmentId, Points, Type, CreationTime, CreatorUserId, IsDeleted)
                SELECT 
                    up.UserId,
                    NULL AS UserPunishmentId,
                    SUM(up.Amount) AS Points,
                    0 AS Type, -- PointType.IsClaim
                    GETDATE() AS CreationTime,
                    up.UserId AS CreatorUserId,
                    0 AS IsDeleted -- false
                FROM UserPunishmentPaids up
                WHERE up.CreationTime >= '2025-11-01'
                GROUP BY up.UserId;
            ");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Type",
                table: "UserPunishmentRefunds");

            migrationBuilder.AlterColumn<long>(
                name: "UserPunishmentId",
                table: "UserPunishmentRefunds",
                nullable: false,
                oldClrType: typeof(long),
                oldNullable: true);
        }
    }
}
