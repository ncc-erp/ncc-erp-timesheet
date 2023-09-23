using Abp.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Timesheet.Anotations;
using Timesheet.Paging;
using Timesheet.Uitls;

namespace Timesheet.APIs.BackgroundJobs.Dto
{
    public class GetAllBackgroundJobsDto: Entity<long>
    {
        [ApplySearch]
        public string JobType { get; set; }
        [ApplySearch]
        public string JobAgrs { get; set; }
        public int TryCount { get; set; }
        public DateTime? LastTryTime { get; set; }
        public DateTime NextTryTime { get; set; }
        public bool IsAbandoned { get; set; }
        public int Priority { get; set; }
        public DateTime CreationTime { get; set; }
        public long CreatorUserId { get; set; }
        public string Description => CommonUtils.BackgroundJobDescription(JobAgrs).Where(s => s.SubJobType.Contains(SubJobType)).FirstOrDefault()?.Description;
        public string SubJobType => (((JobType.Split(','))[0]).Split('.')).LastOrDefault();
    }

    public class InputToGetAll
    {
        public string SearchById { get; set; }
        public GridParam Param { get; set; }
    }
}
