using Abp.Domain.Entities.Auditing;
using Ncc.Authorization.Users;
using Ncc.Entities;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;
using static Ncc.Entities.Enum.StatusEnum;

namespace Timesheet.Entities
{
    public class TeamBuildingDetail : FullAuditedEntity<long>
    {
        [ForeignKey(nameof(ProjectId))]
        public Project Project { get; set; }
        public long ProjectId { get; set; }
        [ForeignKey(nameof(EmployeeId))]
        public User Employee { get; set; }
        public long EmployeeId { get; set; }

        [ForeignKey(nameof(TeamBuildingRequestHistoryId))]
        public TeamBuildingRequestHistory TeamBuildingRequestHistory { get; set; }
        public DateTime ApplyMonth { get; set; }
        public long? TeamBuildingRequestHistoryId { get; set; }
        public float Money { get; set; }
        public TeamBuildingStatus Status { get; set; }
    }
}
