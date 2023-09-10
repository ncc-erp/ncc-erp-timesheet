using Abp.Application.Services.Dto;
using Abp.AutoMapper;
using Ncc.Authorization.Users;
using Ncc.Entities;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;
using Timesheet.Anotations;
using Timesheet.Entities;
using static Ncc.Entities.Enum.StatusEnum;

namespace Timesheet.Timesheets.Timesheets.Dto
{
    [AutoMapTo(typeof(MyTimesheet))]
    public class TimeSheetDto : EntityDto<long>
    {
        public long ProjectTaskId { get; set; }

        public ProjectTask ProjectTask { get; set; }

        public long UserId { get; set; }

        public TypeOfWork TypeOfWork { get; set; }
        public bool IsCharged { get; set; }
        public string Note { get; set; }

        public TimesheetStatus Status { get; set; }

        public DateTime DateAt { get; set; }
        public int WorkingTime { get; set; }//min     
    }
}