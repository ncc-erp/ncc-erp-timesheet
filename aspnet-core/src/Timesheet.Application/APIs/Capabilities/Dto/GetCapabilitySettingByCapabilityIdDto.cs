
using System;
using System.Collections.Generic;
using System.Text;
using static Ncc.Entities.Enum.StatusEnum;
namespace Timesheet.APIs.Capabilities.Dto
{
    public class GetCapabilitySettingByCapabilityIdDto
    {
        public Usertype Usertype { get; set; }
        public string UsertypeName => Usertype == Usertype.Internship ? "Intern" : Usertype.ToString();
        public string PositionName { get; set; }
        public float Coefficient { get; set; }
    }
}