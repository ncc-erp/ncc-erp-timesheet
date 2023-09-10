using Microsoft.EntityFrameworkCore.Internal;
using Ncc.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Timesheet.APIs.MyAbsenceDays.Dto;
using Timesheet.DomainServices.Dto;
using Timesheet.Uitls;
using static Ncc.Entities.Enum.StatusEnum;

namespace Timesheet.APIs.Timesheets.MyTimesheets.Dto
{
    public class NotifyKomuTimesheetDto : ProjectPMDto
    {
        public List<TimesheetKomuDto> Timesheets { get; set; }

        public string   ProjectCode { get; set; }
        public string ProjectName { get; set; }
        public List<string> Emails { get; set; }

        public string TimesheetsKomuMsg() { 
            var sb = new StringBuilder();
            foreach(var ts in Timesheets)
            {
                sb.AppendLine(ts.KomuMsg());
            }
            return sb.ToString();
        }
    }
    public class TimesheetKomuDto
    {
        public long Id { get; set; }
        public DateTime DateAt { get; set; }
        public int WorkingTime { get; set; }//min  
        public TypeOfWork TypeOfWork { get; set; }
        public string TaskName { get; set; }
        public bool? IsUnlockedByEmployee { get; set; }
        public bool IsCharged { get; set; }
        public string Note { get; set; }

        private string chargedInfo()
        {
            return this.TypeOfWork == TypeOfWork.NormalWorkingHours ? "" : $"({CommonUtils.ChargeName(this.IsCharged)})";
        }

        public string KomuMsg()
        {
           return $"#{Id} {DateTimeUtils.ToString(DateAt)} - {TaskName} - {CommonUtils.ConvertHourToHHmm(WorkingTime)} - {CommonUtils.TypeOfWorkName(TypeOfWork)} {chargedInfo()} - {Note}";
        }
    }
}
