using Abp.Configuration;
using Abp.Dependency;
using Microsoft.Office.Interop.Word;
using Ncc.Authorization.Users;
using Ncc.Configuration;
using Ncc.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Http.Results;
using Timesheet.DomainServices.Dto;
using Timesheet.Entities;
using Timesheet.Uitls;
using static Ncc.Entities.Enum.StatusEnum;

namespace Timesheet.DomainServices
{
    public class NotifyHRTheEmployeeMayHaveLeftServices : BaseDomainService, INotifyHRTheEmployeeMayHaveLeftServices, ITransientDependency
    {
        public NotifyHRTheEmployeeMayHaveLeftDto GetListEmployeeMayHaveLeft()
        {
            double timePeriod = Double.Parse(SettingManager.GetSettingValueForApplication(AppSettingNames.NotifyHRTheEmployeeMayHaveLeftTimePeriod));
            var dateNow = DateTimeUtils.GetNow();
            // lấy ra ngày thứ n trước Now hiện tại
            var startDate = dateNow.AddDays(timePeriod);

            var listUserIdCheckInAndCheckOut = WorkScope.GetAll<Timekeeping>()
                .Where(s => s.CheckIn != null || s.CheckOut != null)
                .Where(s => s.DateAt >= startDate.Date && s.DateAt.Date <= dateNow)
                .Select(s => s.UserId.Value)
                .Distinct()
                .ToList();

            var listUserIdRequest = WorkScope.GetAll<AbsenceDayDetail>()
                .Where(s => s.DateAt >= startDate.Date && s.DateAt.Date <= dateNow)
                .Select(s => s.Request.UserId)
                .Distinct()
                .ToList();

            var listUserIdLogTimesheet = WorkScope.GetAll<MyTimesheet>()
                .Where(s => s.DateAt >= startDate.Date && s.DateAt.Date <= dateNow)
                .Select(s => s.UserId)
                .Distinct()
                .ToList();

            var satisfyUserIds = listUserIdCheckInAndCheckOut.Concat(listUserIdRequest).Concat(listUserIdLogTimesheet).Distinct().ToList();

            var listUserInfoMayHaveLeft = WorkScope.GetAll<User>()
                .Where(s => s.IsActive && !s.IsStopWork)
                .Where(s => !satisfyUserIds.Contains(s.Id))
                .Select(s => new
                    {
                        UserId = s.Id,
                        FullName = s.FullName,
                        EmailAddress = s.EmailAddress,
                    }).ToList();

            var allProjectUsers = WorkScope.GetAll<ProjectUser>()
                .Where(s => s.Type != ProjectUserType.DeActive)
                .Where(s => s.Project.Status == ProjectStatus.Active);

            var projectIds = allProjectUsers
                .Where(s => listUserInfoMayHaveLeft.Select(u => u.UserId).Contains(s.UserId))
                .Where(s => !s.Project.isAllUserBelongTo)
                .Select(s => s.ProjectId)
                .Distinct()
                .ToList();

            var projectInfos = allProjectUsers
                .Where(s => projectIds.Contains(s.ProjectId))
                .Select(s => new
                {
                    ProjectId = s.ProjectId,
                    Type = s.Type,
                    UserId = s.UserId,
                    KomuUserId = s.User.KomuUserId,
                    EmailAddress = s.User.EmailAddress,
                    FullName = s.User.FullName,
                })
                .Distinct()
                .ToList();

            var dicProjectIdToPMs = projectInfos
                .Where(s => s.Type == ProjectUserType.PM)
                .GroupBy(s => s.ProjectId)
                .ToDictionary(s => s.Key,
                s => s.Select(x => new PMInfoNotifyHRTheEmployeeMayHaveLeftDto
                {
                    PMKomuUserId = x.KomuUserId,
                    PMEmailAddress = x.EmailAddress,
                }).ToList());

            var dicProjectIdToUsers = projectInfos
                .Where(s => listUserInfoMayHaveLeft.Select(x => x.UserId).Contains(s.UserId))
                .GroupBy(s => s.ProjectId)
                .ToDictionary(s => s.Key,
                s => s.Select(x => new EmployeeInfoNotifyHRTheEmployeeMayHaveLeftDto
                {
                    FullName =  x.FullName,
                    EmailAddress = x.EmailAddress,
                })
                .Distinct()
                .ToList());

            var projectInfoNotifys = new List<ProjectInfoNotifyHRTheEmployeeMayHaveLeftDto>();
            projectIds.ForEach(item =>
            {
                projectInfoNotifys.Add(new ProjectInfoNotifyHRTheEmployeeMayHaveLeftDto
                {
                    PMs = dicProjectIdToPMs[item],
                    Employees = dicProjectIdToUsers[item],
                });
            });

            var listEmailHR = SettingManager.GetSettingValueForApplication(AppSettingNames.NotifyHRTheEmployeeMayHaveLeftToHREmail);

            string[] arrListEmailHR = listEmailHR.Split(',');
            int countListEmailHR = arrListEmailHR.Count();

            var dEmailAddressToKomuId = WorkScope.GetAll<User>()
                .Where(s => arrListEmailHR.Contains(s.EmailAddress) || arrListEmailHR.Contains(s.UserName))
                .ToDictionary(s => s.EmailAddress, s => s.KomuUserId);

            var listHRInfo = new List<HRInfoTheEmployeeMayHaveLeftDto>();
            if (countListEmailHR > 0)
            {
                for (var i = 0; i < countListEmailHR; i++)
                {
                    listHRInfo.Add(new HRInfoTheEmployeeMayHaveLeftDto
                    {
                        HREmailAddress = arrListEmailHR[i],
                        HRKomuUserId = dEmailAddressToKomuId[arrListEmailHR[i]],
                    });
                }
            }

            var listResult = new NotifyHRTheEmployeeMayHaveLeftDto
            {
                HRInfos = listHRInfo,
                ProjectInfos = projectInfoNotifys.Distinct().ToList(),
            };

            return listResult;
        }
    }
}
