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
    }
}
