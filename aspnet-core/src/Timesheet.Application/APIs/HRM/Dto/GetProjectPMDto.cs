using Abp.Application.Services.Dto;
using System;
using System.Collections.Generic;
using System.Text;

namespace Timesheet.APIs.HRM.Dto
{
    public class GetProjectPMDto : EntityDto<long>
    {
        public string Name { get; set; }
        public string Code { get; set; }
        public string Pms { get; set; }
    }
}
