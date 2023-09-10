using Abp.Application.Services.Dto;
using Abp.Authorization;
using Abp.UI;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Ncc;
using Ncc.Entities;
using System;
using System.Linq;
using System.Threading.Tasks;
using Timesheet.APIs.OverTimeSettings.Dto;
using Timesheet.Entities;
using Timesheet.Extension;
using Timesheet.Paging;
using Ncc.Configuration;
using Timesheet.Uitls;
using Ncc.IoC;

namespace Timesheet.APIs.OverTimeSettings
{
    public class OverTimeSettingAppService : AppServiceBase
    {
        public OverTimeSettingAppService(IWorkScope workScope) : base(workScope)
        {

        }

        [HttpPost]
        [AbpAuthorize(Ncc.Authorization.PermissionNames.OverTimeSetting_View)]
        public async Task<GridResult<GetOverTimeSettingDto>> GetAllPagging(GridParam input, DateTime? dateAt, long? projectId)
        {
            var query = WorkScope.GetAll<OverTimeSetting>()
                 .Where(s => !projectId.HasValue || s.ProjectId == projectId)
                 .Where(s => !dateAt.HasValue || s.DateAt == dateAt.Value.Date)
                 .Include(s => s.Project)
                 .OrderByDescending(s => s.DateAt)
                 .ThenBy(s=>s.Project.Name)
                 .Select(s => new GetOverTimeSettingDto
                 {
                     Id = s.Id,
                     ProjectName = s.Project.Name,
                     DateAt = s.DateAt,
                     Coefficient = s.Coefficient,
                     ProjectId = s.ProjectId,
                     Note = s.Note
                 });

            return await query.GetGridResult(query, input);
        }

        //TODO: test case function Save [var firstDayOfLastMonth:(now.Month=1 => new DateTime(2023,0,1) => Lỗi)]
        [HttpPost]
        [AbpAuthorize(Ncc.Authorization.PermissionNames.OverTimeSetting_AddNew, Ncc.Authorization.PermissionNames.OverTimeSetting_Edit)]
        public async Task<OverTimeSettingDto> Save(OverTimeSettingDto input)
        {
            var now = DateTimeUtils.GetNow();

            var DateToLockTimesheetOfLastMonthCfg = await SettingManager.GetSettingValueAsync(AppSettingNames.DateToLockTimesheetOfLastMonth);
            int DateToLockTimesheetOfLastMonth = 5;

            int.TryParse(DateToLockTimesheetOfLastMonthCfg, out DateToLockTimesheetOfLastMonth);

            var firstDayOfThisMonth = new DateTime(now.Year, now.Month, 1);
            var firstDayOfLastMonth = firstDayOfThisMonth.AddMonths(-1);

            if (now.Day < DateToLockTimesheetOfLastMonth && input.DateAt.Date < firstDayOfLastMonth)
            {
                Logger.Info("input.DateAt.Date < firstDayOfLastMonth " + input.DateAt.Date + "<" + firstDayOfLastMonth);
                throw new UserFriendlyException(string.Format("Cannot setting OT for day {0}", input.DateAt.ToString("MM/dd/yyyy")));
            }
            if (now.Day >= DateToLockTimesheetOfLastMonth && input.DateAt.Date < firstDayOfThisMonth)
            {
                Logger.Info("input.DateAt.Date < firstDayOfThisMonth " + input.DateAt.Date + "<" + firstDayOfThisMonth);
                throw new UserFriendlyException(string.Format("Cannot setting OT for day {0}", input.DateAt.ToString("MM/dd/yyyy")));
            }

            var projectName = await WorkScope.GetAll<Project>()
                .Where(s => s.Id == input.ProjectId)
                .Select(s => s.Name).FirstOrDefaultAsync();

            if (projectName == default)
            {
                throw new UserFriendlyException(string.Format("Not found project"));
            };

            var isExistOT = await WorkScope.GetAll<OverTimeSetting>().AnyAsync(s => s.DateAt == input.DateAt && s.ProjectId == input.ProjectId && s.Id != input.Id);
            if (isExistOT)
                throw new UserFriendlyException(string.Format("OverTimeSetting at {0} of project {1} already existed ", input.DateAt.ToString("MM/dd/yyyy"), projectName));

            if (input.Id <= 0) //insert
            {
                var item = ObjectMapper.Map<OverTimeSetting>(input);
                input.Id = await WorkScope.InsertAndGetIdAsync(item);
            }
            else //update
            {
                var item = await WorkScope.GetAsync<OverTimeSetting>(input.Id);
                ObjectMapper.Map<OverTimeSettingDto, OverTimeSetting>(input, item);
                await WorkScope.UpdateAsync(item);
            }

            return input;
        }

        [HttpDelete]
        [AbpAuthorize(Ncc.Authorization.PermissionNames.OverTimeSetting_Delete)]
        public async System.Threading.Tasks.Task Delete(EntityDto<long> input)
        {
            await WorkScope.GetRepo<OverTimeSetting>().DeleteAsync(input.Id);
        }
    }
}