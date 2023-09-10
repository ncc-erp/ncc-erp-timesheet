using System;
using System.Collections.Generic;
using System.Text;

namespace Timesheet.APIs.Info.Dto
{
    public class ListUserUnlockTSDto
    {
        public double TotalAmount { get; set; }
        public List<UserUnlockTSDto> ListUnlock{get; set;}
    }
    public class UserUnlockTSDto
    {
        public int Rank { get; set; }
        public string FullName { get; set; }
        public double Amount { get; set; }
    }
}
