using Abp.Domain.Repositories;
using Abp.Runtime.Session;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Ncc.Authorization.Users;
using Ncc.IoC;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Security.Cryptography;
using System.Threading.Tasks;
using Abp.UI;
using SimpleBase;
using Timesheet.DomainServices.Dto;
using Timesheet.Entities;
using Timesheet.Services.MMN.Dto;
using static Ncc.Entities.Enum.StatusEnum;

namespace Timesheet.DomainServices
{
    public class UserPunishmentPaidService : BaseDomainService, IUserPunishmentPaidService
    {
        private readonly IRepository<UserPunishmentPaid, long> _userPunishmentPaidRepository;
        private readonly IRepository<User, long> _userRepository;
        private readonly IAbpSession _abpSession;
        private readonly ILogger<UserPunishmentPaidService> _logger;
        private readonly string _indexerUri;
        private readonly string _donationWallet;
        private const string _serviceName = "MMNService";
        private static readonly decimal TOKEN_DECIMAL_FACTOR = (decimal)Math.Pow(10, 6);

        public static string GenerateAddress(string input)
        {
            using (var sha256 = SHA256.Create())
            {
                var hash = sha256.ComputeHash(System.Text.Encoding.UTF8.GetBytes(input));
                return Base58Encode(hash);
            }
        }

        private static string Base58Encode(byte[] data)
        {
            return SimpleBase.Base58.Bitcoin.Encode(data);
        }

        public UserPunishmentPaidService(
            IWorkScope workScope,
            IRepository<UserPunishmentPaid, long> userPunishmentPaidRepository,
            IRepository<User, long> userRepository,
            IAbpSession abpSession,
            ILogger<UserPunishmentPaidService> logger,
            IConfiguration configuration) : base(workScope)
        {
            _userPunishmentPaidRepository = userPunishmentPaidRepository;
            _userRepository = userRepository;
            _abpSession = abpSession;
            _logger = logger;
            _indexerUri = configuration.GetValue<string>($"{_serviceName}:IndexerUri");
            _donationWallet = configuration.GetValue<string>($"{_serviceName}:DonationWallet");
        }

        public async Task<List<UserPunishmentPaidDto>> GetByCurrentUserAsync(DateTime targetMonth)
        {
            try
            {
                if (!_abpSession.UserId.HasValue)
                {
                    return new List<UserPunishmentPaidDto>();
                }

                var currentUserId = _abpSession.UserId.Value;
                var endOfMonth = targetMonth.AddMonths(1);

                _logger.LogInformation($"Getting punishment paid data for user {currentUserId} for target month {targetMonth:yyyy-MM}");

                var entities = await _userPunishmentPaidRepository
                    .GetAll()
                    .Where(x => x.UserId == currentUserId)
                    .Where(x => x.TargetMonth >= targetMonth && x.TargetMonth < endOfMonth)
                    .OrderByDescending(x => x.DateAt)
                    .ToListAsync();

                _logger.LogInformation($"Found {entities.Count} punishment paid records for user {currentUserId} in {targetMonth:yyyy-MM}");

                return entities.Select(x => new UserPunishmentPaidDto
                {
                    DateAt = x.DateAt,
                    TargetMonth = x.TargetMonth,
                    Amount = x.Amount,
                    TxHash = x.TxHash
                }).ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error when getting user punishment paid data for current user for target month {targetMonth:yyyy-MM}");
                return new List<UserPunishmentPaidDto>();
            }
        }

