using Abp.AutoMapper;
using Abp.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Text;
using Timesheet.Anotations;
using Timesheet.Entities;
using Timesheet.Extension;
using Timesheet.Manager.Common.Dto;
using static Ncc.Entities.Enum.StatusEnum;
using Branch = Ncc.Entities.Enum.StatusEnum.Branch;

namespace Timesheet.APIs.RetroDetails.Dto
{
    public class RetroResultDto : Entity<long>
    {
        [ApplySearch]
        public string FullName { get; set; }
        [ApplySearch]
        public string EmailAddress { get; set; }
        [ApplySearch]
        public string ProjectName { get; set; }
        public long ProjectId { get; set; }
        public string PositionName { get; set; }
        public long PositionId { get; set; }
        public long? BranchId { get; set; }
        public long? UserBranchId { get; set; }
        public string BranchColor { get; set; }
        public string UserBranchColor { get; set; }
        public string BranchName { get; set; }
        public string UserBranchName { get; set; }
        public Usertype? Type { get; set; }
        public Usertype? UserType { get; set; }
        public UserLevel? Level { get; set; }
        public UserLevel? UserLevel { get; set; }
        public double Point { get; set; }
        public string Note { get; set; }
        public string RetroName { get; set; }
        public DateTime UpdatedAt => LastModifierTime.HasValue ? LastModifierTime.Value : CreationTime;
        public string UpdatedName => String.IsNullOrEmpty(LastModifierUserName) ? CreatedUserName : LastModifierUserName;
        public string CreatedUserName { get; set; }
        public string LastModifierUserName { get; set; }
        public DateTime CreationTime { get; set; }
        public DateTime? LastModifierTime { get; set; }
        public long UserId { get; set; }
        public long? PmId { get; set; }
        public string PmFullName { get; set; }
        public string PmEmailAddress { get; set; }
    }
}