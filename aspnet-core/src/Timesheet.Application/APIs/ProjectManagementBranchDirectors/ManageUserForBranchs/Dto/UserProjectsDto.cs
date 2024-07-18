using Abp.Application.Services.Dto;
using System;
using System.Collections.Generic;
using System.Text;
using static Ncc.Entities.Enum.StatusEnum;
using Timesheet.Anotations;
using Timesheet.Uitls;
using Timesheet.Users.Dto;
using Timesheet.Entities;

namespace Timesheet.APIs.ProjectManagementBranchDirectors.ManageUserForBranchs.Dto
{
    public class UserProjectsDto : EntityDto<long>
    {
        [ApplySearch]
        public string UserName { get; set; }

        [ApplySearch]
        public string EmailAddress { get; set; }

        public string FullName { get; set; }
        public IEnumerable<PUDto> ProjectUsers { get; set; }
        public Usertype? Type { get; set; }
        public UserLevel? Level { get; set; }
        public Sex? Sex { get; set; }
        public string AvatarPath { get; set; }
        public string AvatarFullPath => FileUtils.FullFilePath(AvatarPath);
        public long? BranchId { get; set; }
        public string BranchDisplayName { get; set; }
        public long? PositionId { get; set; }
        public string PositionName { get; set; }
        public long? ProjectCount { get; set; }
    }

    public class UserValueProjectDto
    {
        public long Id { get; set; }
        public string ProjectCode { get; set; }
        public string ProjectName { get; set; }
        public List<UserValueDto> Users { get; set; }
    }

    public class ProjectUserInfoDto
    {
        public long ProjectId { get; set; }
        public string Code { get; set; }
        public string Name { get; set; }
        public long UserId { get; set; }
        public long? BranchId { get; set; }
    }

    public class UserInfoProjectDto
    {
        public string FullName { get; set; }
        public string EmailAddress { get; set; }
        public long WorkingTime { get; set; }
        public ValueOfUserType ValueType { get; set; }
    }
}
