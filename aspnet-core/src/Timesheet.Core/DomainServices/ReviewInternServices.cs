using Abp.Configuration;
using Abp.Dependency;
using Abp.Timing;
using Abp.UI;
using Microsoft.EntityFrameworkCore;
using Ncc.Authorization.Users;
using Ncc.Configuration;
using Ncc.IoC;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using Timesheet.DomainServices.Dto;
using Timesheet.Entities;
using Timesheet.Uitls;
using static Ncc.Entities.Enum.StatusEnum;

namespace Timesheet.DomainServices
{
    public class ReviewInternServices : BaseDomainService, IReviewInternServices, ITransientDependency
    {
        private readonly ISettingManager _settingManager;
        public ReviewInternServices(
          IWorkScope workScope,
          ISettingManager settingManager) : base(workScope)
        {
            _settingManager = settingManager;
        }
        public long LastIdReviewIntern()
        {
            return WorkScope.GetAll<ReviewIntern>().
                Where(s => s.IsActive).
                OrderByDescending(s => s.Id).
                Select(s => s.Id).FirstOrDefault();
        }

        public List<NotifyReviewInternDto> GetListPmNotReview(long reviewId)
        {
            var PMsNotReview = WorkScope.GetAll<ReviewDetail>().
                                     Include(s => s.Reviewer).
                                     Where(s => s.ReviewId == reviewId).
                                     Where(s => s.Status == ReviewInternStatus.Draft || s.Status == ReviewInternStatus.Rejected)
                                    .GroupBy(ts => new
                                    {
                                        ts.Reviewer.EmailAddress,
                                        ts.Reviewer.KomuUserId,
                                    })
                                    .Select(s => new NotifyReviewInternDto
                                    {
                                        EmailAddress = s.Key.EmailAddress,
                                        KomuUserId = s.Key.KomuUserId,
                                        StartWorkingAt = s.First().Reviewer.MorningStartAt,
                                        EndWorkingAt = s.First().Reviewer.AfternoonEndAt,
                                        InterShips = s.Select(x => new NotifyUserInfoDto
                                        {
                                            FullName = x.InterShip.FullName,
                                            BranchDisplayName = x.InterShip.Branch.DisplayName,
                                            Level = x.InterShip.Level,
                                        }).ToList()
                                    }).ToList();
            return PMsNotReview;
        }

        public async Task<LateReviewPunishmentResultDto> CheckAndPunishLateReview(ReviewInternsDto input)
        {
         var review = await WorkScope.GetAll<ReviewIntern>()
                .Where(x => x.Month == input.Month && x.Year == input.Year && x.IsActive)
                .FirstOrDefaultAsync();

            if (review == null)
            {
                throw new UserFriendlyException($"There are no active intern review periods for the current month {input.Month}/{input.Year}");
            }

            var now = Clock.Now;
            var currentDate = now.Date;

            var deadlineDay = int.Parse(SettingManager.GetSettingValueForApplication(AppSettingNames.ReviewDeadlineDay));
            var startDay = int.Parse(SettingManager.GetSettingValueForApplication(AppSettingNames.ReviewStartDayOfMonth));
            var startDate = new DateTime(DateTime.Now.Year, DateTime.Now.Month, startDay);
            var endDate = startDate.AddDays(deadlineDay - 1);

            int weekendDays = 0;
            for (var date = startDate; date <= endDate; date = date.AddDays(1))
            {
                if (date.DayOfWeek == DayOfWeek.Saturday || date.DayOfWeek == DayOfWeek.Sunday)
                {
                    weekendDays++;
                }
            }

            deadlineDay += weekendDays;
            var deadlineDate = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1).AddDays(deadlineDay - 1);

            if (deadlineDate.DayOfWeek == DayOfWeek.Sunday)
            {
                deadlineDate = deadlineDate.AddDays(1);
            }

            if (currentDate < deadlineDate)
            {
                throw new UserFriendlyException($"The penalty check is not yet due. The deadline is on {deadlineDate:dd/MM/yyyy}");
            }

            review.IsPunishmentProcessed = true;
            await WorkScope.UpdateAsync(review);

            var unreviewedPMs = await WorkScope.GetAll<ReviewDetail>()
                .Include(x => x.Reviewer)
                .Where(x => x.ReviewId == review.Id)
                .Where(x => x.Status == ReviewInternStatus.Draft || x.Status == ReviewInternStatus.Rejected)
                .GroupBy(x => new { x.ReviewerId, x.Reviewer.FullName, x.Reviewer.EmailAddress })
                .Select(g => new
                {
                    UserId = g.Key.ReviewerId.Value,
                    g.Key.FullName,
                    g.Key.EmailAddress,
                    UnreviewedCount = g.Count()
                })
                .ToListAsync();

            if (!unreviewedPMs.Any())
            {
                return new LateReviewPunishmentResultDto
                {
                    TotalPunishedPMs = 0,
                    TotalPunishmentAmount = 0,
                    PunishedPMs = new List<PunishedPMDto>()
                };
            }

            var punishmentType = UserPunishmentType.ReviewIntern;
            var punishmentSystem = await WorkScope.GetAll<PunishmentSystem>()
                .Where(x => x.Type == punishmentType && x.IsActive)
                .OrderByDescending(x => x.CreationTime)
                .FirstOrDefaultAsync();

            if (punishmentSystem == null)
            {
                throw new UserFriendlyException("No penalty configuration found for late review");
            }

            var punishments = await WorkScope.GetAll<UserPunishment>()
                .Where(x => x.DateAt.Year == input.Year && x.DateAt.Month == input.Month)
                .Where(x => x.Type == punishmentType)
                .ToListAsync();

            punishments.ForEach(p =>
            {
                p.IsDeleted = true;
                p.DeletionTime = DateTimeUtils.GetNow();
            });

            await CurrentUnitOfWork.SaveChangesAsync();

            var result = new LateReviewPunishmentResultDto
            {
                PunishedPMs = new List<PunishedPMDto>(),
                TotalPunishedPMs = 0,
                TotalPunishmentAmount = 0
            };

            foreach (var pm in unreviewedPMs)
            {
                var existingPunishment = await WorkScope.GetAll<UserPunishment>()
                    .Where(x => x.UserId == pm.UserId)
                    .Where(x => x.DateAt.Year == input.Year && x.DateAt.Month == input.Month)
                    .Where(x => x.Type == punishmentType)
                    .FirstOrDefaultAsync();

                if (existingPunishment != null)
                {
                    continue;
                }

                var punishment = new UserPunishment
                {
                    UserId = pm.UserId,
                    DateAt = now,
                    PunishmentSystemId = punishmentSystem.Id,
                    Type = punishmentType,
                    Count = 1,
                    TotalMoney = punishmentSystem.Money
                };

                await WorkScope.InsertAndGetIdAsync(punishment);

                result.PunishedPMs.Add(new PunishedPMDto
                {
                    UserId = pm.UserId,
                    FullName = pm.FullName,
                    Email = pm.EmailAddress,
                    TotalUnreviewedInterns = pm.UnreviewedCount,
                    PunishmentAmount = punishmentSystem.Money
                });

                result.TotalPunishedPMs++;
                result.TotalPunishmentAmount += punishmentSystem.Money;
            }

            await CurrentUnitOfWork.SaveChangesAsync();
            return result;
        }
    }
}
