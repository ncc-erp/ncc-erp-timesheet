﻿using Microsoft.EntityFrameworkCore.Internal;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Timesheet.DomainServices.Dto;
using static Ncc.Entities.Enum.StatusEnum;

namespace Timesheet.APIs.MyAbsenceDays.Dto
{
    public class ProjectPMDto
    {
        public long ProjectId { get; set; }
        public NotifyChannel notifyChannel {  get; set; }
        public string mezonUrl { get; set; }
        public string KomuChannelId { get; set; }
        public bool IsNotifyEmail { get; set; }
        public bool IsNotifyKomu { get; set; }
        public bool IsNoticeKMSubmitTS { get; set; }
        public bool IsNoticeKMApproveRejectTimesheet { get; set; }
        public bool IsNoticeKMRequestOffDate { get; set; }
        public bool IsNoticeKMApproveRequestOffDate { get; set; }
        public bool IsNoticeKMRequestChangeWorkingTime { get; set; }
        public bool IsNoticeKMApproveChangeWorkingTime { get; set; }  
        public List<NotifyUserInfoDto> PMs { get; set; }

        public string KomuPMsTag()
        {
            return PMs.Select(s => s.KomuAccountTag(this.notifyChannel)).ToList().Join(", ");
        }

        public string KomuPMsTag(HashSet<long> alreadySentToPMIds)
        {

            var PMsNoTag =  PMs.Where(s => alreadySentToPMIds.Contains(s.UserId))
                .Select(s => $"**{s.FullName}**");

            var PMsTag = PMs.Where(s => !alreadySentToPMIds.Contains(s.UserId))
                .Select(s => s.KomuAccountTag(this.notifyChannel));

            return PMsTag.Union(PMsNoTag).ToList().Join(", ");
        }
    }
}
