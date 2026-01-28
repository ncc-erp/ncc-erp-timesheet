using Abp.Dependency;
using Abp.Domain.Repositories;
using Abp.Domain.Uow;
using Abp.Timing;
using Abp.UI;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Ncc.Authorization.Users;
using Ncc.Configuration;
using Ncc.IoC;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Timesheet.DomainServices.Dto;
using Timesheet.Entities;
using Timesheet.Services.Komu;
using Timesheet.Services.Project;
using Timesheet.Services.Project.Dto;
using Timesheet.Uitls;
using static Ncc.Entities.Enum.StatusEnum;

namespace Timesheet.DomainServices
{
    public class UserPunishmentServices : BaseDomainService, IUserPunishmentServices, ITransientDependency
    {
        private readonly ProjectService _projectService;
        private readonly KomuService _komuService;
        private readonly ILogger<UserPunishmentServices> _logger;

        public UserPunishmentServices(
            IWorkScope workScope,
            ProjectService projectService,
            KomuService komuService,
            ILogger<UserPunishmentServices> logger)
            : base(workScope)
        {
            _projectService = projectService;
            _komuService = komuService;
            _logger = logger;
        }

        [UnitOfWork]
        public async Task<List<PMReportItemDto>> ApplyPMReportPunishmentsAsync()
        {
            var selectedDate = Clock.Now.Date;
            
            var isOffDate = await WorkScope.GetAll<DayOffSetting>()
                .Where(s => s.DayOff.Date == selectedDate)
                .AnyAsync();

            if (isOffDate)
            {
                _logger.LogInformation($"{selectedDate:MM/dd/yyyy} is Off Date => Skip PM Report punishment");
                return new List<PMReportItemDto>();
            }
            
            var users = WorkScope.GetAll<User>()
              .Where(u => u.IsActive)
              .Select(u => new {
                  u.Id,
                  u.FullName,
                  u.EmailAddress
              })
              .ToList();

            var oldPunishments = await WorkScope.GetAll<UserPunishment>()
              .Where(p => p.DateAt.Date == selectedDate &&
                (p.Type == UserPunishmentType.PMReport_20k || p.Type == UserPunishmentType.PMReport_50k) &&
                !p.IsDeleted)
              .ToListAsync();

            if (oldPunishments.Any())
            {
                foreach (var p in oldPunishments)
                {
                    p.IsDeleted = true;
                    p.DeletionTime = DateTimeUtils.GetNow();
                }

                await CurrentUnitOfWork.SaveChangesAsync();
            }
            var pmReportData = _projectService.GetCurrentWeekPMReport();
            if (pmReportData == null ||
              (pmReportData.between3To5PM?.Count == 0 && pmReportData.after5PMOrMissing?.Count == 0))
            {
                _logger.LogWarning("GetCurrentWeekPMReport returned null or empty");
                return new List<PMReportItemDto>();
            }

            var listbetween3to5PM = pmReportData.between3To5PM.ToList();
            var listAfter5OrMissing = pmReportData.after5PMOrMissing.ToList();

            // Map of email addresses for PMs who submitted reports between 3-5 PM
            var mapBetween3To5PM = listbetween3to5PM.Select(x => x.emailAddress).Distinct().ToHashSet();

            // Map of email addresses for PMs who submitted reports after 5 PM or missed the deadline
            var mapAfter5PMOrMissing = listAfter5OrMissing.Select(x => x.emailAddress).Distinct().ToHashSet();

            var systems = await WorkScope.GetAll<PunishmentSystem>()
              .Where(x => (x.Type == UserPunishmentType.PMReport_20k || x.Type == UserPunishmentType.PMReport_50k) && x.IsActive)
              .ToDictionaryAsync(x => x.Type, x => x);

            if (!systems.ContainsKey(UserPunishmentType.PMReport_20k) || !systems.ContainsKey(UserPunishmentType.PMReport_50k))
                throw new UserFriendlyException("Punishment systems for PM report late not configured.");

            var pmReport20k = systems[UserPunishmentType.PMReport_20k];
            var pmReport50k = systems[UserPunishmentType.PMReport_50k];

            var punishments = new List<UserPunishment>();
            var punishedDtos = new List<PMReportItemDto>();

            foreach (var user in users)
            {
                if (mapAfter5PMOrMissing.Contains(user.EmailAddress))
                {
                    punishments.Add(new UserPunishment
                    {
                        DateAt = Clock.Now,
                        UserId = user.Id,
                        PunishmentSystemId = pmReport50k.Id,
                        Type = pmReport50k.Type,
                        Count = 1,
                        TotalMoney = pmReport50k.Money
                    });

                    punishedDtos.Add(new PMReportItemDto
                    {
                        userName = user.FullName,
                        emailAddress = user.EmailAddress,
                        money = pmReport50k.Money
                    });
                }
                else if (mapBetween3To5PM.Contains(user.EmailAddress))
                {
                    punishments.Add(new UserPunishment
                    {
                        DateAt = Clock.Now,
                        UserId = user.Id,
                        PunishmentSystemId = pmReport20k.Id,
                        Type = pmReport20k.Type,
                        Count = 1,
                        TotalMoney = pmReport20k.Money
                    });

                    punishedDtos.Add(new PMReportItemDto
                    {
                        userName = user.FullName,
                        emailAddress = user.EmailAddress,
                        money = pmReport20k.Money
                    });
                }
            }

            if (punishments.Any())
            {
                await WorkScope.InsertRangeAsync(punishments);
            }

            return punishedDtos;
        }

