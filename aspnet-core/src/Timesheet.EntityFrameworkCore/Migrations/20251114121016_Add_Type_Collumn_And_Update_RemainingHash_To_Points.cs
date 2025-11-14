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
                SELECT 
                    all_users.UserId,
                    ISNULL(punishment_totals.TotalAmount, 0) as TotalPunishmentMoney,
                    ISNULL(hash_totals.TotalAmount, 0) as RemainPoints,
                    GETDATE() as CreationTime,
                    all_users.UserId as CreatorUserId,
                    0 as IsDeleted
                FROM (
                    SELECT DISTINCT UserId FROM UserPunishments 
                    WHERE IsDeleted = 0 AND (IsPaid = 0 OR IsPaid IS NULL)
                    UNION
                    SELECT DISTINCT UserId FROM UserPunishmentPaids 
                    WHERE CreationTime >= '2024-11-01'
                ) all_users
                LEFT JOIN (
                    SELECT UserId, SUM(TotalMoney) as TotalAmount
                    FROM UserPunishments 
                    WHERE IsDeleted = 0 AND (IsPaid = 0 OR IsPaid IS NULL)
                    GROUP BY UserId
                ) punishment_totals ON all_users.UserId = punishment_totals.UserId
                LEFT JOIN (
                    SELECT UserId, SUM(Amount) as TotalAmount
                    FROM UserPunishmentPaids 
                    WHERE CreationTime >= '2024-11-01'
                    GROUP BY UserId
                ) hash_totals ON all_users.UserId = hash_totals.UserId
                WHERE NOT EXISTS (
                    SELECT 1 FROM UserPunishmentBalances 
                    WHERE UserId = all_users.UserId
                );

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
