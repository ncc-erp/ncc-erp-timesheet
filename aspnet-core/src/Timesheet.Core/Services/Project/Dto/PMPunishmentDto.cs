using System;
using System.Collections.Generic;
using System.Text;

namespace Timesheet.Services.Project.Dto
{
    public class PMOtherPunishmentDto
    {
        public string MezonId { get; set; }
        public string Email { get; set; }
        public DateTime Date { get; set; }
        public float Amount { get; set; }
        public string Reason { get; set; }
    }
}
