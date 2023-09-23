using Abp.Application.Services.Dto;
using System;
using System.Collections.Generic;
using System.Text;

namespace Timesheet.Users.Dto
{
    public class GetAllPMDto 
    {
        public long PMId { get; set; }
        public string PMFullName { get; set; }
        public string PMEmailAddress { get; set; }
        public string PMAvatarPath { get; set; }
       
    }
}
