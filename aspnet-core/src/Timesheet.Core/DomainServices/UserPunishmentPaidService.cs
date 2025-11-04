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

            var existingTransaction = await _userPunishmentPaidRepository
                .GetAll()
                .FirstOrDefaultAsync(x => x.TxHash == transactionHash);

            if (existingTransaction != null)
            {
                _logger.LogWarning($"Transaction with hash {transactionHash} already exists in the database");
                throw new UserFriendlyException($"Transaction with hash {transactionHash} has already been processed. Please use a different transaction.");
            }

            int amount = (int)(decimalAmount / TOKEN_DECIMAL_FACTOR);

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

                _logger.LogInformation($"Successfully marked transaction {transactionHash} as paid for user {_abpSession.UserId.Value}");
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
                        ToAddress = transactionResponse.Data.Transaction.ToAddress
                    };
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error getting transaction info for hash: {transactionHash}");
                throw new UserFriendlyException($"Error verifying transaction: {ex.Message}");
            }
        }
    }
}
