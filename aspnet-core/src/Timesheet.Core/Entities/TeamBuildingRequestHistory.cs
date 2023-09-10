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
    public class TeamBuildingRequestHistory : FullAuditedEntity<long>
    {
        [ForeignKey(nameof(RequesterId))]
        public User Requester { get; set; }
        public long RequesterId { get; set; }
        public string TitleRequest { get; set; }
        public string Note { get; set; }
        public float RequestMoney { get; set; }
        public float? DisbursedMoney { get; set; }
        public float? RemainingMoney { get; set; }
        public RemainingMoneyStatus RemainingMoneyStatus { get; set; }
        public TeamBuildingRequestStatus Status { get; set; }
        public float? InvoiceAmount { get; set; }
    }
}