        public async Task<bool> MarkPaidTransactions(string transactionHash, int year, int month)
        {
            if (!_abpSession.UserId.HasValue)
            {
                _logger.LogError($"Cannot mark paid transaction {transactionHash}: User not logged in");
                throw new UserFriendlyException("You must be logged in to mark a transaction as paid.");
            }

            var transactionInfo = await GetMMNTransactionsInfo(transactionHash);

            if (transactionInfo.Status != MmnTransactionStatus.Finalized)
            {
                _logger.LogError($"Transaction status {transactionInfo.Status} is not successful for hash {transactionHash}");
                throw new UserFriendlyException("Transaction has not been confirmed successfully.");
            }

            if (!decimal.TryParse(transactionInfo.Value, out decimal decimalAmount))
            {
                _logger.LogError($"Cannot parse transaction amount: {transactionInfo.Value}");
                return false;
            }

            var transactionDate = DateTimeOffset.FromUnixTimeSeconds(transactionInfo.TransactionTimestamp).DateTime;
            var targetMonthDate = new DateTime(year, month, 1);
            
            _logger.LogInformation($"Processing payment: Transaction date is {transactionDate:yyyy-MM-dd}, applying to month {targetMonthDate:yyyy-MM}");

            if (transactionInfo.ToAddress != _donationWallet)
            {
                _logger.LogError($"Transaction to_address {transactionInfo.ToAddress} does not match donation wallet {_donationWallet}");
                throw new UserFriendlyException($"Transaction must be sent to the official donation wallet. Please check your transaction details.");
            }

            var currentUser = await _userRepository.GetAsync(_abpSession.UserId.Value);
            
            var expectedFromAddress = GenerateAddress(currentUser.MezonUserId);
            if (transactionInfo.FromAddress != expectedFromAddress)
            {
                _logger.LogError($"Transaction from_address {transactionInfo.FromAddress} does not match expected address {expectedFromAddress} for user {currentUser.MezonUserId}");
                throw new UserFriendlyException($"Transaction must be sent from your own wallet. Please use your personal wallet to make the payment.");
            }

            var endOfMonth = targetMonthDate.AddMonths(1);
            var hasUnpaidPunishments = await WorkScope.GetAll<UserPunishment>()
                .AnyAsync(p => p.UserId == _abpSession.UserId.Value 
                    && !p.IsDeleted 
                    && (p.IsPaid == false || p.IsPaid == null)
                    && p.DateAt >= targetMonthDate 
                    && p.DateAt < endOfMonth);

            if (!hasUnpaidPunishments)
            {
                _logger.LogWarning($"User {_abpSession.UserId.Value} attempted to make payment for {targetMonthDate:yyyy-MM} but has no unpaid punishments in that month");
                throw new UserFriendlyException($"You cannot make a payment for {targetMonthDate:yyyy-MM} because you have no unpaid punishments in that month.");
            }

            var existingTransaction = await _userPunishmentPaidRepository
                .GetAll()
                .FirstOrDefaultAsync(x => x.TxHash == transactionHash);

            if (existingTransaction != null)
            {
                _logger.LogWarning($"Transaction with hash {transactionHash} already exists in the database");
                throw new UserFriendlyException($"Transaction with hash {transactionHash} has already been processed. Please use a different transaction.");
            }

            int amount = (int)(decimalAmount / TOKEN_DECIMAL_FACTOR);

            var userBalance = await WorkScope.GetAll<UserPunishmentBalance>()
                .FirstOrDefaultAsync(b => b.UserId == _abpSession.UserId.Value);
            
            var totalPunishmentMoney = userBalance?.TotalPunishmentMoney ?? 0;
            var remainPoints = userBalance?.RemainPoints ?? 0;
            var requiredAmount = Math.Max(0, totalPunishmentMoney - remainPoints);
            
            if (amount < requiredAmount)
            {
                _logger.LogError($"Transaction amount {amount} is less than required amount {requiredAmount} (TotalPunishmentMoney: {totalPunishmentMoney} - RemainPoints: {remainPoints}) for user {_abpSession.UserId.Value}");
                throw new UserFriendlyException($"Transaction amount ({amount:N0} VNĐ) is insufficient to cover required payment ({requiredAmount:N0} VNĐ). Your current balance: {totalPunishmentMoney:N0} VNĐ penalty - {remainPoints:N0} reward points.");
            }

            var userPunishmentPaid = new UserPunishmentPaid
            {
                UserId = _abpSession.UserId.Value,
                DateAt = DateTimeOffset.FromUnixTimeSeconds(transactionInfo.TransactionTimestamp).DateTime,
                TargetMonth = targetMonthDate, 
                Amount = amount,
                TxHash = transactionInfo.Hash
            };

            try
            {
                await _userPunishmentPaidRepository.InsertAsync(userPunishmentPaid);

                await MarkPunishmentsAsPaid(_abpSession.UserId.Value, targetMonthDate);

                await RecalculateUserPunishmentBalanceWithRemainPoints(_abpSession.UserId.Value, amount);

                return true;
            }
            catch (Exception ex)
            {
                if (ex is UserFriendlyException)
                    throw;
                    
                _logger.LogError(ex, $"Error marking transaction {transactionHash} as paid");
                return false;
            }
        }

