using System;
using System.Collections.Generic;
using System.Text;
using static Ncc.Entities.Enum.StatusEnum;

namespace Timesheet.APIs.ManageWorkingTimes.Dto
{
    public class FilterDto
    {
        public string userName { get; set; }
        public RequestStatus? status { get; set; }
        public List<long> projectIds { get; set; }
    }
}
