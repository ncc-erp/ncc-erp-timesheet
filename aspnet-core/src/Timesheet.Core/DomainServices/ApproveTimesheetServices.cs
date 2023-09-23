using Abp.Configuration;
using Abp.Dependency;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Ncc.Authorization.Users;
using Ncc.Configuration;
using Ncc.Entities;
using Ncc.IoC;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Text;
using System.Threading.Tasks;
using Timesheet.DomainServices.Dto;
using Timesheet.Entities;
using static Ncc.Entities.Enum.StatusEnum;

namespace Timesheet.DomainServices
{
    public class ApproveTimesheetServices : BaseDomainService, IApproveTimesheetServices, ITransientDependency
    {
        class UserInfoTimesheetDto
        {
            public string FullName { get; set; }
            public string EmailAddress { get; set; }
            public ulong? KomuUserId { get; set; }
            public DateTime DateAt { get; set; }
            public long ProjectId { get; set; }
        }

        public List<NotifyApproveTimesheetDto> GetListPmNotApproveTimesheet()
        {
            double timePeriodWithPendingRequest = Double.Parse(SettingManager.GetSettingValueForApplication(AppSettingNames.ApproveTimesheetNotifyTimePeriodWithPendingRequest));
            var now = DateTime.Now;

            // lấy ra ngày thứ 30 trước Now hiện tại
            var thirtiethDayAgo = now.AddDays(timePeriodWithPendingRequest);

            var listTimesheetPending = WorkScope.GetAll<MyTimesheet>()
                .Where(s => s.Status == TimesheetStatus.Pending)
                .Where(s => s.CreationTime >= thirtiethDayAgo && s.CreationTime <= now
                || (s.LastModificationTime != null && s.LastModificationTime >= thirtiethDayAgo && s.LastModificationTime <= now))
                .Select(s => new UserInfoTimesheetDto
                {
                    FullName = s.User.FullName,
                    ProjectId = s.ProjectTask.ProjectId,
                    DateAt = s.DateAt
                })
                .Distinct();

            var listUser = listTimesheetPending.ToList();

            var listPm = WorkScope.GetAll<ProjectUser>()
                .Where(s => s.Type == ProjectUserType.PM)
                .Where(s => listTimesheetPending.Select(x => x.ProjectId).Contains(s.ProjectId))
                .Where(s => s.Project.Status == ProjectStatus.Active)
                .Select(s => new UserInfoTimesheetDto
                {
                    EmailAddress = s.User.EmailAddress,
                    KomuUserId = s.User.KomuUserId,
                    ProjectId = s.ProjectId
                })
                .Distinct()
                .ToList();

            var res = (from u in listUser
                       join pm in listPm on u.ProjectId equals pm.ProjectId
                       select new UserInfoTimesheetDto
                       {
                           EmailAddress = pm.EmailAddress,
                           KomuUserId = pm.KomuUserId,
                           FullName = u.FullName,
                           DateAt = u.DateAt,
                       }).GroupBy(s => new
                       {
                           s.EmailAddress,
                           s.KomuUserId,
                       })
                      .Select(s => new NotifyApproveTimesheetDto
                      {
                          EmailAddress = s.Key.EmailAddress,
                          KomuUserId = s.Key.KomuUserId,
                          Users = s.Select(x => new UserInfoApproveTimesheetDto
                          {
                              FullName = x.FullName,
                              DateAt = x.DateAt,
                          }).ToList(),
                      })
                      .ToList();
            return res;
        }
    }
}
