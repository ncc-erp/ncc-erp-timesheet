using Abp.Application.Services.Dto;
using Abp.AutoMapper;
using Ncc.Entities;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;
using Timesheet.Anotations;
using static Ncc.Entities.Enum.StatusEnum;

namespace Timesheet.Timesheets.Tasks.Dto
{
    [AutoMapTo(typeof(Task))]
    public class TaskDto : EntityDto<long>
    {
        [Required]
        [ApplySearchAttribute] 
        public string Name { get; set; }
        
        public TaskType Type { get; set; }        
        public bool IsDeleted { get; set; }
    }
}
