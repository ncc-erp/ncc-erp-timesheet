using System;
using System.ComponentModel.DataAnnotations;
using Abp.Auditing;
using Abp.Authorization.Users;
using Abp.AutoMapper;
using Abp.Runtime.Validation;
using Ncc.Authorization.Users;
using static Ncc.Entities.Enum.StatusEnum;

namespace Ncc.Users.Dto
{
    [AutoMapTo(typeof(User))]
    public class CreateUserDto : IShouldNormalize
    {
        [Required]
        [StringLength(AbpUserBase.MaxUserNameLength)]
        public string UserName { get; set; }

        [Required]
        [StringLength(AbpUserBase.MaxNameLength)]
        public string Name { get; set; }

        [Required]
        [StringLength(AbpUserBase.MaxSurnameLength)]
        public string Surname { get; set; }

        [Required]
        [EmailAddress]
        [StringLength(AbpUserBase.MaxEmailAddressLength)]
        public string EmailAddress { get; set; }
        public string PhoneNumber { get; set; }
        public string Address { get; set; }

        public bool IsActive { get; set; }

        public string[] RoleNames { get; set; }

        [Required]
        [StringLength(AbpUserBase.MaxPlainPasswordLength)]
        [DisableAuditing]
        public string Password { get; set; }
        public Usertype? Type { get; set; }
        public String JobTitle { get; set; }
        public UserLevel? Level { get; set; }
        public string RegisterWorkDay { get; set; }
        public Double? AllowedLeaveDay { get; set; }
        public DateTime? StartDateAt { get; set; }
        public int? Salary { get; set; }
        public DateTime? SalaryAt { get; set; }
        [MaxLength(5)]
        public string UserCode { get; set; }
        public void Normalize()
        {
            if (RoleNames == null)
            {
                RoleNames = new string[0];
            }
        }
        public long? ManagerId { get; set; }
        public Branch Branch { get; set; }
        public Sex? Sex { get; set; }
        public string MorningWorking { get; set; }
        public string MorningStartAt { get; set; }
        public string MorningEndAt { get; set; }
        public string AfternoonWorking { get; set; }
        public string AfternoonStartAt { get; set; }
        public string AfternoonEndAt { get; set; }
    }
}
