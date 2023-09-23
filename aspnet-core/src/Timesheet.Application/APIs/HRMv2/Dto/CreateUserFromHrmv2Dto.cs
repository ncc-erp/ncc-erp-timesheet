using System;
using System.Collections.Generic;
using System.Text;
using static Ncc.Entities.Enum.StatusEnum;

namespace Timesheet.APIs.HRMv2.Dto
{
    public class CreateUserFromHrmv2Dto
    {
        public string UserName { get; set; }
        public Sex Sex { get; set; }
        public Usertype UserType { get; set; }
        public Usertype Type { get; set; }
        public string EmailAddress { get; set; }
        public string Surname { get; set; }
        public string Name { get; set; }
        public string UserCode { get; set; }
        public string PhoneNumber { get; set; }
        public bool IsActive { get; set; }
        public string BranchCode { get; set; }
        public string LevelCode { get; set; }
        public  string Password { get; set; }
        public  string[] RoleNames { get; set; }
        public long BranchId { get; set; }
    }
}
