using System;
using System.Collections.Generic;
using System.Text;
using static Ncc.Entities.Enum.StatusEnum;

namespace Timesheet.APIs.Public.Dto
{
    public class TimesheetDto
    {
        public string ActualEmailAddress { get; set; }
        public string TaskName { get; set; }
        public string ProjectName { get; set; }
        public string ProjectCode { get; set; }
        public TypeOfWork WorkType { get; set; }
        public int ActualWorkingMinute { get; set; }
        public string Note { get; set; }
        public DateTime DateAt { get; set; }

        public string ShadowEmailAddress { get; set; }
        public int ShadowWorkingMinute { get; set; }

        public int WorkingMinute => ShadowWorkingMinute > 0 ? ShadowWorkingMinute : ActualWorkingMinute;
        public string EmailAddress => string.IsNullOrEmpty(ShadowEmailAddress) ? ActualEmailAddress : ShadowEmailAddress;
        
        public float ManDay
        {
            get
            {
                return ((float) WorkingMinute / 60 / 8);
            }
        }

    }

    public class TimesheetTaxDto
    {
        public List<DateTime> ListWorkingDay { get; set; }
        public List<TimesheetDto> ListTimesheet { get; set; }
    }

}
