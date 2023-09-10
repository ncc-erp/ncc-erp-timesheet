using Abp.Configuration;
using Abp.Dependency;
using Abp.Domain.Repositories;
using Abp.Domain.Uow;
using Abp.Threading.BackgroundWorkers;
using Abp.Threading.Timers;
using Ncc.Configuration;
using System;
using System.Linq;
using Timesheet.Entities;
using Timesheet.Uitls;
using static Ncc.Entities.Enum.StatusEnum;

namespace Timesheet.BackgroundWorker
{
    //public class SubmitTimesheet : PeriodicBackgroundWorkerBase, ISingletonDependency
    //{
    //    private readonly IRepository<MyTimesheet, long> _myTimesheetRepository;

    //    public SubmitTimesheet(AbpTimer timer, IRepository<MyTimesheet, long> myTimesheetRepository)
    //        : base(timer)
    //    {
    //        _myTimesheetRepository = myTimesheetRepository;
    //        Timer.Period = 1000*60*60; //1h
    //    }

    //    [UnitOfWork]
    //    protected override void DoWork()
    //    {
    //        //DateTime Now = DateTimeUtils.GetNow();
    //        //string AutoSubmitTSAt = SettingManager.GetSettingValueForApplication(AppSettingNames.AutoSubmitAt);
    //        //string autoSubmitTimesheet = SettingManager.GetSettingValueForApplication(AppSettingNames.AutoSubmitTimesheet);
    //        //DayOfWeek submitDay = Enum.Parse<DayOfWeek>(AutoSubmitTSAt);
            
    //        //DateTime monday = Now.AddDays(-(int)Now.DayOfWeek + (int)DayOfWeek.Monday);
    //        //DateTime submitDate = Now.AddDays(-(int)Now.DayOfWeek + (int)submitDay);
    //        ////DateTime date = DateTime.Now;

    //        //Logger.Info("SubmitTimesheet.DoWork() started |Now = " + Now.ToString("yyyy-MM-dd HH:mm:ss") + "| AutoSubmitTSAt = " + AutoSubmitTSAt + " |autoSubmitTimesheet=" + autoSubmitTimesheet + " |monday=" + monday + " |submitDate=" + submitDate);

    //        //if (autoSubmitTimesheet == "true"
    //        //    && (Now.DayOfWeek.ToString() == AutoSubmitTSAt && Now.Hour == 12)
    //        //    || (Now.Day == 1 && Now.Hour == 0))
    //        //{
    //        //    //Lấy ra các bản ghi từ đầu tuần chưa submit
    //        //    var timesheetLogs = _myTimesheetRepository
    //        //        .GetAllList()
    //        //        .Where(t => t.DateAt >= monday && t.DateAt <= submitDate && t.Status == TimesheetStatus.None)
    //        //        .ToList();

    //        //    foreach (var timesheet in timesheetLogs)
    //        //    {
    //        //        timesheet.Status = TimesheetStatus.Pending;
    //        //        timesheet.LastModificationTime = Now;
    //        //    }

    //        //    CurrentUnitOfWork.SaveChanges();
    //        //    Logger.Info("SubmitTimesheet.DoWork() end timesheetLogs.Count = " + timesheetLogs.Count);
    //        //}

    //    }
    //}
}
