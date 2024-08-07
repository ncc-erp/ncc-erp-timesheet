using Abp.Application.Services;
using Abp.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Ncc.Authorization.Users;
using Ncc.IoC;
using System;
using System.Linq;
using Timesheet.APIs.Timesheets.External.Dto;
using Timesheet.Entities;
using Timesheet.NCCAuthen;

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
        [Route("api/external/timesheet/opentalk")]
        [AbpAllowAnonymous]
        [NccAuthentication]
        public async System.Threading.Tasks.Task createTimeSheetOpentalk(ListUserDto[] userList)
        {
            var userDict = userList.ToDictionary(s => s.google_id,s => s);
            var OpentalkList = await WorkScope.GetAll<User>().Where(s => s.GoogleId != null && userDict.ContainsKey(s.GoogleId))
                                      .Select(s => new OpenTalk
                                      {
                                          UserId = s.Id,
                                          startTime = userDict[s.GoogleId].startTime,
                                          endTime = userDict[s.GoogleId].endTime
                                      }).ToListAsync();
            foreach (var opentalk in OpentalkList)
            {
                var dbRequest = await WorkScope.GetAll<OpenTalk>().Where(s => s.UserId == opentalk.UserId)
                                                                  .Where(s => s.startTime.Date == opentalk.startTime.Date)
                                                                  .FirstOrDefaultAsync();
                if (dbRequest == null)
                {
                    opentalk.totalTime = Convert.ToInt32(opentalk.endTime.Subtract(opentalk.startTime).TotalMinutes);
                    await WorkScope.InsertAsync(opentalk);
                } else
                {
                    dbRequest.startTime = opentalk.startTime;
                    dbRequest.endTime = opentalk.endTime;
                    dbRequest.totalTime = Convert.ToInt32(opentalk.endTime.Subtract(opentalk.startTime).TotalMinutes);
                    await WorkScope.UpdateAsync(dbRequest);
                }
            }
        }
    }
}
