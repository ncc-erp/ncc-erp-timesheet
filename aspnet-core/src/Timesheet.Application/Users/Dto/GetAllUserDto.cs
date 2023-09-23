using Abp.Application.Services.Dto;
using Abp.Authorization.Users;
using Abp.AutoMapper;
using Ncc.Authorization.Users;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;
using Timesheet.Anotations;
using Timesheet.Uitls;
using static Ncc.Entities.Enum.StatusEnum;

namespace Timesheet.Users.Dto
{
   
    public class GetAllUserDto : EntityDto<long>
    {

        [ApplySearchAttribute]
        public string UserName { get; set; }

        [ApplySearchAttribute]
        public string Name { get; set; }

        [ApplySearchAttribute]
        public string Surname { get; set; }

        [ApplySearchAttribute]
        public string EmailAddress { get; set; }

        public string PhoneNumber { get; set; }

       //[ApplySearchAttribute]
        public string Address { get; set; }

        public bool IsActive { get; set; }

        public string FullName { get; set; }
        public string[] RoleNames { get; set; }
        public IEnumerable<PUDto> ProjectUsers { get; set; }
        public Usertype? Type { get; set; }
        public int? Salary { get; set; }
        public DateTime? SalaryAt { get; set; }
        public DateTime? StartDateAt { get; set; }
        public Double? AllowedLeaveDay { get; set; }
        [ApplySearchAttribute]
        public string UserCode { get; set; }
        public string JobTitle { get; set; }
        public UserLevel? Level { get; set; }
        public string RegisterWorkDay { get; set; }
        public string AvatarPath { get; set; }
        public string AvatarFullPath => FileUtils.FullFilePath(AvatarPath);
        public long? ManagerId { get; set; }
        public string ManagerAvatarPath { get; set; }
        public string ManagerAvatarFullPath => FileUtils.FullFilePath(ManagerAvatarPath);
        public string ManagerName { get; set; }
        public Sex? Sex { get; set; }
        public DateTime CreationTime { get; set; }
        [MaxLength(5)]
        public double? MorningWorking { get; set; }
        [MaxLength(5)]
        public string MorningStartAt { get; set; }
        [MaxLength(5)]
        public string MorningEndAt { get; set; }
        [MaxLength(5)]
        public double? AfternoonWorking { get; set; }
        [MaxLength(5)]
        public string AfternoonStartAt { get; set; }
        [MaxLength(5)]
        public string AfternoonEndAt { get; set; }
        public long? BranchId { get; set; }
        public string BranchDisplayName { get; set; }
        public long? PositionId { get; set; }
        public string PositionName { get; set; }
    }
}

