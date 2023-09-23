using Abp.Domain.Entities;
using System;
using Timesheet.Anotations;
using static Ncc.Entities.Enum.StatusEnum;

namespace Timesheet.APIs.TeamBuildingDetails.Dto
{
    public class GetTeamBuildingDetailDto : Entity<long>
    {
        [ApplySearch]
        public string ProjectName { get; set; }
        [ApplySearch]
        public string ProjectCode { get; set; }
        public long ProjectId { get; set; }
        public string PMEmailAddress { get; set; }
        [ApplySearch]
        public string EmployeeFullName { get; set; }
        [ApplySearch]
        public string EmployeeEmailAddress { get; set; }
        public long EmployeeId { get; set; }
        public string RequesterEmailAddress { get; set; }
        public string RequesterFullName { get; set; }
        public long? RequesterId { get; set; }
        public float Money { get; set; }
        public DateTime ApplyMonth { get; set; }
        public DateTime UpdatedAt => LastModifierTime.HasValue ? LastModifierTime.Value : CreationTime;
        public string UpdatedName => String.IsNullOrEmpty(LastModifierUserName) ? CreatedUserName : LastModifierUserName;
        public string CreatedUserName { get; set; }
        public string LastModifierUserName { get; set; }
        public DateTime CreationTime { get; set; }
        public DateTime? LastModifierTime { get; set; }
        public TeamBuildingStatus Status { get; set; }
    }

    public class GetProjectTeamBuildingDto
    {
        public long Id { get; set; }
        public string Name { get; set; }
    }

    public class GetEmployeeTeamBuildingDto
    {
        public long Id { get; set; }
        public string FullName { get; set; }
    }
}
