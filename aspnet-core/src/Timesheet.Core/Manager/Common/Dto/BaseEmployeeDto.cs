using Abp.Application.Services.Dto;
using System;
using System.Collections.Generic;
using System.Text;
using static Ncc.Entities.Enum.StatusEnum;
using Timesheet.Anotations;
using Timesheet.Uitls;
using Timesheet.Extension;

namespace Timesheet.Manager.Common.Dto
{
    public class BaseEmployeeDto : EntityDto<long>
    {
        [ApplySearch]
        public string FullName { get; set; }
        [ApplySearch]
        public string Email { get; set; }
        public Sex Sex { get; set; }
        public Usertype UserType { get; set; }
        public long LevelId { get; set; }
        public BadgeInfoDto LevelInfo { get; set; }
        public long BranchId { get; set; }
        public BadgeInfoDto BranchInfo { get; set; }
        public long JobPositionId { get; set; }
        public BadgeInfoDto JobPositionInfo { get; set; }
        public string Avatar { get; set; }
        public string UserTypeName => Enum.GetName(typeof(Usertype), UserType);
        public BadgeInfoDto UserTypeInfo
        {
            get
            {
                return new BadgeInfoDto
                {
                   // Name = CommonUtils.GetUserTypeByTypeFromHrmV2(UserType).Name
                   // Color = CommonUtils.GetUserTypeByTypeFromHrmV2(UserType).Color
                };
            }
        }
    }

    public class BadgeInfoDto
    {
        public string Name { get; set; }
        public string Color { get; set; }
    }
}
