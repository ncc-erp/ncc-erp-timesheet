using Abp.Application.Services.Dto;
using Abp.AutoMapper;
using System;
using System.Collections.Generic;
using System.Text;
using Timesheet.Entities;
using Ncc.IoC;
using System.Linq;
using Ncc.Entities;
using static Ncc.Entities.Enum.StatusEnum;

namespace Timesheet.Timesheets.MyTimesheets.Dto
{
    public class TimeStatisticMemberDto
    {
        public long UserID { get; set; }
        public string UserName { get; set; }
        public ProjectUserType projectUserType { get; set; }
        public int TotalWorkingTime { get; set; }
        public int BillableWorkingTime { get; set; }
    }
}
