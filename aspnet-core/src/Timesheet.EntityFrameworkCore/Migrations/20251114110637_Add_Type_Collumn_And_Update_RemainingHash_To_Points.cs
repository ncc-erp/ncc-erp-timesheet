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
                INSERT INTO UserPunishmentBalances (UserId, TotalPunishmentMoney, RemainPoints, CreationTime, CreatorUserId, IsDeleted)
                SELECT DISTINCT 
                    upp.UserId,
                    0 as TotalPunishmentMoney,
                    0 as RemainPoints,
                    GETDATE() as CreationTime,
                    upp.UserId as CreatorUserId,
                    0 as IsDeleted
                FROM UserPunishmentPaids upp
                WHERE upp.CreationTime >= '2024-11-01'
                AND NOT EXISTS (
                    SELECT 1 FROM UserPunishmentBalances upb 
                    WHERE upb.UserId = upp.UserId
                );

                UPDATE upb 
                SET RemainPoints = upb.RemainPoints + ISNULL(hash_totals.TotalAmount, 0)
                FROM UserPunishmentBalances upb
                INNER JOIN (
                    SELECT 
                        UserId,
                        SUM(Amount) as TotalAmount
                    FROM UserPunishmentPaids 
                    WHERE CreationTime >= '2024-11-01'
                    GROUP BY UserId
                ) hash_totals ON upb.UserId = hash_totals.UserId;

                INSERT INTO UserPunishmentRefunds (UserId, UserPunishmentId, Points, Type, CreationTime, CreatorUserId, IsDeleted)
                SELECT 
                    UserId,
                    NULL as UserPunishmentId,
                    Amount as Points,
                    0 as Type, -- PointType.IsClaim
                    CreationTime,
                    CreatorUserId,
                    0 as IsDeleted -- false
                FROM UserPunishmentPaids 
                WHERE CreationTime >= '2024-11-01';
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
