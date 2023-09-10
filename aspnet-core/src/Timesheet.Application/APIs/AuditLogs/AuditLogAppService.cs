using Abp.Auditing;
using Abp.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Ncc;
using Ncc.Authorization.Users;
using Ncc.IoC;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Threading.Tasks;
using Timesheet.APIs.AuditLogs.Dto;
using Timesheet.Extension;
using Timesheet.Paging;

namespace Timesheet.APIs.AuditLogs
{
    public class AuditLogAppService : AppServiceBase
    {
        public AuditLogAppService(IWorkScope workScope) : base(workScope)
        {}

        [HttpPost]
        [AbpAuthorize(Ncc.Authorization.PermissionNames.Admin_AuditLog_View)]
        public async Task<GridResult<GetAuditLogDto>> GetAllPagging(GridParam input)
        {
            var dEmailAddress = WorkScope.GetAll<User>().ToDictionary(s => s.Id, s => s.EmailAddress);
            var query = WorkScope.GetAll<AuditLog>()
                .Select(s => new GetAuditLogDto
                {
                    ExecutionDuration = s.ExecutionDuration,
                    ExecutionTime = s.ExecutionTime,
                    MethodName= s.MethodName,
                    Parameters= s.Parameters,
                    ServiceName= s.ServiceName,
                    UserId = s.UserId,
                    UserIdString = s.UserId.ToString(),
                    EmailAddress = s.UserId.HasValue ? dEmailAddress.ContainsKey(s.UserId.Value) ? dEmailAddress[s.UserId.Value] : "" : ""
                });

            return await query.GetGridResult(query, input);
        }

        [HttpGet]
        [AbpAuthorize(Ncc.Authorization.PermissionNames.Admin_AuditLog_View)]
        public async Task<List<GetAllEmailAddressInAuditLogDto>> GetAllEmailAddressInAuditLog()
        {
            var userIdInAuditLog = await WorkScope.GetAll<AuditLog>().Where(s => s.UserId != null)
                .Select(s => s.UserId).Distinct().ToListAsync();

            var emailAddressByUserId = WorkScope.GetAll<User>().Where(s => userIdInAuditLog.Contains(s.Id)).Select(s => new GetAllEmailAddressInAuditLogDto
            {
                EmailAddress = s.EmailAddress,
                UserId = s.Id
            }).ToListAsync();

            return await emailAddressByUserId;
        }

        [HttpGet]
        [AbpAuthorize(Ncc.Authorization.PermissionNames.Admin_AuditLog_View)]
        public async Task<List<GetAllServiceNameInAuditLogDto>> GetAllServiceNameInAuditLog()
        {
            var serviceNames = await WorkScope.GetAll<AuditLog>()
                .Select(x=> new GetAllServiceNameInAuditLogDto { ServiceName = x.ServiceName})
                .Distinct().ToListAsync();
            return serviceNames;
        }

        [HttpGet]
        [AbpAuthorize(Ncc.Authorization.PermissionNames.Admin_AuditLog_View)]
        public async Task<List<GetAllMethodNameInAuditLogDto>> GetAllMethodNameInAuditLog()
        {
            var serviceNames = await WorkScope.GetAll<AuditLog>()
                .Select(x => new GetAllMethodNameInAuditLogDto { MethodName = x.MethodName })
                .Distinct().ToListAsync();
            return serviceNames;
        }
    }
}
