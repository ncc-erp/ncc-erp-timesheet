using Abp.Dependency;
using Abp.Domain.Repositories;
using Abp.Domain.Uow;
using Abp.Timing;
using Abp.UI;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Ncc.Authorization.Users;
using Ncc.IoC;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Timesheet.DomainServices.Dto;
using Timesheet.Entities;
using Timesheet.Services.Project;
using Timesheet.Services.Project.Dto;
using Timesheet.Uitls;
using static Ncc.Entities.Enum.StatusEnum;

namespace Timesheet.DomainServices
{
    public class UserPunishmentServices : BaseDomainService, IUserPunishmentServices, ITransientDependency
    {
        private readonly ProjectService _projectService;
        private readonly ILogger<UserPunishmentServices> _logger;

        public UserPunishmentServices(
            IWorkScope workScope,
            ProjectService projectService,
            ILogger<UserPunishmentServices> logger)
            : base(workScope)
        {
            _projectService = projectService;
            _logger = logger;
        }

        [UnitOfWork]
        public async Task<List<PMReportItemDto>> ApplyPMReportPunishmentsAsync()
        {
            var users = WorkScope.GetAll<User>()
              .Where(u => u.IsActive)
              .Select(u => new {
                  u.Id,
                  u.FullName,
                  u.EmailAddress
              })
              .ToList();
            var selectedDate = Clock.Now.Date;

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

            var listFrom15To17 = pmReportData.between3To5PM.ToList();
            var listAfter17 = pmReportData.after5PMOrMissing.ToList();

            var map15To17 = listFrom15To17.Select(x => x.emailAddress).Distinct().ToHashSet();
            var mapAfter17 = listAfter17.Select(x => x.emailAddress).Distinct().ToHashSet();

            var systems = await WorkScope.GetAll<PunishmentSystem>()
              .Where(x => (x.Type == UserPunishmentType.PMReport_20k || x.Type == UserPunishmentType.PMReport_50k) && x.IsActive)
              .ToDictionaryAsync(x => x.Type, x => x);

            if (!systems.ContainsKey(UserPunishmentType.PMReport_20k) || !systems.ContainsKey(UserPunishmentType.PMReport_50k))
                throw new UserFriendlyException("Punishment systems for PM report late not configured.");

            var pm20k = systems[UserPunishmentType.PMReport_20k];
            var pm50k = systems[UserPunishmentType.PMReport_50k];

            var punishments = new List<UserPunishment>();
            var punishedDtos = new List<PMReportItemDto>();

            foreach (var user in users)
            {
                if (mapAfter17.Contains(user.EmailAddress))
                {
                    punishments.Add(new UserPunishment
                    {
                        DateAt = Clock.Now,
                        UserId = user.Id,
                        PunishmentSystemId = pm50k.Id,
                        Type = pm50k.Type,
                        Count = 1,
                        TotalMoney = pm50k.Money
                    });

                    punishedDtos.Add(new PMReportItemDto
                    {
                        userName = user.FullName,
                        emailAddress = user.EmailAddress,
                        money = pm50k.Money
                    });
                }
                else if (map15To17.Contains(user.EmailAddress))
                {
                    punishments.Add(new UserPunishment
                    {
                        DateAt = Clock.Now,
                        UserId = user.Id,
                        PunishmentSystemId = pm20k.Id,
                        Type = pm20k.Type,
                        Count = 1,
                        TotalMoney = pm20k.Money
                    });

                    punishedDtos.Add(new PMReportItemDto
                    {
                        userName = user.FullName,
                        emailAddress = user.EmailAddress,
                        money = pm20k.Money
                    });
                }
            }

            if (punishments.Any())
            {
                await WorkScope.InsertRangeAsync(punishments);
            }

            return punishedDtos;
        }
    }
}
