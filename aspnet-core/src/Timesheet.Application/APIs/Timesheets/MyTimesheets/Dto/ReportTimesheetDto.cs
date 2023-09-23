using Abp.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Text;
using static Ncc.Entities.Enum.StatusEnum;

namespace Timesheet.Timesheets.MyTimesheets.Dto
{
    public class ReportTimesheetDto : Entity<long>
    {
        public string UserName { get; set; }
        public DateTime DateAt { get; set; }
        public TypeOfWork TypeOfWork { get; set; }
        public string TaskName { get; set; }
        public string Note { get; set; }
        public int WorkingTime { get; set; }

        public int TargetUserWorkingTime { get; set; }
        public string TargetUserName { get; set; }
        public string RoleName { get; set; }
        public bool IsShadow { get; set; }

    }
}