        private async Task<MMNTransactionInfo> GetMMNTransactionsInfo(string transactionHash)
        {
            try
            {
                HttpClientHandler clientHandler = new HttpClientHandler();
                clientHandler.ServerCertificateCustomValidationCallback = (sender, cert, chain, sslPolicyErrors) => { return true; };
                
                using (var httpClient = new HttpClient(clientHandler))
                {
                    var url = $"{_indexerUri}/1337/tx/{transactionHash}/detail";
                    _logger.LogInformation($"Calling MMN API: {url}");
                    
                    httpClient.DefaultRequestHeaders.Accept.Clear();
                    httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                    
                    var response = await httpClient.GetAsync(url);

                    if (!response.IsSuccessStatusCode)
                    {
                        _logger.LogError($"Failed to get transaction info. Status code: {response.StatusCode}");
                        throw new UserFriendlyException($"Failed to verify transaction. Status code: {response.StatusCode}");
                    }

                    var jsonString = await response.Content.ReadAsStringAsync();
                    _logger.LogInformation($"MMN API Response: {jsonString}");
                    
                    var transactionResponse = JsonConvert.DeserializeObject<MMNTransactionResponse>(jsonString);

                    if (transactionResponse?.Data?.Transaction == null)
                    {
                        _logger.LogError($"Invalid transaction response format for hash: {transactionHash}");
                        throw new UserFriendlyException($"Invalid transaction format. The transaction may not exist or has an unsupported format.");
                    }

                    _logger.LogInformation($"Successfully retrieved transaction info for hash: {transactionHash}");
                    return new MMNTransactionInfo
                    {
                        Hash = transactionResponse.Data.Transaction.Hash,
                        Value = transactionResponse.Data.Transaction.Value,
                        TransactionTimestamp = transactionResponse.Data.Transaction.TransactionTimestamp,
                        FromAddress = transactionResponse.Data.Transaction.FromAddress,
                        ToAddress = transactionResponse.Data.Transaction.ToAddress,
                        Status = transactionResponse.Data.Transaction.Status
                    };
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error getting transaction info for hash: {transactionHash}");
                throw new UserFriendlyException($"Error verifying transaction: {ex.Message}");
            }
        }

        private async Task MarkPunishmentsAsPaid(long userId, DateTime targetMonth)
        {
            var endOfMonth = targetMonth.AddMonths(1);
            
            var unpaidPunishments = await WorkScope.GetAll<UserPunishment>()
                .Where(p => p.UserId == userId)
                .Where(p => p.DateAt >= targetMonth && p.DateAt < endOfMonth)
                .Where(p => !p.IsPaid)
                .ToListAsync();

            foreach (var punishment in unpaidPunishments)
            {
                punishment.IsPaid = true;
                await WorkScope.UpdateAsync(punishment);
            }

            _logger.LogInformation($"Marked {unpaidPunishments.Count} punishments as paid for user {userId} in {targetMonth:yyyy-MM}");
        }

        private async Task RecalculateUserPunishmentBalanceWithRemainPoints(long userId, int hashAmount)
        {
            var balance = await WorkScope.GetAll<UserPunishmentBalance>()
                .FirstOrDefaultAsync(b => b.UserId == userId);

            if (balance == null)
            {
                balance = new UserPunishmentBalance
                {
                    UserId = userId,
                    TotalPunishmentMoney = 0,
                    RemainPoints = 0
                };
                await WorkScope.InsertAsync(balance);
                _logger.LogInformation($"Created new balance for user {userId} with TotalPunishmentMoney = 0");
                return;
            }

            var currentTotalPunishmentMoney = balance.TotalPunishmentMoney;
            var currentRemainPoints = balance.RemainPoints;

            if (currentRemainPoints > 0 && currentTotalPunishmentMoney > 0)
            {
                var remainPointsUsed = Math.Min(currentRemainPoints, currentTotalPunishmentMoney);
                balance.RemainPoints -= remainPointsUsed;
                balance.TotalPunishmentMoney -= remainPointsUsed;
                
                _logger.LogInformation($"Applied {remainPointsUsed} RemainPoints for user {userId}. RemainPoints: {currentRemainPoints} -> {balance.RemainPoints}, TotalPunishmentMoney: {currentTotalPunishmentMoney} -> {balance.TotalPunishmentMoney}");

                var refundRecord = new UserPunishmentRefund
                {
                    UserId = userId,
                    UserPunishmentId = null,
                    Points = remainPointsUsed,
                    Type = PointType.IsUse
                };
                await WorkScope.InsertAsync(refundRecord);
            }

            if (balance.TotalPunishmentMoney > 0)
            {
                var beforeHashAmount = balance.TotalPunishmentMoney;
                balance.TotalPunishmentMoney = Math.Max(0, balance.TotalPunishmentMoney - hashAmount);
                
                _logger.LogInformation($"Applied hash amount {hashAmount} for user {userId}. TotalPunishmentMoney: {beforeHashAmount} -> {balance.TotalPunishmentMoney}");
            }
            
            await WorkScope.UpdateAsync(balance);
            _logger.LogInformation($"Final balance for user {userId}: TotalPunishmentMoney = {balance.TotalPunishmentMoney}, RemainPoints = {balance.RemainPoints}");
        }

        public async Task<int> GetTotalRemainPointsUsedInMonth(long userId, int year, int month)
        {
            var targetMonth = new DateTime(year, month, 1);
            var startOfMonth = targetMonth;
            var endOfMonth = targetMonth.AddMonths(1);
            
            _logger.LogInformation($"GetTotalRemainPointsUsedInMonth - Year: {year}, Month: {month}, StartOfMonth: {startOfMonth:yyyy-MM-dd HH:mm:ss}, EndOfMonth: {endOfMonth:yyyy-MM-dd HH:mm:ss}");

            return await WorkScope.GetAll<UserPunishmentRefund>()
                .Where(r => r.UserId == userId 
                    && r.CreationTime >= startOfMonth 
                    && r.CreationTime < endOfMonth
                    && r.Type == PointType.IsUse)
                .SumAsync(r => r.Points);
        }

        public async Task<int> GetTotalPaidPunishmentInMonth(long userId, int year, int month)
        {
            var targetMonth = new DateTime(year, month, 1);
            var endOfMonth = targetMonth.AddMonths(1);

            var totalPaid = await WorkScope.GetAll<UserPunishment>()
                .Where(p => p.UserId == userId 
                    && !p.IsDeleted 
                    && p.IsPaid == true
                    && p.DateAt >= targetMonth 
                    && p.DateAt < endOfMonth)
                .SumAsync(p => p.TotalMoney);

            return totalPaid;
        }

        public async Task<UserPunishmentSummaryDto> PreviewApplyAndGetSummaryAsync(int year, int month)
        {
            var result = new UserPunishmentSummaryDto();

            try
            {
                if (!_abpSession.UserId.HasValue)
                {
                    result.Success = false;
                    result.Message = "User not logged in";
                    return result;
                }

                var userId = _abpSession.UserId.Value;
                _logger.LogInformation($"PreviewApplyAndGetSummary: User {userId}, {year}/{month}");

                var targetMonth = new DateTime(year, month, 1);
                var startOfMonth = targetMonth;
                var endOfMonth = targetMonth.AddMonths(1);

                var balance = await WorkScope.GetAll<UserPunishmentBalance>()
                    .Where(b => b.UserId == userId)
                    .FirstOrDefaultAsync();

                var currentTotalPunishmentMoney = balance?.TotalPunishmentMoney ?? 0;
                var currentRemainPoints = balance?.RemainPoints ?? 0;

                bool canPayAll = currentRemainPoints >= currentTotalPunishmentMoney;
                bool usedRemainPoints = false;

                if (canPayAll && currentTotalPunishmentMoney > 0)
                {
                    int newTotalPunishmentMoney = 0; 
                    int newRemainPoints = currentRemainPoints - currentTotalPunishmentMoney;

                    if (balance == null)
                    {
                        balance = new UserPunishmentBalance
                        {
                            UserId = userId,
                            TotalPunishmentMoney = newTotalPunishmentMoney,
                            RemainPoints = newRemainPoints
                        };
                        await WorkScope.InsertAsync(balance);
                    }
                    else
                    {
                        balance.TotalPunishmentMoney = newTotalPunishmentMoney;
                        balance.RemainPoints = newRemainPoints;
                        await WorkScope.UpdateAsync(balance);
                    }

                    _logger.LogInformation($"Updated UserPunishmentBalance: {currentTotalPunishmentMoney} - {currentRemainPoints} = {newTotalPunishmentMoney}, RemainPoints: {currentRemainPoints} - {currentTotalPunishmentMoney} = {newRemainPoints}");

                    var refundRecord = new UserPunishmentRefund
                    {
                        UserId = userId,
                        UserPunishmentId = null, 
                        Points = currentTotalPunishmentMoney,
                        Type = PointType.IsUse
                    };
                    await WorkScope.InsertAsync(refundRecord);
                    _logger.LogInformation($"Recorded {currentTotalPunishmentMoney} RemainPoints usage for user {userId}");

                    var unpaidPunishmentsAll = await WorkScope.GetAll<UserPunishment>()
                        .Where(p => p.UserId == userId 
                            && !p.IsDeleted 
                            && (p.IsPaid == false || p.IsPaid == null)
                            && p.DateAt >= startOfMonth 
                            && p.DateAt < endOfMonth)
                        .ToListAsync();

                    foreach (var punishment in unpaidPunishmentsAll)
                    {
                        punishment.IsPaid = true;
                        await WorkScope.UpdateAsync(punishment);
                    }
                    usedRemainPoints = true;
                }
                else
                {
                    _logger.LogInformation($"RemainPoints ({currentRemainPoints}) not enough to cover TotalPunishmentMoney ({currentTotalPunishmentMoney}). No changes made for user {userId} in {year}/{month}");
                }

                var updatedBalance = await WorkScope.GetAll<UserPunishmentBalance>()
                    .Where(b => b.UserId == userId)
                    .FirstOrDefaultAsync();

                result.UserBalance = new GetUserPunishmentBalanceDto
                {
                    TotalPunishmentMoney = updatedBalance?.TotalPunishmentMoney ?? 0,
                    RemainPoints = updatedBalance?.RemainPoints ?? 0
                };

                result.TotalRemainPointsUsedInMonth = await GetTotalRemainPointsUsedInMonth(userId, year, month);

                result.TotalPaidPunishmentInMonth = await GetTotalPaidPunishmentInMonth(userId, year, month);

                result.Success = true;
                result.Message = usedRemainPoints
                    ? $"Successfully applied {currentTotalPunishmentMoney:N0} reward points. All penalties paid."
                    : "No reward points were used";

                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error in PreviewApplyAndGetSummary: {ex.Message}", ex);
                result.Success = false;
                result.Message = "An error occurred while processing punishment points";
                return result;
            }
        }
    }
}
