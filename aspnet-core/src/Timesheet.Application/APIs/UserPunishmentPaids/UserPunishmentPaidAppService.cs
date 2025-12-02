using Abp.Application.Services;
using Abp.Authorization;
using Abp.UI;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Timesheet.APIs.UserPunishmentPaids.Dto;
using Timesheet.DomainServices;
using Timesheet.DomainServices.Dto;

namespace Timesheet.APIs.UserPunishmentPaids
{
    [AbpAuthorize]
    public class UserPunishmentPaidAppService : ApplicationService
    {
        private readonly IUserPunishmentPaidService _userPunishmentPaidService;

        public UserPunishmentPaidAppService(
            IUserPunishmentPaidService userPunishmentPaidService)
        {
            _userPunishmentPaidService = userPunishmentPaidService;
        }

        [HttpGet]
        [AbpAuthorize]
        public async Task<List<UserPunishmentPaidDto>> GetForCurrentUser(GetPunishmentPaidFilterDto input)
        {
            if (input == null)
            {
                throw new ArgumentNullException(nameof(input));
            }

            if (input.Year <= 0 || input.Month <= 0 || input.Month > 12)
            {
                throw new ArgumentException("Invalid year or month");
            }

            var targetMonth = new DateTime(input.Year, input.Month, 1);
            return await _userPunishmentPaidService.GetByCurrentUserAsync(targetMonth);
        }

        [HttpPost]
        [AbpAuthorize]
        public async Task<MarkPaidTransactionResultDto> MarkPaidTransaction(MarkPaidTransactionDto input)
        {
            if (string.IsNullOrWhiteSpace(input.TransactionHash))
            {
                throw new UserFriendlyException("Transaction hash is required");
            }

            var result = await _userPunishmentPaidService.MarkPaidTransactions(input.TransactionHash, input.Year, input.Month);

            return new MarkPaidTransactionResultDto
            {
                Success = result,
                Message = "Transaction marked as paid successfully"
            };
        }

        [HttpPost]
        [AbpAuthorize]
        public async Task<UserPunishmentSummaryDto> PreviewApplyAndGetSummary(PreviewAndApplyPunishmentPointsDto input)
        {
            return await _userPunishmentPaidService.PreviewApplyAndGetSummaryAsync(input.Year, input.Month);
        }

        [HttpPost]
        [AbpAuthorize]
        public async Task<UserPunishmentSummaryDto> ApplyRemainPoints()
        {
            return await _userPunishmentPaidService.ApplyRemainPointsAsync();
        }
    }
}
