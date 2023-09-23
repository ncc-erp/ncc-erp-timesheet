using Abp;
using Abp.Authorization;
using Microsoft.AspNetCore.Mvc;
using Ncc.IoC;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Timesheet.APIs.OverTimeHours.Dto;
using Timesheet.Entities;
using Timesheet.Paging;
using System.Linq;
using Ncc;
using static Ncc.Entities.Enum.StatusEnum;
using static Timesheet.APIs.OverTimeHours.Dto.GetOverTimeHourDto;
using Timesheet.Extension;
using Microsoft.EntityFrameworkCore;
using Ncc.Entities;
using Abp.Collections.Extensions;
using Timesheet.APIs.HRMv2.Dto;

namespace Timesheet.APIs.OverTimeHours
{
    [AbpAuthorize]
    public class OverTimeHourAppService : AppServiceBase
    {
        public OverTimeHourAppService(IWorkScope workScope) : base(workScope)
        {

        }

        [HttpPost]
        [AbpAuthorize(Ncc.Authorization.PermissionNames.Report_OverTime_View)]
        public async Task<GridResult<GetOverTimeHourDto>> GetAllPagging(GridParam input, int year, int month, long? projectId)
        {

            var query = IQueryGetOverTimeHour(year, month, projectId);

            var result = await query.GetGridResult(query, input);

            await UpdateOTCoefficient(result.Items);

            return result;
        }

        public async Task<List<GetOverTimeHourHRMDto>> GetAllOverTimeForHRM(int year, int month)
        {
            var query = IQueryGetOverTimeHour(year, month, null);

            var result = await query.ToListAsync();

            await UpdateOTCoefficient(result);

            return result.Select(s => new GetOverTimeHourHRMDto
            {
                Branch = s.BranchCode,
                NormalizedEmailAddress = s.NormalizedEmailAddress,
                UserId = s.UserId,
                ListOverTimeHour = s.ListOverTimeHour.Select(x => new OverTimeHourHRMDto
                {
                    Coefficient = x.Coefficient,
                    Day = x.Day,
                    OTHour = x.OTHour,
                    WorkingHour = x.WorkingHour,
                }).ToList()
            }).ToList();
        }

        public async Task<List<GetOverTimeHourHRMv2Dto>> GetAllOverTimeForHRMv2(InputCollectDataForPayslipDto input)
        {
            var query = IQueryGetOverTimeHour(input.Year, input.Month, null, input.UpperEmails, true);

            var result = await query.ToListAsync();

            await UpdateOTCoefficient(result);

            return result.Select(s => new GetOverTimeHourHRMv2Dto
            {
                NormalizedEmailAddress = s.NormalizedEmailAddress,
                ListOverTimeHour = s.ListOverTimeHour.GroupBy(x => x.DateAt)
                .Select(x => new OverTimeHourHRMv2Dto
                {
                    Date = x.Key,
                    OTHour = x.Sum(y=> y.OTHour),
                }).ToList()
            }).ToList();
        }

        public async Task<List<GetOverTimeHourDto>> GetListOverTimeForChart(DateTime startDate, DateTime endDate, string projectCode, List<string> emails)
        {
            long projectId = await GetProjectIdByCode(projectCode);

            var query = IQueryGetOverTimeHour(startDate, endDate, projectId, emails);

            var result = await query.ToListAsync();

            await UpdateOTCoefficient(result);

            return result;
        }

        public async Task<long> GetProjectIdByCode(string code)
        {
            return await WorkScope.GetAll<Project>()
                .Where(s => s.Code.ToLower().Trim() == code.ToLower().Trim())
                .Select(s => s.Id)
                .FirstOrDefaultAsync();
        }

        public async Task<string> GetProjectCodeById(long id)
        {
            return await WorkScope.GetAll<Project>()
                .Where(s => s.Id == id)
                .Select(s => s.Code)
                .FirstOrDefaultAsync();
        }

