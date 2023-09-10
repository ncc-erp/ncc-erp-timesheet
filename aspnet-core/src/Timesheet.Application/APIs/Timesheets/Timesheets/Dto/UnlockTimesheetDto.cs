using Abp.AutoMapper;
using Abp.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Text;
using Timesheet.Entities;
using static Ncc.Entities.Enum.StatusEnum;

namespace Timesheet.APIs.Timesheets.Timesheets.Dto
{
    [AutoMap(typeof(UnlockTimesheet))]
    public class UnlockTimesheetDto : Entity<long>
    {
        public long UserId { get; set; }
        public LockUnlockTimesheetType Type { get; set; }
    }
}
