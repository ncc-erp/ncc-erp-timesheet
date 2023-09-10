using Abp.Domain.Entities.Auditing;
using System;
using System.Collections.Generic;
using System.Text;

namespace Timesheet.Entities
{
    public class TeamBuildingRequestHistoryFile :FullAuditedEntity<long>
    {
        public string Url { get; set; }
        public string FileName { get; set; }
        public TeamBuildingRequestHistory TeamBuildingRequestHistory { get; set; }
        public long TeamBuildingRequestHistoryId { get; set; }

        public float? InvoiceAmount { get; set; }
        public bool? IsVAT { get; set; }
    }
}
