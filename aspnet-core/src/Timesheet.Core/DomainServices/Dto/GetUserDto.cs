using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Abp.Application.Services.Dto;
using Abp.Authorization.Users;
using Abp.AutoMapper;
using Ncc.Authorization.Users;
using Timesheet.Anotations;
using Timesheet.Uitls;
using static Ncc.Entities.Enum.StatusEnum;

namespace Timesheet.DomainServices.Dto
{
    public class GetUserDto : EntityDto<long>
    {
        public string Name { get; set; }
        public string EmailAddress { get; set; }
        public bool IsActive { get; set; }
        public Usertype? Type { get; set; }
        public string JobTitle { get; set; }
        public UserLevel? Level { get; set; }
        public string UserCode { get; set; }
        public string AvatarPath { get; set; }
        public string AvatarFullPath => FileUtils.FullFilePath(AvatarPath);
        public Branch? Branch { get; set; }
        public string BranchColor { get; set; }
        public string BranchDisplayName { get; set; }
        public long? BranchId { get; set; }
        public long? PositionId { get; set; }
        public string PositionName { get; set; }
    }
    
}

