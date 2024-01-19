using Abp.Domain.Entities;
using System;
using Timesheet.Anotations;
using static Ncc.Entities.Enum.StatusEnum;

namespace Timesheet.APIs.TeamBuildingRequestHistories.dto
{
    public class TeamBuildingRequestHistoryDto:Entity<long>
    {
        public long RequesterId { get; set; }
        [ApplySearch]
        public string FullNameRequester { get; set; }
        [ApplySearch]
        public string EmailRequester { get; set; }
        [ApplySearch]
        public string TitleRequest { get; set; }
        public float RequestMoney { get; set; }
        public float? DisbursedMoney { get; set; }
        public float? RemainingMoney { get; set; }
        public RemainingMoneyStatus RemainingMoneyStatus { get; set; }
        public TeamBuildingRequestStatus Status { get; set; }
        public DateTime CreationTime { get; set; }
        public float InvoiceAmount { get; set; }
    }
}
