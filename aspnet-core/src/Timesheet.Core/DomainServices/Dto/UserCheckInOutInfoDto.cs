using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using Timesheet.Uitls;

namespace Timesheet.DomainServices.Dto
{
    public class UserCheckInOutInfoDto
    {
        public long UserId { get; set; }
        public string EmailAddress { get; set; }
        public string UserName => EmailAddress.Substring(0, EmailAddress.IndexOf("@"));
        public string CheckIn { get; set; }
        public string CheckOut { get; set; }
        public bool IsNoCheckInAndNoCheckOut => string.IsNullOrEmpty(CheckIn) && string.IsNullOrEmpty(CheckOut);
        public bool IsNoCheckInOrNoCheckOut => !IsNoCheckInAndNoCheckOut && (string.IsNullOrEmpty(CheckIn) || string.IsNullOrEmpty(CheckOut));
        public bool IsNoCheckOut => string.IsNullOrEmpty(CheckOut);
    }

    public class PunishCheckInOutResult
    {
        public List<string> List100k { get; set; }
        public List<string> List50k { get; set; }
    }
}
