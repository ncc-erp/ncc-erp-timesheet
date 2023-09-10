using Abp.Application.Services.Dto;
using System;
using System.Collections.Generic;
using System.Text;
using Timesheet.Anotations;

namespace Timesheet.APIs.Timesheets.Projects.Dto
{
    public class GetProjectFilterDto : EntityDto<long>
    {
        [ApplySearchAttribute]
        public string Name { get; set; }
        [ApplySearchAttribute]
        public string Code { get; set; }
    }
}
