using System;
using System.Collections.Generic;
using System.Text;
using static Ncc.Entities.Enum.StatusEnum;

namespace Timesheet.Services.HRM.Dto
{
    public class UpdateToHrmDto
    {
        public class UserToUpdateDto
        {
            public long UserId { get; set; }
            public string EmailAddress { get; set; }
            public UserLevel? NewLevel { get; set; }
            public Usertype? Type { get; set; }
            public bool? IsFullSalary { get; set; }
            public byte? SubLevel { get; set; }
            public int? Salary { get; set; }
            public long UpdateToHRMByUserId { get; set; }
            public DateTime ExcutedDate { get; set; }
            public string NormalizedEmailAddress { get; set; }
        }


        public class CreateRequestFromTSDto
        {
            public string CreatorNormalizedEmail { get; set; }
            public string RequestName { get; set; }
            public List<UserToUpdateDto> ListUserToUpdate { get; set; }
            public DateTime ExcutedDate { get; set; }

        }
    }
}
