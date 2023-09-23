using System;
using System.Collections.Generic;
using System.Text;

namespace Timesheet.Services.Project.Dto
{
    public class UserInOutProject
    {
        public string EmailAddress { get; set; }
        public List<TimeJoinOut> ListTimeInOut { get; set; }
    }


    public class TimeJoinOut
    {
        public DateTime DateAt { get; set; }
        public bool IsJoin { get; set; }
    }
    public class GetUserTempInProject
    {
        public string EmailAddress { get; set; }
    }

    public class ResultGetUserTempInProject
    {
        public string Code { get; set; }
        public List<GetUserTempInProject> ListUserTempInProject { get; set; }
    }
}
