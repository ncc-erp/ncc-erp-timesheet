using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Timesheet.DomainServices.Dto;
using Timesheet.Entities;

namespace Timesheet.DomainServices
{
    public interface IUserPunishmentPaidService
    {
        Task<List<UserPunishmentPaidDto>> GetByCurrentUserAsync(DateTime targetMonth);

        Task<bool> MarkPaidTransactions(string transactionHash, int year, int month);

        Task<PreviewAndApplyPunishmentPointsResult> PreviewAndApplyPunishmentPointsAsync(int year, int month);

        Task<int> GetTotalRemainPointsUsedInMonth(long userId, int year, int month);

        Task<int> GetTotalPaidPunishmentInMonth(long userId, int year, int month);

        Task<(int TotalPunishmentMoney, int RemainPoints, bool HasBalance)> GetUserPunishmentBalanceAsync();
    }
}
