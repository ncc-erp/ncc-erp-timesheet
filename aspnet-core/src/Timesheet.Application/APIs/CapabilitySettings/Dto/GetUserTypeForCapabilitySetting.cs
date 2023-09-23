using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;
using Timesheet.Anotations;
namespace Timesheet.APIs.CapabilitySettings.Dto
{
    public class GetUserTypeForCapabilitySetting
    {
        public long Id { get; set; }
        [ApplySearchAttribute]
        [Required]
        public string Name { get; set; }
    }
}