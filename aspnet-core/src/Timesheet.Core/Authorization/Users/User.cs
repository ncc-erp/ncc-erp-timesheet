using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Abp.Authorization.Users;
using Abp.Extensions;
using static Ncc.Entities.Enum.StatusEnum;

namespace Ncc.Authorization.Users
{
    public class User : AbpUser<User>
    {
        public const string DefaultPassword = "123qwe";

        public static string CreateRandomPassword()
        {
            return Guid.NewGuid().ToString("N").Truncate(16);
        }

        public static User CreateTenantAdminUser(int tenantId, string emailAddress)
        {
            var user = new User
            {
                TenantId = tenantId,
                UserName = AdminUserName,
                Name = AdminUserName,
                Surname = AdminUserName,
                EmailAddress = emailAddress,
            };

            user.SetNormalizedNames();

            return user;
        }

        public string Address { get; set; }
        public Usertype? Type { get; set; }
        public int? Salary { get; set; }
        public DateTime? SalaryAt { get; set; }
        public DateTime? StartDateAt { get; set; }
        public Double? AllowedLeaveDay { get; set; }
        [MaxLength(256)]
        public string UserCode { get; set; }
        public string RegisterWorkDay { get; set; }
        public long? ManagerId { get; set; }
        public string JobTitle { get; set; }
        public UserLevel? Level { get; set; }
        public Entities.Enum.StatusEnum.Branch? BranchOld { get; set; }
        public string AvatarPath { get; set; }

        public Sex? Sex { get; set; }
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
        public ulong? KomuUserId { get; set; }
        public bool? isWorkingTimeDefault { get; set; }
        public bool IsStopWork { get; set; }

        public long? DefaultProjectTaskId { get; set; }
        public UserLevel? BeginLevel { get; set; }
        public DateTime? EndDateAt { get; set; }
        public long? BranchId { get; set; }
        public long? PositionId { get; set; }

        #region Foreign Keys

        [ForeignKey(nameof(ManagerId))]
        public User Manager { get; set; }

        [ForeignKey(nameof(BranchId))]
        public Timesheet.Entities.Branch Branch { get; set; }

        [ForeignKey(nameof(PositionId))]
        public Timesheet.Entities.Position Position { get; set; }

        #endregion
    }
}
