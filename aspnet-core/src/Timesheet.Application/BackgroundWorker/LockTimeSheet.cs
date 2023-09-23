using Abp.Configuration;
using Abp.Dependency;
using Abp.Domain.Repositories;
using Abp.Domain.Uow;
using Abp.Threading.BackgroundWorkers;
using Abp.Threading.Timers;
using Abp.Timing;
using Ncc;
using Ncc.Authorization.Users;
using Ncc.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Timesheet.Entities;
using Timesheet.Uitls;
using static Ncc.Entities.Enum.StatusEnum;

namespace Timesheet.BackgroundWorker
{
    //public class LockTimeSheet : PeriodicBackgroundWorkerBase, ISingletonDependency
    //{
        //private readonly IRepository<UnlockTimesheet, long> _unlockTimesheetRepository;
        ////private readonly IRepository<MyTimesheet, long> _myTimesheetRepository;
       

        //public LockTimeSheet(AbpTimer timer, IRepository<UnlockTimesheet, long> unlockTimesheetRepository)
        //    : base(timer)
        //{
        //    _unlockTimesheetRepository = unlockTimesheetRepository;
        //    //this._myTimesheetRepository = _myTimesheetRepository;
        //    Timer.Period = 1000 * 60 *60;//1 hour
        //}

        //[UnitOfWork]
        //protected override void DoWork()
        //{
        //    //DateTime date = DateTimeUtils.GetNow();           
        //    //string lockDayAfterUnlock = SettingManager.GetSettingValueForApplication(AppSettingNames.LockDayAfterUnlock);
        //    //string lockHourAfterUnlock = SettingManager.GetSettingValueForApplication(AppSettingNames.LockHourAfterUnlock);
        //    //Logger.Info("LockTimeSheet.DoWork() started |date = " + date.ToString("yyyy-MM-dd HH:mm:ss") + "| lockDayAfterUnlock = " + lockDayAfterUnlock + " |LockHourAfterUnlock=" + lockHourAfterUnlock);


        //    //if (date.DayOfWeek.ToString() == lockDayAfterUnlock && date.Hour == int.Parse(lockHourAfterUnlock))
        //    //{
        //    //    var timesheetLogs = _unlockTimesheetRepository.GetAllList().ToList();

        //    //    foreach (var timesheet in timesheetLogs)
        //    //    {
        //    //        timesheet.IsDeleted = true;
        //    //        timesheet.DeletionTime = date;
        //    //    }
        //    //    CurrentUnitOfWork.SaveChanges();
        //    //    Logger.Info("LockTimeSheet.DoWork() end timesheetLogs.Count = " + timesheetLogs.Count);
        //    //}

        //}
    //}
}
