using Abp.Application.Services;
using Abp.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Ncc.Authorization.Users;
using Ncc.Configuration;
using Ncc.IoC;
using System;
using System.Linq;
using Timesheet.Entities;
using Timesheet.NCCAuthen;
using Timesheet.Uitls;
using static Ncc.Entities.Enum.StatusEnum;

namespace Timesheet.Timesheets.External
{
    public class ExternalAppService : ApplicationService
    {
        protected IWorkScope WorkScope { get; set; }
        public ExternalAppService(IWorkScope workScope)
        {
            WorkScope = workScope;
        }
        [HttpPost]
        [AbpAllowAnonymous]
        [NccAuthentication]
        public async System.Threading.Tasks.Task RejectTimesheetOpenTalk(string[] emailAddress)
        {
            var UserIdList = WorkScope.GetAll<User>().Where(s => emailAddress.Contains(s.EmailAddress)).Select(s => s.Id);
            var OpenTalkProjectTaskId = Convert.ToInt64(await SettingManager.GetSettingValueAsync(AppSettingNames.ProjectTaskId));
            var OpenTalkIds = await WorkScope.GetAll<MyTimesheet>().Where(s => DateTimeUtils.FirstDayOfCurrentyWeek() <= s.DateAt && s.DateAt <= DateTimeUtils.LastDayOfCurrentWeek())
                    .Where(s => s.ProjectTaskId == OpenTalkProjectTaskId && s.Status != TimesheetStatus.None && UserIdList.Contains(s.UserId)).ToListAsync();
            OpenTalkIds.ForEach(s => s.Status = TimesheetStatus.Reject);
            await WorkScope.UpdateRangeAsync(OpenTalkIds);
        }
    }
}
