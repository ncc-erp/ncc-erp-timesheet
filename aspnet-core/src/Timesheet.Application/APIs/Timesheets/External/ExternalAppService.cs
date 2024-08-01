using Abp.Application.Services;
using Abp.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Ncc.Authorization.Users;
using Ncc.Configuration;
using Ncc.IoC;
using System;
using System.Linq;
using Timesheet.APIs.Timesheets.External.Dto;
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
        [Route("api/external/timesheet/reject-opentalk")]
        [AbpAllowAnonymous]
        [NccAuthentication]
        public async System.Threading.Tasks.Task RejectTimesheetOpenTalk(ListUserDto[] listUser)
        {
            var OpentalkList = WorkScope.GetAll<User>().Where(s => listUser.Where(x => x.FullName == s.FullName).Any())
                                      .Select(s => new OpenTalk
                                      {
                                          UserId = s.Id,
                                          startTime = listUser.Where(x => x.FullName == s.FullName).Select(x => x.startTime).FirstOrDefault(),
                                          endTime = listUser.Where(x => x.FullName == s.FullName).Select(x => x.endTime).FirstOrDefault()
                                      });
            await WorkScope.InsertRangeAsync(OpentalkList);
        }
    }
}
