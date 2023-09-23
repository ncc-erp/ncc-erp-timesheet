using System;
using System.Collections.Generic;
using System.Text;

namespace Timesheet.Hubs.Dto
{
    public class HubConnectionDto
    {
        public long UserId { get; set; }
        public string ConnectionId { get; set; }
        public bool IsConnected { get; set; }
    }
}
