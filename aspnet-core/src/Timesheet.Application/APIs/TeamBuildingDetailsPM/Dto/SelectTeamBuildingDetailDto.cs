using Abp.Application.Services.Dto;
using System;
using static Ncc.Entities.Enum.StatusEnum;

namespace Timesheet.APIs.TeamBuildingDetailsPM.dto
{
    public class SelectTeamBuildingDetailDto : EntityDto<long>
    {
        public long EmployeeId { get; set; }
        public long ProjectId { get; set; }
        public string ProjectName { get; set; }
        public string EmployeeFullName { get; set; }
        public string EmployeeEmailAddress { get; set; }
        public long? BranchId { get; set; }
        public string BranchName { get; set; }
        public string BranchColor { get; set; }
        public float Money { get; set; }
        public DateTime CreationTime { get; set; }
        public TeamBuildingStatus Status { get; set; }
        public DateTime ApplyMonth { get; set; }
        public string RequesterEmailAddress { get; set; }
        public bool IsWarning { get; set; }
        public string RequesterFullName { get; set; }
        public string EmployeeUserName { get; set; }
        public string EmployeeName { get; set; }
        public long? TeamBuildingRequestHistoryId { get; set; }
    }
}
