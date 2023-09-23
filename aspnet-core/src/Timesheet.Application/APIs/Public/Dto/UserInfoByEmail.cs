using System;
using System.Collections.Generic;
using System.Text;
using Timesheet.Uitls;
using static Ncc.Entities.Enum.StatusEnum;

namespace Timesheet.APIs.Public.Dto
{
    public class UserInfoByEmail
    {
        public long UserId { get; set; }
        public string FullName { get; set; }
        public string EmailAddress { get; set; }
        public Usertype? UserType { get; set; }
        public string Type => CommonUtils.UserTypeName(this.UserType);
        public string Branch { get; set; }
        public IEnumerable<PDto> ProjectUsers { get; set; }
        
    }
    public class PDto
    {
        public long ProjectId { get; set; }
        public string ProjectName { get; set; }
        public List<string> Pms { get; set; }
    }
}
