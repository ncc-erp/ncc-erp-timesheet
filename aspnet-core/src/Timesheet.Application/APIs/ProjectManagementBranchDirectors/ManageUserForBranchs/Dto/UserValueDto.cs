using System;
using System.Collections.Generic;
using System.Text;
using Timesheet.Entities;

namespace Timesheet.APIs.ProjectManagementBranchDirectors.ManageUserForBranchs.Dto
{
    public class UserValueDto
    {
        public long UserId { get; set; }
        public long? BranchId { get; set; }
        public string Name { get; set; }
        public ValueOfUserType ValueType { get; set; }
    }
}
