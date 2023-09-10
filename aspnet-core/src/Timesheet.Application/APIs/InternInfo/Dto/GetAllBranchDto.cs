using System;
using System.Collections.Generic;
using System.Text;

namespace Timesheet.APIs.InternInfo.Dto
{
    public class GetAllBranchDto
    {
        public long BranchId { get; set; }
        public string BranchDisplayName { get; set; }
    }
}
