using Abp.Application.Services.Dto;
using Abp.AutoMapper;
using System;
using System.Collections.Generic;
using System.Text;
using Timesheet.Entities;
using static Ncc.Entities.Enum.StatusEnum;

namespace Timesheet.Timesheets.MyTimesheets.Dto
{
    [AutoMapTo(typeof(MyTimesheet))]
    public class MyTimesheetDto : EntityDto<long>
    {
        public long ProjectTaskId { get; set; }
        public string Note { get; set; }
        public int WorkingTime { get; set; }
        public int TargetUserWorkingTime { get; set; }
        public TypeOfWork TypeOfWork { get; set; }
        public bool IsCharged { get; set; }
        public DateTime DateAt { get; set; }
        public TimesheetStatus Status { get; set; }
        public long? ProjectTargetUserId { get; set; }
        public bool IsTemp { get; set; }
    }
}