        private IQueryable<GetOverTimeHourDto> IQueryGetOverTimeHour(int year, int month, long? projectId, List<string> emails = null, bool? isForHrmv2 = false)
        {
            DateTime startDate = new DateTime(year, month, 1);
            DateTime endDate = startDate.AddMonths(1).AddDays(-1);
            return IQueryGetOverTimeHour(startDate, endDate, projectId, emails, isForHrmv2);
        }
        private IQueryable<GetOverTimeHourDto> IQueryGetOverTimeHour(DateTime startDate, DateTime endDate, long? projectId, List<string> emails, bool? isForHrmv2 = false)
        {
            var query = WorkScope.GetAll<MyTimesheet>()
                .Include(s => s.User).ThenInclude(s => s.Branch)
                .Include(s => s.ProjectTask).ThenInclude(s => s.Project)
                 .Where(ts => ts.Status == TimesheetStatus.Approve)
                 .Where(ts => ts.DateAt >= startDate.Date && ts.DateAt.Date <= endDate)
                 .Where(ts => ts.TypeOfWork == TypeOfWork.OverTime)
                 .Select(s => new
                 {
                     s.ProjectTask.ProjectId,
                     s.User.NormalizedEmailAddress,
                     s.User.EmailAddress,
                     s.UserId,
                     s.User.FullName,
                     s.User.BranchId,
                     BranchCode = s.User.Branch != null ? s.User.Branch.Code : null,
                     ProjectName = s.ProjectTask.Project.Name,
                     ProjectCode = s.ProjectTask.Project.Code,
                     s.DateAt,
                     s.WorkingTime,
                     s.IsCharged
                 });

            if (projectId.HasValue)
            {
                query = query.Where(s => s.ProjectId == projectId.Value);
            }

            if (!emails.IsNullOrEmpty())
            {
                if (emails.Count == 1 & !string.IsNullOrEmpty(emails[0]))
                {
                    var emailAddress = emails[0].ToUpper().Trim();
                    query = query.Where(s => s.NormalizedEmailAddress == emailAddress);
                }
                else
                {
                    query = query.Where(s => emails.Contains(s.NormalizedEmailAddress));
                }

            }

            if (isForHrmv2 == true)
            {
                return query.Select(ts => new
                {
                    ts.NormalizedEmailAddress,
                    ts.DateAt,
                    ts.ProjectId,
                    ts.WorkingTime,
                    ts.IsCharged
                }).GroupBy(ts => new
                {
                    ts.NormalizedEmailAddress
                }).Select(s => new GetOverTimeHourDto
                {
                    NormalizedEmailAddress = s.Key.NormalizedEmailAddress,
                    ListOverTimeHour = s.Select(x => new OverTimeHourDto
                    {
                        DateAt = x.DateAt.Date,
                        ProjectId = x.ProjectId,
                        WorkingMinute = x.WorkingTime,
                        IsCharged = x.IsCharged
                    }).ToList()
                });
            }
            else
            {
                return query.GroupBy(ts => new
                {
                    ts.UserId,
                    ts.FullName,
                    ts.EmailAddress,
                    ts.BranchCode,
                    ts.BranchId,
                    ts.NormalizedEmailAddress
                }).Select(s => new GetOverTimeHourDto
                {
                    UserId = s.Key.UserId,
                    EmailAddress = s.Key.EmailAddress,
                    NormalizedEmailAddress = s.Key.NormalizedEmailAddress,
                    Branch = s.Key.BranchCode,
                    BranchId = s.Key.BranchId.Value,
                    FullName = s.Key.FullName,
                    ListOverTimeHour = s.Select(x => new OverTimeHourDto
                    {
                        DateAt = x.DateAt.Date,
                        ProjectId = x.ProjectId,
                        WorkingMinute = x.WorkingTime,
                        ProjectName = $"[{x.ProjectCode}] {x.ProjectName}",
                        IsCharged = x.IsCharged
                    }).ToList()
                });
            }

        }

        private HashSet<DateTime> GetHashSetDateAt(IReadOnlyList<GetOverTimeHourDto> listOTTimesheet)
        {
            var resultSet = new HashSet<DateTime>();
            foreach (var userOT in listOTTimesheet)
            {
                foreach (var OTHour in userOT.ListOverTimeHour)
                {
                    if (!resultSet.Contains(OTHour.DateAt.Date))
                    {
                        resultSet.Add(OTHour.DateAt.Date);
                    }
                }

            }

            return resultSet;
        }
        private async System.Threading.Tasks.Task UpdateOTCoefficient(IReadOnlyList<GetOverTimeHourDto> listOTTimesheet)
        {
            var dateAts = GetHashSetDateAt(listOTTimesheet);

            var dayoffSettings = await WorkScope.GetAll<DayOffSetting>()
                .Where(s => dateAts.Contains(s.DayOff.Date))
                .Select(s => new DayOffSettingDto
                {
                    DateAt = s.DayOff,
                    Coefficient = s.Coefficient
                }).ToListAsync();

            var projectOTSettings = await WorkScope.GetAll<OverTimeSetting>()
             .Where(s => dateAts.Contains(s.DateAt.Date))
             .Select(s => new ProjectOTSettingDto
             {
                 DateAt = s.DateAt,
                 ProjectId = s.ProjectId,
                 Coefficient = s.Coefficient
             }).ToListAsync();

            foreach (var item in listOTTimesheet)
            {
                foreach (var ot in item.ListOverTimeHour)
                {
                    ot.ProjectCoefficient = projectOTSettings.Where(d => d.ProjectId == ot.ProjectId)
                        .Where(d => d.DateAt.Date == ot.DateAt.Date)
                        .Select(d => d.Coefficient)
                        .FirstOrDefault();

                    ot.DayOffCoefficient = dayoffSettings.Where(d => d.DateAt.Date == ot.DateAt.Date)
                     .Select(d => d.Coefficient).FirstOrDefault();

                }
            }
        }

    }
}

