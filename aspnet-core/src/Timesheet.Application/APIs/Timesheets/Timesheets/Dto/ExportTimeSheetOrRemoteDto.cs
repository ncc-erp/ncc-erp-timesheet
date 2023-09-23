using Abp.Application.Services.Dto;
using System;
using System.Collections.Generic;
using System.Text;
using static Ncc.Entities.Enum.StatusEnum;

namespace Timesheet.APIs.Timesheets.Timesheets.Dto
{
    public class ExportTimeSheetOrRemoteDto : EntityDto<long>
    {
        public string Name { get; set; }
        public string ProjectName { get; set; }
        public double NormalWorkingHours { get; set; }
        public double OverTime { get; set; }
        public DayType DayOffType { get; set; }
        public OnDayType? TimeCustom { get; set; }
        public double AbsenceTime { get; set; }
    }
}
