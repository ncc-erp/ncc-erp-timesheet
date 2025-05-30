using Abp.Configuration;
using Abp.Dependency;
using Abp.Domain.Uow;
using Abp.Threading.BackgroundWorkers;
using Abp.Threading.Timers;
using Ncc.Authorization.Users;
using Ncc.Configuration;
using Ncc.IoC;
using System;
using System.Linq;
using Timesheet.Entities;
using Timesheet.Services.Komu;
using Timesheet.Services.Mezon;
using Timesheet.Uitls;

namespace Timesheet.BackgroundWorker
{
    public class AddDataToOpenTalk : PeriodicBackgroundWorkerBase, ISingletonDependency
    {
        private readonly MezonService _mezonService;
        private readonly KomuService _komuService;
        private readonly IWorkScope _workScope;
        public AddDataToOpenTalk(AbpTimer timer, IWorkScope workScope, MezonService mezonService, KomuService komuService) : base(timer)
        {
            _mezonService = mezonService;
            _komuService = komuService;
            _workScope = workScope;
            Timer.Period = 1000 * 60 * 60;
            //Timer.Period = 1000 * 30;
        }
        [UnitOfWork]
        protected override void DoWork()
        {
            DateTime now = DateTimeUtils.GetNow();
            try
            {
                AddDataToOpenTalkBackground(now);
            }
            catch (Exception ex)
            {
                Logger.Error("AddDataToOpenTalkBackground() error: " + ex.Message);
            }
        }
        private void AddDataToOpenTalkBackground(DateTime now)
        {
            string addDataToOpenTalkEnable = SettingManager.GetSettingValueForApplication(AppSettingNames.AddDataToOpenTalkEnable);
            string addDataToOpenTalkAtHour = SettingManager.GetSettingValueForApplication(AppSettingNames.AddDataToOpenTalkAtHour);
            string addDataToOpenTalkAtDayOfWeek = SettingManager.GetSettingValueForApplication(AppSettingNames.AddDataToOpenTalkAtDayOfWeek);
            
            if (addDataToOpenTalkEnable != "True")
            {
                Logger.Error("AddDataToOpenTalkBackground() stop: addDataToOpenTalkEnable = " + addDataToOpenTalkEnable);
                return;
            }
            int.TryParse(addDataToOpenTalkAtHour, out int hour);
            if (hour != now.Hour)
            {
                Logger.Error("AddDataToOpenTalkBackground() stop: addDataToOpenTalkAtHour = " + addDataToOpenTalkAtHour);
                return;
            }
            if (addDataToOpenTalkAtDayOfWeek != now.DayOfWeek.ToString())
            {
                Logger.Error("AddDataToOpenTalkBackground() stop: addDataToOpenTalkAtDayOfWeek = " + addDataToOpenTalkAtDayOfWeek);
                return;
            }

            // TODO: cronjob run 10h Sunday
            DateTime yesterday = now.AddDays(-1);
            var OpenTalkListDto = _komuService.GetOpenTalkLogNew(yesterday);

            if (OpenTalkListDto != null)
            {
                var userDict = OpenTalkListDto.ToDictionary(s => s.email, s => s);
                var OpentalkList = _workScope.GetAll<User>().Where(s => s.UserName != null && userDict.ContainsKey(s.UserName))
                                            .Select(s => new OpenTalk
                                            {
                                                UserId = s.Id,
                                                DateAt = userDict[s.UserName].date,
                                                totalTime = userDict[s.UserName].totalTime
                                            }).ToList();

                foreach (var opentalk in OpentalkList)
                {
                    var dbRequest = _workScope.GetAll<OpenTalk>().Where(s => s.UserId == opentalk.UserId)
                                                                        .Where(s => s.DateAt.Date == opentalk.DateAt.Date)
                                                                        .FirstOrDefault();
                    if (dbRequest == null)
                    {
                        _workScope.Insert(opentalk);
                    }
                    else
                    {
                        dbRequest.totalTime = opentalk.totalTime;
                        _workScope.Update(dbRequest);
                    }
                }
            }
        }
    }
}
