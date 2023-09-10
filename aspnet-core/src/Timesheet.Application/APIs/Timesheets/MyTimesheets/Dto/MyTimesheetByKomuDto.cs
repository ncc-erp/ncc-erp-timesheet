using Abp.Application.Services.Dto;
using Abp.AutoMapper;
using System;
using System.Collections.Generic;
using System.Text;
using Timesheet.Entities;
using static Ncc.Entities.Enum.StatusEnum;

namespace Timesheet.Timesheets.MyTimesheets.Dto
{    
    public class MyTimesheetByKomuDto
    {
        public string EmailAddress { get; set; }
        public string Note { get; set; }
        public double Hour { get; set; }
        public DateTime? DateAt { get; set; }

    }
}
