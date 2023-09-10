using System;
using System.ComponentModel.DataAnnotations;
using System.Web.Http.ModelBinding;
using Abp.Auditing;
using Abp.Authorization.Users;
using Abp.AutoMapper;
using Abp.Runtime.Validation;
using Microsoft.AspNetCore.Http;
using Ncc.Authorization.Users;
using Newtonsoft.Json;
using Timesheet.Uitls;
using static Ncc.Entities.Enum.StatusEnum;

namespace Timesheet.DomainServices.Dto
{
    [AutoMapTo(typeof(User))]
    public class CreateUserDto : IShouldNormalize
    {
        public long Id { get; set; }
        public string UserName { get; set; }

        public string Name { get; set; }

        public string Surname { get; set; }

        public string EmailAddress { get; set; }
        public string PhoneNumber { get; set; }
        public string Address { get; set; }

        public bool IsActive { get; set; }

        public string[] RoleNames { get; set; }

        public string Password { get; set; }
        public Usertype? Type { get; set; }
        public String JobTitle { get; set; }
        public UserLevel? Level { get; set; }
        public string RegisterWorkDay { get; set; }
        public Double? AllowedLeaveDay { get; set; }
        public DateTime? StartDateAt { get; set; }
        public int? Salary { get; set; }
        public DateTime? SalaryAt { get; set; }
        public string UserCode { get; set; }
        public void Normalize()
        {
            if (RoleNames == null)
            {
                RoleNames = new string[0];
            }
        }
        public long? ManagerId { get; set; }
        public Sex? Sex { get; set; }
        public string MorningWorking { get; set; }
        public string MorningStartAt { get; set; }
        public string MorningEndAt { get; set; }
        public string AfternoonWorking { get; set; }
        public string AfternoonStartAt { get; set; }
        public string AfternoonEndAt { get; set; }
        public bool? isWorkingTimeDefault { get; set; }
        public long? BranchId { get; set; }
        public string BranchCode { get; set; }
        public string AvatarPath { get; set; }
        public UserLevel? BeginLevel { get; set; }
        public DateTime? EndDateAt { get; set; }
        public long? PositionId { get; set; }
    }
}
