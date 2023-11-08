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
            DateTime time = now.AddMonths(-1);
            string getDataAtHour = SettingManager.GetSettingValueForApplication(AppSettingNames.CreateNewRetroAtHour);
            string getDataOnDate = SettingManager.GetSettingValueForApplication(AppSettingNames.CreateNewRetroOnDates);
            DateTime startDate = new DateTime(time.Year, time.Month, 1);
            DateTime endDate = startDate.AddMonths(1).AddDays(-1);
            DateTime deadline = startDate.AddMonths(1).AddDays(-1);
            RetroStatus status = RetroStatus.Public;
            string name = "Retro tháng " + time.Month + " - " + time.Year;
            var listRetroName = _workScope.GetAll<Retro>()
                            .Select(s => s.Name)
                            .ToList();
            if (DateTime.TryParse(getDataAtHour, out DateTime dataAt))
            {
                if (dataAt.Hour == now.Hour && dataAt.Minute == now.Minute 
                    && int.TryParse(getDataOnDate, out int date) 
                    && date == now.Day 
                    && !listRetroName.Contains(name))
                {
                    var data = new RetroCreateDto
                    {
                        Name = name,
                        StartDate = startDate,
                        EndDate = endDate,
                        Deadline = deadline,
                        Status = status
                    };
                    Logger.Info("CreateNewRetro.DoWork() getDataAt= " + getDataAtHour + ". now.Date= " + now.Date);
                    var newRetro = _retroAppService.Create(data);
                }
                else
                {
                    Logger.Info($"CreateNewRetro() now.Day ({now.Day}) != GenerateDataOnDate => stop");
                }
            }
            else
            {
                Logger.Error("Invalid format for getDataAtHour");
            }
        }

        private void GenerateRetroResult(DateTime now)
        {
            DateTime time = now.AddMonths(-1);
            string getDataAtHour = SettingManager.GetSettingValueForApplication(AppSettingNames.GenerateRetroResultAtHour);
            string getDataOnDate = SettingManager.GetSettingValueForApplication(AppSettingNames.GenerateRetroResultOnDates);
            long lastRetroId = _workScope.GetAll<Retro>()
                        .Where(s => s.Status == RetroStatus.Public)
                        .OrderByDescending(s => s.Id)
                        .Select(s => s.Id).FirstOrDefault();
            var listRetroResultInRetroId = _workScope.GetAll<RetroResult>()
                                            .Where(s => s.RetroId == lastRetroId)
                                            .ToList();
            if (lastRetroId != -1)
            {
                if(listRetroResultInRetroId.Count > 0) {
                    Logger.Info("retro result has been created");
                    return;
                }
                else
                {
                    if (DateTime.TryParse(getDataAtHour, out DateTime dataAt))
                    { 
                        if (dataAt.Hour == now.Hour 
                                && dataAt.Minute == now.Minute 
                                && int.TryParse(getDataOnDate, out int date) 
                                && date == now.Day)
                        {
                            var data = new InputGenerateDataRetroResultDto
                            {
                                Year = now.Year,
                                Month = time.Month,
                                RetroId = lastRetroId
                            };
                            Logger.Info("GenerateRetroResult.DoWork() getDataAt= " + ". now.Date= " + now.Date);
                            _retroResultAppService.GenerateDataRetroResult(data);
                        }
                        else
                        {
                            Logger.Info("date or hour is incorrect");
                            return;
                        }
                    }
                    else
                    {
                        Logger.Error("Invalid format for getDataAtHour");
                        return;
                    }
                }
            }
            else
            {
                Logger.Info("lastRetroId inValid");
                return;
            }
        }
    }
}
