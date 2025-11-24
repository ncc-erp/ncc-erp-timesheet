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

        Task<UserPunishmentSummaryDto> PreviewApplyAndGetSummaryAsync(int year, int month);

        Task<UserPunishmentSummaryDto> ApplyRemainPointsAsync(int year, int month);
    }
}
