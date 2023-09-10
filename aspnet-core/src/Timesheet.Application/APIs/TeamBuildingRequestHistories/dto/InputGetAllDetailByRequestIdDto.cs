using System;
using System.Collections.Generic;
using System.Text;

namespace Timesheet.APIs.TeamBuildingRequestHistories.dto
{
    public class InputGetAllDetailByRequestIdDto
    {
        public long TeamBuildingHistoryId { get; set; }
        public long? ProjectId { get; set; }
        public int? Month { get; set; }
        public long? BranchId { get; set; }
    }
}
