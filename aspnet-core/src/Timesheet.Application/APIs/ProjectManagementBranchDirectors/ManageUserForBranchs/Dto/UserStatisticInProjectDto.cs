using System;
using System.Collections.Generic;
using System.Text;

namespace Timesheet.APIs.ProjectManagementBranchDirectors.ManageUserForBranchs.Dto
{
    public class UserStatisticInProjectDto
    {
        public long ProjectId { get; set; }
        public string ProjectName { get; set; }
        public long? BranchId { get; set; }
        public int TotalUser { get; set; }
        public int MemberCount { get; set; }
        public int ExposeCount { get; set; }
        public int ShadowCount { get; set; }
    }
}
