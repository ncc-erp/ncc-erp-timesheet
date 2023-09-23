using System;
using System.Collections.Generic;
using System.Text;
using static Ncc.Entities.Enum.StatusEnum;

namespace Timesheet.APIs.Timesheets.MyTimesheets.Dto
{
    public class MyFullTimesheetByKomuDto
    {
        public string EmailAddress { get; set; }

        public string ProjectCode { get; set; }

        public string TaskName { get; set; }

        public double Hour { get; set; }

        //public TypeOfWork TypeOfWork { get; set; }


        public string Note { get; set; }
    }
}
