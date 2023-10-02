using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;
using static Ncc.Entities.Enum.StatusEnum;
using Timesheet.Anotations;
using Timesheet.Uitls;
using Timesheet.Users.Dto;
using Abp.Application.Services.Dto;

namespace Timesheet.APIs.ProjectManagementBranchDirectors.ManageUserForBranch.Dto
{
    public class UserProjectsDto : EntityDto<long>
    {
        [ApplySearch]
        public string UserName { get; set; }

        [ApplySearch]
        public string Name { get; set; }

        [ApplySearch]
        public string Surname { get; set; }

        [ApplySearch]
        public string EmailAddress { get; set; }

        public string PhoneNumber { get; set; }

        public string Address { get; set; }

        public bool IsActive { get; set; }

        public string FullName { get; set; }
        public IEnumerable<PUDto> ProjectUsers { get; set; }
        public Usertype? Type { get; set; }
        public UserLevel? Level { get; set; }
        [ApplySearch]
        public string UserCode { get; set; }
        public string AvatarPath { get; set; }
        public string AvatarFullPath => FileUtils.FullFilePath(AvatarPath);
        public long? ManagerId { get; set; }
        public string ManagerAvatarPath { get; set; }
        public string ManagerAvatarFullPath => FileUtils.FullFilePath(ManagerAvatarPath);
        public string ManagerName { get; set; }
        public Sex? Sex { get; set; }
        public DateTime CreationTime { get; set; }
        public long? BranchId { get; set; }
        public string BranchDisplayName { get; set; }
        public long? PositionId { get; set; }
        public string PositionName { get; set; }
        public long? ProjectCount { get; set; }
    }
}
