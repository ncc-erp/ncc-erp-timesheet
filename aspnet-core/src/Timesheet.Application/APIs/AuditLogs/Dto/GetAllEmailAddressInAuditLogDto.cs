using System;
using System.Collections.Generic;
using System.Text;

namespace Timesheet.APIs.AuditLogs.Dto
{
    public class GetAllEmailAddressInAuditLogDto
    {
        public long UserId { get; set; }
        public string EmailAddress { get; set; }
    }

    public class GetAllServiceNameInAuditLogDto
    {
        public string ServiceName { get; set; }
    }

    public class GetAllMethodNameInAuditLogDto
    {
        public string MethodName { get; set; }
    }
}
