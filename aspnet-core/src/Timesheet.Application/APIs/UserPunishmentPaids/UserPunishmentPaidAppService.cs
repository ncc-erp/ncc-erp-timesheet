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

            var startDate = input.GetStartDate();
            var endDate = input.GetEndDate();

            return await _userPunishmentPaidService.GetByCurrentUserAsync(startDate, endDate);
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
    }
}
