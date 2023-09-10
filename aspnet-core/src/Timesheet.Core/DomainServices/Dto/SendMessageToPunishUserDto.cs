using Abp.Configuration;
using Ncc.Configuration;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Net.Mail;
using System.Text;
using Timesheet.Uitls;

namespace Timesheet.DomainServices.Dto
{
    public class SendMessageToPunishUserDto
    {
        public string UserName { get; set; }
        public DateTime DateAt { get; set; }
        public int MoneyPunish { get; set; }
        public string NotePunish { get; set; }
        public string CheckIn { get; set; }
        public string CheckOut { get; set; }
        public string RegisterCheckIn { get; set; }
        public string RegisterCheckOut { get; set; }
        public double? ResultCheckIn { get; set; }
        public double? ResultCheckOut { get; set; }
        public bool IsPunishedCheckIn { get; set; }
        public bool IsPunishedCheckOut { get; set; }
    }
}
