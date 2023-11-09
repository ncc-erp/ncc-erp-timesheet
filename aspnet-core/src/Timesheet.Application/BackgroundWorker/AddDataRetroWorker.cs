using Abp.Configuration;
using Abp.Domain.Uow;
using Abp.Threading.Timers;
using Ncc.Configuration;
using System;
using static Ncc.Entities.Enum.StatusEnum;
using Timesheet.APIs.RetroDetails;
using Timesheet.APIs.Retros.Dto;
using Timesheet.APIs.Retros;
using Timesheet.Uitls;
using Abp.Dependency;
using Abp.Threading.BackgroundWorkers;
using Timesheet.DomainServices.Dto;
using Ncc.IoC;
using Timesheet.Entities;
using System.Linq;

namespace Timesheet.BackgroundWorker
{
    public class AddDataRetroWorker: PeriodicBackgroundWorkerBase, ISingletonDependency
    {
        private readonly IWorkScope _workScope;
        private readonly RetroAppService _retroAppService;
        private readonly RetroResultAppService _retroResultAppService;
        public AddDataRetroWorker(AbpTimer timer,
             IWorkScope workScope,
            RetroResultAppService retroResultAppService,
            RetroAppService retroAppService
            ) : base(timer)
        {
            _workScope = workScope;
            _retroAppService = retroAppService;
            _retroResultAppService = retroResultAppService;
            Timer.Period = 1000 * 60 * 60;
        }

        [UnitOfWork]
        protected override void DoWork()
        {
            DateTime now = DateTimeUtils.GetNow();
            try
            {
                CreateNewRetro(now);
            }
            catch (Exception ex)
            {
                Logger.Error("CreateNewRetro() error: " + ex.Message);
            }
            try
            {
                GenerateRetroResult(now);
            }
            catch (Exception ex)
            {
                Logger.Error("CreateNewRetro() error: " + ex.Message);
            }
        }

        private void CreateNewRetro(DateTime now)
        {
            string createNewRetroEnableWorker = SettingManager.GetSettingValueForApplication(AppSettingNames.CreateNewRetroEnableWorker);
            string createNewRetroAtHour = SettingManager.GetSettingValueForApplication(AppSettingNames.CreateNewRetroAtHour);
            string createNewRetroOnDate = SettingManager.GetSettingValueForApplication(AppSettingNames.CreateNewRetroOnDate);

            if (createNewRetroEnableWorker != "true")
            {
                Logger.Error("CreateNewRetro() stop: createNewRetroEnableWorker = " + createNewRetroEnableWorker);
                return;
            }

            int.TryParse(createNewRetroAtHour, out int hour);
            if (hour != now.Hour)
            {
                Logger.Error("CreateNewRetro() stop: createNewRetroAtHour = " + createNewRetroAtHour);
                return;
            }

            int.TryParse(createNewRetroOnDate, out int date);
            if (date != now.Day)
            {
                Logger.Error("CreateNewRetro() stop: createNewRetroOnDate = " + createNewRetroOnDate);
                return;
            }

            DateTime time = now.AddMonths(-1);
            DateTime startDate = new DateTime(time.Year, time.Month, 1);
            DateTime endDate = startDate.AddMonths(1).AddDays(-1);
            DateTime deadline = startDate.AddMonths(1).AddDays(-1);
            RetroStatus status = RetroStatus.Public;
            string name = "Retro tháng " + time.Month + " - " + time.Year;
            var listRetroName = _workScope.GetAll<Retro>()
                .Select(s => new
                {
                    Name = s.Name,
                    IsDeleted = s.IsDeleted
                })
                .Where(s => s.IsDeleted == false)
                .Select(s => s.Name)
                .ToList();
            if (!listRetroName.Contains(name))
            {
                var data = new RetroCreateDto
                {
                    Name = name,
                    StartDate = startDate,
                    EndDate = endDate,
                    Deadline = deadline,
                    Status = status
                };
                Logger.Info("CreateNewRetro.DoWork() createNewRetroAt= " + createNewRetroAtHour + ". now.Date= " + now.Date);
                _retroAppService.Create(data);
            }
            else
            {
                Logger.Error("Retro has been created");
                return;
            }
        }

        private void GenerateRetroResult(DateTime now)
        {
            DateTime time = now.AddMonths(-1);
            string generateRetroResultEnableWorker = SettingManager.GetSettingValueForApplication(AppSettingNames.GenerateRetroResultEnableWorker);
            string getDataAtHour = SettingManager.GetSettingValueForApplication(AppSettingNames.GenerateRetroResultAtHour);
            string getDataOnDate = SettingManager.GetSettingValueForApplication(AppSettingNames.GenerateRetroResultOnDate);

            if (generateRetroResultEnableWorker != "true")
            {
                Logger.Error("GenerateRetroResult() stop: generateRetroResultEnableWorker = " + generateRetroResultEnableWorker);
                return;
            }

            int.TryParse(getDataAtHour, out int hour);
            if (hour != now.Hour)
            {
                Logger.Error("GenerateRetroResult() stop: generateRetroResultAtHour = " + getDataAtHour);
                return;
            }

            int.TryParse(getDataOnDate, out int date);
            if (date != now.Day)
            {
                Logger.Error("GenerateRetroResult() stop: generateRetroResultOnDate = " + getDataOnDate);
                return;
            }

            long lastRetroId = _workScope.GetAll<Retro>()
                .Where(s => s.Status == RetroStatus.Public)
                .OrderByDescending(s => s.Id)
                .Select(s => s.Id)
                .FirstOrDefault();

            var countRetroResultInRetroId = _workScope.GetAll<RetroResult>()
                .Where(s => s.RetroId == lastRetroId)
                .Count();

            if (countRetroResultInRetroId > 0)
            {
                Logger.Info("retro result has been created");
                return;
            }
            else
            {
                var data = new InputGenerateDataRetroResultDto
                {
                    Year = now.Year,
                    Month = time.Month,
                    RetroId = lastRetroId
                };
                Logger.Info("GenerateRetroResult.DoWork() getDataAt= " + getDataAtHour + ". now.Date= " + now.Date);
                _retroResultAppService.GenerateDataRetroResult(data);
            }
        }
    }
}
