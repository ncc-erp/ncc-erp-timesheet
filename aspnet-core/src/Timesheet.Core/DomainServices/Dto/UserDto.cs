using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Abp.Application.Services.Dto;
using Abp.Authorization.Users;
using Abp.AutoMapper;
using Ncc.Authorization.Users;
using Timesheet.Anotations;

using static Ncc.Entities.Enum.StatusEnum;

namespace Timesheet.DomainServices.Dto
{
    [AutoMapFrom(typeof(User))]
    public class UserDto : EntityDto<long>
    {
        [Required]
        public string UserName { get; set; }

        [Required]
        public string Name { get; set; }

        [Required]
        public string Surname { get; set; }

        [Required]
        [EmailAddress]
        public string EmailAddress { get; set; }

        public string PhoneNumber { get; set; }

        public string Address { get; set; }

        public bool IsActive { get; set; }
        public string FullName { get; set; }
        public string[] RoleNames { get; set; }
        public Usertype? Type { get; set; }
        public int? Salary { get; set; }
        public DateTime? SalaryAt { get; set; }
        public DateTime? StartDateAt { get; set; }
        public Double? AllowedLeaveDay { get; set; }
        [ApplySearchAttribute]
        [MaxLength(256)]
        public string UserCode { get; set; }
        public string JobTitle { get; set; }
        public UserLevel? Level { get; set; }
        public string RegisterWorkDay { get; set; }
        public long? ManagerId { get; set; }
        public Branch? Branch { get; set; }
        public Sex? Sex { get; set; }
        public string AvatarPath { get; set; }
        public double MorningWorking { get; set; }
        public string MorningStartAt { get; set; }
        public string MorningEndAt { get; set; }
        public double AfternoonWorking { get; set; }
        public string AfternoonStartAt { get; set; }
        public string AfternoonEndAt { get; set; }
        public bool? isWorkingTimeDefault{ get; set; }
        public bool IsStopWork { get; set; }
        public long? BranchId { get; set; }
        public UserLevel? BeginLevel { get; set; }
        public DateTime? EndDateAt { get; set; }
        public long? PositionId { get; set; }
        public string PositionName { get; set; }
    }
    
}