        public async Task<List<UserPunishment>> ApplyPMOtherPunishmentsAsync(int getPMOtherPunishmentAtMonth, int getPMOtherPunishmentAtYear)
        {
            try
            {
                var pmOtherRecords = await _projectService.GetAllPMOtherPunishment(getPMOtherPunishmentAtMonth, getPMOtherPunishmentAtYear);
                if (pmOtherRecords == null || !pmOtherRecords.Any())
                {
                    Logger.Info("Project tool returned no punishment records.");
                    return new List<UserPunishment>();
                }

                var pmOtherType = await WorkScope.GetAll<PunishmentSystem>()
                    .FirstOrDefaultAsync(x => x.Type == UserPunishmentType.PMOthers && x.IsActive);
                if (pmOtherType == null)
                {
                    throw new UserFriendlyException("Cannot get PM Other Punishment records because this Punishment Type is not found or inactive.");
                }

                var oldPMOtherPunishments = await WorkScope.GetAll<UserPunishment>()
                    .Where(p => p.DateAt.Month == getPMOtherPunishmentAtMonth && p.DateAt.Year == getPMOtherPunishmentAtYear)
                    .Where(p => p.Type == UserPunishmentType.PMOthers && !p.IsDeleted)
                    .GroupBy(p => $"{p.UserId}_{p.DateAt:yyyyMMdd}")
                    .ToDictionaryAsync(g => g.Key, g => g.First());

                var punishmentsFilteredByMezonId = pmOtherRecords
                    .Where(p => !string.IsNullOrEmpty(p.MezonId))
                    .GroupBy(p => p.MezonId)
                    .ToDictionary(g => g.Key, g => g.ToList());

                var punishmentsFilteredByEmail = pmOtherRecords
                    .Where(p => !string.IsNullOrEmpty(p.Email))
                    .GroupBy(p => p.Email)
                    .ToDictionary(g => g.Key, g => g.ToList());

                var targetMezonIds = punishmentsFilteredByMezonId.Keys.ToList();
                var targetEmails = punishmentsFilteredByEmail.Keys.ToList();

                var activeUsers = await WorkScope.GetAll<User>()
                    .Where(u => targetMezonIds.Contains(u.MezonUserId) || targetEmails.Contains(u.EmailAddress))
                    .Where(u => u.IsActive && !u.IsDeleted && !u.IsStopWork)
                    .Select(u => new { u.Id, u.MezonUserId, u.EmailAddress })
                    .ToListAsync();

                var userPunishmentsToInsert = new List<UserPunishment>();

                foreach (var user in activeUsers)
                {
                    var userPunishmentList = new List<PMOtherPunishmentDto>();
                    if (!string.IsNullOrEmpty(user.MezonUserId) && punishmentsFilteredByMezonId.TryGetValue(user.MezonUserId, out var listByMezonId))
                    {
                        userPunishmentList.AddRange(listByMezonId);
                    }
                    else if (!string.IsNullOrEmpty(user.EmailAddress) && punishmentsFilteredByEmail.TryGetValue(user.EmailAddress, out var listByEmail))
                    {
                        userPunishmentList.AddRange(listByEmail);
                    }

                    foreach (var record in userPunishmentList)
                    {
                        string keyToCheck = $"{user.Id}_{record.Date:yyyyMMdd}";
                        if (oldPMOtherPunishments.ContainsKey(keyToCheck))
                        {
                            continue;
                        }

                        var pmOtherPunishmentRecord = new UserPunishment
                        {
                            DateAt = record.Date,
                            UserId = user.Id,
                            PunishmentSystemId = pmOtherType.Id,
                            Type = pmOtherType.Type,
                            Count = 1,
                            TotalMoney = (int)record.Amount,
                            NoteReply = record.Reason,
                            IsPaid = false
                        };

                        userPunishmentsToInsert.Add(pmOtherPunishmentRecord);
                    }
                }

                await WorkScope.InsertRangeAsync(userPunishmentsToInsert);

                var adminClanName = await SettingManager.GetSettingValueForApplicationAsync(AppSettingNames.PMOtherPunishAdminClanName);
                DateTime now = DateTimeUtils.GetNow();
                var executionTime = now.ToString("HH:mm:ss");
                var executionDate = now.ToString("dd/MM/yyyy");

                string contentBody = userPunishmentsToInsert.Any()
                    ? $"Hệ thống đã lưu**{userPunishmentsToInsert.Count}**bản ghi thuộc loại phạt PM Others từ tool Project vào tool Timesheet."
                    : "Không có bản ghi phạt PM Others mới nào được thêm từ tool Project vào tool Timesheet.";

                string userMessage = $"Chào**{adminClanName}**\n" +
                             $"{contentBody}\n" +
                             $"Thời gian lấy dữ liệu phạt lúc {executionTime}, ngày {executionDate}";

                _komuService.SendSimpleNotificationToUser(userMessage, adminClanName);

                return userPunishmentsToInsert;
            }
            catch (Exception ex)
            {
                throw new UserFriendlyException($"Error: " + ex.Message);
            }
        }
    }
}
