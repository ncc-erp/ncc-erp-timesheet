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
        public async Task<PreviewAndApplyPunishmentPointsResultDto> PreviewAndApplyPunishmentPoints(PreviewAndApplyPunishmentPointsDto input)
        {
            var result = await _userPunishmentPaidService.PreviewAndApplyPunishmentPointsAsync(input.Year, input.Month);

            return new PreviewAndApplyPunishmentPointsResultDto
            {
                Success = result.Success,
                Message = result.Message,
                TotalHashAmount = result.TotalHashAmount,
                TotalPunishmentMoney = result.TotalPunishmentMoney,
                RemainPoints = result.RemainPoints,
                EffectivePunishmentAmount = result.EffectivePunishmentAmount,
                PunishmentsMarkedAsPaid = result.PunishmentsMarkedAsPaid,
                HasSufficientFunds = result.HasSufficientFunds
            };
        }

        [HttpGet]
        [AbpAuthorize]
        public async Task<int> GetTotalRemainPointsUsedInMonth(int year, int month)
        {
            if (!AbpSession.UserId.HasValue)
            {
                return 0;
            }
            
            var userId = AbpSession.UserId.Value;
            return await _userPunishmentPaidService.GetTotalRemainPointsUsedInMonth(userId, year, month);
        }

        [HttpGet]
        [AbpAuthorize]
        public async Task<GetUserPunishmentBalanceDto> GetCurrentUserBalance()
        {
            var (totalPunishmentMoney, remainPoints, hasBalance) = await _userPunishmentPaidService.GetUserPunishmentBalanceAsync();

            var effectiveAmount = Math.Max(0, totalPunishmentMoney - remainPoints);

            return new GetUserPunishmentBalanceDto
            {
                TotalPunishmentMoney = totalPunishmentMoney,
                RemainPoints = remainPoints,
                EffectiveAmount = effectiveAmount,
                HasBalance = hasBalance
            };
        }

        [HttpGet]
        [AbpAuthorize]
        public async Task<int> GetTotalPaidPunishmentInMonth(int year, int month)
        {
            if (!AbpSession.UserId.HasValue)
            {
                return 0;
            }
            
            var userId = AbpSession.UserId.Value;
            return await _userPunishmentPaidService.GetTotalPaidPunishmentInMonth(userId, year, month);
        }
    }
}
