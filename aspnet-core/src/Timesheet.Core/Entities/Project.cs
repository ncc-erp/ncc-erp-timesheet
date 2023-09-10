using Abp.Domain.Entities;
using Ncc.Entities.Enum;
using System;
using System.Collections.Generic;
using System.Text;
using static Ncc.Entities.Enum.StatusEnum;
using Sieve.Attributes;
using AutoMapper;
using Abp.AutoMapper;
using Abp.Domain.Entities.Auditing;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel;

namespace Ncc.Entities
{
    public class Project : FullAuditedEntity<long>
    {
        public string Name { get; set; }
        public DateTime TimeStart { get; set; }
        public DateTime? TimeEnd { get; set; }
        public ProjectStatus Status { get; set; }
        public string Code { get; set; }
        public ProjectType ProjectType { get; set; }
        public string Note { get; set; }

        [ForeignKey(nameof(CustomerId))]
        public Customer Customer { get; set; }
        public long CustomerId { get; set; }
        public string KomuChannelId { get; set; }
        [DefaultValue(false)]
        public bool IsNotifyToKomu { get; set; }
        [DefaultValue(false)]
        public bool IsNoticeKMSubmitTS { get; set; }
        [DefaultValue(false)]
        public bool IsNoticeKMRequestOffDate { get; set; }
        [DefaultValue(false)]
        public bool IsNoticeKMApproveRequestOffDate { get; set; }
        [DefaultValue(false)]
        public bool IsNoticeKMRequestChangeWorkingTime { get; set; }
        [DefaultValue(false)]
        public bool IsNoticeKMApproveChangeWorkingTime { get; set; }
        public bool isAllUserBelongTo { get; set; }
        [DefaultValue(true)]
        public bool IsAllowTeamBuilding { get; set; }
    }
}
