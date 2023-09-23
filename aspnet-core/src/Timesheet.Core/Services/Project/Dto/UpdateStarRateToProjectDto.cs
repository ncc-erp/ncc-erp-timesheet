using System;
using System.Collections.Generic;
using System.Text;
using static Ncc.Entities.Enum.StatusEnum;

namespace Timesheet.Services.Project.Dto
{
    public class UpdateStarRateToProjectDto
    {
        public string UserCode { get; set; }
        public string EmailAddress { get; set; }
        public float? StarRate { get; set; }
        public UserLevel? Level { get; set; }
        public Usertype? Type { get; set; }
    }
}
