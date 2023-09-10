using System;
using System.Collections.Generic;
using System.Text;
using static Ncc.Entities.Enum.StatusEnum;

namespace Timesheet.Services.HRM.Dto
{
    public class UpdateToHrmv2Dto
    {
        public class UserToUpdateHrmv2Dto
        {
            public UserLevel NewLevel { get; set; }
            public Usertype UserType { get; set; }
            public bool IsFullSalary { get; set; }
            public double Salary { get; set; }
            public DateTime ApplyDate { get; set; }
            public string NormalizedEmailAddress { get; set; }
        }

        public class InputCreateRequestHrmv2Dto
        {
            public string RequestName { get; set; }
            public List<UserToUpdateHrmv2Dto> ListUserToUpdate { get; set; }
            public DateTime Applydate { get; set; }
            public string CreatedBy { get; set; }
        }
    }
}
