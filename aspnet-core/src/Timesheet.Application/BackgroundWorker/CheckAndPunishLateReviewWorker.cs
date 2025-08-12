
using Abp.Configuration;
using Abp.Dependency;
using Abp.Domain.Repositories;
using Abp.Domain.Uow;
using Abp.Threading.BackgroundWorkers;
using Abp.Threading.Timers;
using Abp.UI;
using Microsoft.Extensions.Logging;
using Ncc.Configuration;
using System;
using System.Linq;
using Timesheet.APIs.ReviewInterns;
using Timesheet.DomainServices;
using Timesheet.DomainServices.Dto;
using Timesheet.Entities;
using Timesheet.Uitls;

namespace Timesheet.BackgroundWorker
{
    public class CheckAndPunishLateReviewWorker : PeriodicBackgroundWorkerBase, ISingletonDependency
    {
        private readonly ILogger<CheckAndPunishLateReviewWorker> _logger;
        private readonly IIocResolver _iocResolver;

        public CheckAndPunishLateReviewWorker(
            AbpTimer timer,
            ILogger<CheckAndPunishLateReviewWorker> logger,
            IIocResolver iocResolver)
            : base(timer)
        {
            _logger = logger;
            _iocResolver = iocResolver;

            Timer.RunOnStart = true;
            Timer.Period = (int)TimeSpan.FromHours(1).TotalMilliseconds;
            Timer.Elapsed += Timer_Elapsed;
        }

        [UnitOfWork]
        protected override void DoWork()
        {
            using (var reviewInternService = _iocResolver.ResolveAsDisposable<IReviewInternServices>())
            using (var reviewInternRepository = _iocResolver.ResolveAsDisposable<IRepository<ReviewIntern, long>>())
            {
                string enable = SettingManager.GetSettingValueForApplication(AppSettingNames.ReviewEnableWorker);
                if (enable != "True")
                {
                    _logger.LogInformation("CheckAndPunishLateReviewWorker skipped: Disabled via settings.");
                    return;
                }

                var now = DateTimeUtils.GetNow();   
                var previousMonthDate = now.AddMonths(-1);
                int month = previousMonthDate.Month;
                int year = previousMonthDate.Year;
                var input = new ReviewInternsDto { Month = month, Year = year };

                DateTime deadlineDate = CalculateDeadlineDate(input);

                if (now.Date < deadlineDate.Date)
                {
                    _logger.LogInformation(
                        $"Deadline not reached yet ({now:dd/MM/yyyy} ≤ {deadlineDate:dd/MM/yyyy}); scheduled to retry at 00:00 on {deadlineDate:dd/MM/yyyy}.");

                    var nextRun = deadlineDate.Date;
                    Timer.Period = (int)(nextRun - now).TotalMilliseconds;
                    return;
                }

                _logger.LogInformation($"Starting to apply late review penalties for {month}/{year}.");
                var processed = false;
                var reviewIntern = reviewInternRepository.Object.GetAll()
                .FirstOrDefault(x => x.Month == month && x.Year == year);

                if (reviewIntern == null)
                {
                    _logger.LogWarning($"No review intern period found for {month}/{year}.");
                }
                else
                {
                    processed = reviewIntern.IsPunishmentProcessed;

                    if (processed)
                    {
                        _logger.LogInformation($"Completed processing late review penalties for {month}/{year}.");
                    }
                    else
                    {
                        try
                        {
                            var result = reviewInternService.Object.CheckAndPunishLateReview(input).Result;

                            reviewIntern.IsPunishmentProcessed = true;
                            reviewInternRepository.Object.Update(reviewIntern);
                            CurrentUnitOfWork.SaveChanges();
                        }
                        catch (UserFriendlyException ufex)
                        {
                            _logger.LogWarning(ufex.Message);
                        }
                        catch (AggregateException agex) when (agex.InnerException is UserFriendlyException)
                        {
                            _logger.LogWarning(agex.InnerException.Message);
                        }
                        catch (Exception ex)
                        {
                            _logger.LogError(ex, "Failed to process late review penalties.");
                        }
                    }
                }

                try
                {
                    Timer.Period = GetPeriodToNextReviewDate();
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error setting timer period. Using default period of 24 hours.");
                    Timer.Period = (int)TimeSpan.FromHours(24).TotalMilliseconds;
                }
            }
        }

        private void Timer_Elapsed(object sender, System.EventArgs e)
        {
            try
            {
                DoWork();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Failed in Timer_Elapsed: {ex.Message}");
            }
        }

        private DateTime CalculateDeadlineDate(ReviewInternsDto input)
        {
            var deadlineDay = int.Parse(SettingManager.GetSettingValueForApplication(AppSettingNames.ReviewDeadlineDay));
            var daysToAdd = int.Parse(SettingManager.GetSettingValueForApplication(AppSettingNames.ReviewDeadlineDaysToAdd));
            var startDay = int.Parse(SettingManager.GetSettingValueForApplication(AppSettingNames.ReviewStartDayOfMonth));
            var startDate = new DateTime(input.Year, input.Month, startDay);
            var endDate = startDate.AddDays(daysToAdd);

            int weekendDays = 0;
            for (var date = startDate; date <= endDate; date = date.AddDays(1))
            {
                if (date.DayOfWeek == DayOfWeek.Saturday || date.DayOfWeek == DayOfWeek.Sunday)
                {
                    weekendDays++;
                }
            }

            deadlineDay += weekendDays;
            var deadlineDate = startDate.AddDays(deadlineDay - 1);

            while (deadlineDate.DayOfWeek == DayOfWeek.Saturday || deadlineDate.DayOfWeek == DayOfWeek.Sunday)
            {
                deadlineDate = deadlineDate.AddDays(1);
            }

            return deadlineDate;
        }

        private int GetPeriodToNextReviewDate()
        {
            try
            {
                var nextRunDate = int.Parse(SettingManager.GetSettingValueForApplication(AppSettingNames.ReviewNextRunDate));
                var now = DateTimeUtils.GetNow();
                var nextRun = new DateTime(now.Year, now.Month, nextRunDate, 0, 0, 0);
                if (now > nextRun)
                {
                    nextRun = nextRun.AddMonths(1);
                }

                var milliseconds = (nextRun - now).TotalMilliseconds;

                if (milliseconds > int.MaxValue)
                {
                    const int fixedPeriodDays = 15;

                    _logger.LogInformation($"Calculated period ({milliseconds} ms) exceeds Int32.MaxValue. Next run scheduled after {fixedPeriodDays} days.");
                    return (int)TimeSpan.FromDays(fixedPeriodDays).TotalMilliseconds;
                }

                return (int)milliseconds;
            }
            catch (Exception ex)
            {
                const int defaultPeriodDays = 15;
                _logger.LogError(ex, $"Error calculating next review period. Using default period of {defaultPeriodDays} days.");
                return (int)TimeSpan.FromDays(defaultPeriodDays).TotalMilliseconds;
            }
        }
    }
}
