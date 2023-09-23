using Abp.Authorization;
using Abp.BackgroundJobs;
using Abp.Domain.Repositories;
using Microsoft.AspNetCore.Mvc;
using Ncc;
using Ncc.Authorization;
using Ncc.IoC;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Timesheet.APIs.BackgroundJobs.Dto;
using Timesheet.Extension;
using Timesheet.Paging;
namespace Timesheet.APIs.BackgroundJobs
{
    [AbpAuthorize]
    public class BackgroundJobAppService : AppServiceBase
    {
        private readonly IRepository<BackgroundJobInfo, long> _storeJobs;
        private readonly IBackgroundJobManager _backgroundJobManager;
        public BackgroundJobAppService(IWorkScope workScope, 
                IRepository<BackgroundJobInfo, long> storeJobs,
                IBackgroundJobManager backgroundJobManager) : base(workScope)
        {
            _storeJobs = storeJobs;
            _backgroundJobManager = backgroundJobManager;
        }

        [HttpPost]
        public async Task<GridResult<GetAllBackgroundJobsDto>> GetAllPaging(InputToGetAll input)
        {
            var query = _storeJobs.GetAll()
                .Select(x => new GetAllBackgroundJobsDto
                {
                    Id = x.Id,
                    JobType = x.JobType,
                    JobAgrs = x.JobArgs,
                    NextTryTime = x.NextTryTime,
                    LastTryTime = x.LastTryTime,
                    Priority = (int)x.Priority,
                    TryCount = x.TryCount,
                    IsAbandoned = x.IsAbandoned,
                    CreationTime = x.CreationTime,

                }).OrderByDescending(x => x.CreationTime);

            if (!string.IsNullOrEmpty(input.SearchById))
            {
                query = (IOrderedQueryable<GetAllBackgroundJobsDto>)query.Where(x => x.Id.ToString().Contains(input.SearchById));
            }
            return await query.GetGridResult(query, input.Param);

        }
        [AbpAuthorize(PermissionNames.Admin_BackgroundJob_Delete)]
        [HttpDelete]
        public async Task Delete(long Id)
        {
            await _backgroundJobManager.DeleteAsync(Id.ToString());
        }
    }
}
