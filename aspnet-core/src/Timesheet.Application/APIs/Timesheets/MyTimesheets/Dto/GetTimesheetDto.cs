using System;
using System.Collections.Generic;
using System.Text;
using Timesheet.Uitls;
using static Ncc.Entities.Enum.StatusEnum;

namespace Timesheet.Timesheets.MyTimesheets.Dto
{
    public class GetTimesheetDto
    {
        public long Id { get; set; }
        public string ProjectName { get; set; }
        public string TaskName { get; set; }
        public long ProjectTaskId { get; set; }
        public string CustomerName { get; set; }
        public string ProjectCode { get; set; }
        public DateTime DateAt { get; set; }
        public int WorkingTime { get; set; }
        public TimesheetStatus Status { get; set; }
        public string Note { get; set; }
        public TypeOfWork TypeOfWork { get; set; }
        public bool IsCharged { get; set; }
        public bool Billable { get; set; }
        public bool IsTemp { get; set; }
        public string WorkType
        {
            get
            {
                return CommonUtils.ProjectUserWorkType(IsTemp);
            }
        }

    }
}
