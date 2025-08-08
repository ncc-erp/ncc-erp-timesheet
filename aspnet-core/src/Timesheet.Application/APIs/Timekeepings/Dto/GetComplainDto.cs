using System;
using System.Collections.Generic;
using System.Text;

namespace Timesheet.APIs.Timekeepings.Dto
{
    public class GetComplainDto
    {
        public int Id { get; set; }
        public int TimekeepingId { get; set; }
        public int PunishmentType { get; set; }
        public string PunishmentTypeName { get; set; }
        public string UserNote { get; set; }
        public DateTime CreatedTime { get; set; }
        public string Status { get; set; }
    }
}
