using System;
using System.Collections.Generic;
using System.Text;
using static Ncc.Entities.Enum.StatusEnum;

namespace Timesheet.Services.W2.Dto
{
    public class WorkFromHomeRequestDto
    {
        public string Email { get; set; }
        public string Date { get; set; }
        public WfhW2RequestStatus Status { get; set; }
    }
}
