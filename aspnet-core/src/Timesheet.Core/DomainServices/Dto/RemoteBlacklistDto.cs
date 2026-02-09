using System;
using System.Collections.Generic;
using System.Text;

namespace Timesheet.DomainServices.Dto
{
    public class AddNewUserToRemoteBlacklistDto
    {
        public long UserId { get; set; }
        public int PenaltyDays { get; set; }
    }
    public class GetRemoteBlacklistDto
    {
        public long Id { get; set; }
        public long UserId { get; set; }
        public string FullName { get; set; }
        public string UserName { get; set; }
        public int PenaltyDays { get; set; }
    }
    public class UpdatePenaltyDaysDto
    {
        public long Id { get; set; }
        public int PenaltyDays { get; set; }
    }
    public class ImportRemoteBlacklistRowDto
    {
        public int Row { get; set; }
        public string MezonUserId { get; set; }
        public string Email { get; set; }
        public string PenaltyDaysStr { get; set; }
    }
    public class ImportRemoteBlacklistResultDto
    {
        public int SuccessCount { get; set; }
        public int FailCount { get; set; }
        public List<string> FailedList { get; set; }
    }
}
