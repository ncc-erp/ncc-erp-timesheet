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
        private readonly IReviewInternServices _reviewInternService;
        private readonly IRepository<ReviewIntern, long> _reviewInternRepository;

        public CheckAndPunishLateReviewWorker(
            AbpTimer timer,
            ILogger<CheckAndPunishLateReviewWorker> logger,
            IReviewInternServices reviewInternService,
            IRepository<ReviewIntern, long> reviewInternRepository)
            : base(timer)
        {
            _logger = logger;
            _reviewInternService = reviewInternService;
            _reviewInternRepository = reviewInternRepository;

            Timer.RunOnStart = false;
            Timer.Period = (int)TimeSpan.FromHours(1).TotalMilliseconds; 
            Timer.Elapsed += Timer_Elapsed;
        }

        [UnitOfWork]
        protected override void DoWork()
        {
            var now = DateTimeUtils.GetNow();
            int month = now.Month;
            int year = now.Year;
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
            var processed = _reviewInternRepository.GetAll()
                .Where(x => x.Month == month && x.Year == year)
                .Select(x => x.IsPunishmentProcessed)
                .FirstOrDefault();

            if (processed)
            {
                _logger.LogInformation($"Completed processing late review penalties for {month}/{year}.");
            }
            else
            {
                try
                {
                    var result = _reviewInternService.CheckAndPunishLateReview(input).Result;

                    var reviewIntern = _reviewInternRepository.GetAll()
                        .FirstOrDefault(x => x.Month == month && x.Year == year);
                    if (reviewIntern != null)
                    {
                        reviewIntern.IsPunishmentProcessed = true;
                        _reviewInternRepository.Update(reviewIntern);
                        CurrentUnitOfWork.SaveChanges();
                    }
                }
                catch (UserFriendlyException ufex)
                {
                    _logger.LogWarning(ufex.Message);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to process late review penalties.");
                }
            }

            Timer.Period = GetPeriodToNextReviewDate();
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
            var nextRunDate = int.Parse(SettingManager.GetSettingValueForApplication(AppSettingNames.ReviewNextRunDate));
            var now = DateTimeUtils.GetNow();
            var nextRun = new DateTime(now.Year, now.Month, nextRunDate, 0, 0, 0);
            if (now > nextRun)
            {
                nextRun = nextRun.AddMonths(1);
            }
            return (int)(nextRun - now).TotalMilliseconds;
        }
    }
}
