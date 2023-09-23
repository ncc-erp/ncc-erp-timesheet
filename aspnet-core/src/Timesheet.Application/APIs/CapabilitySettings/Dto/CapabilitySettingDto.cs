using Abp.AutoMapper;
using Abp.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Text;
using Timesheet.Entities;
using Timesheet.Paging;
using static Ncc.Entities.Enum.StatusEnum;
namespace Timesheet.APIs.CapabilitySettings.Dto
{
    [AutoMapTo(typeof(CapabilitySetting))]
    public class CapabilitySettingDto : Entity<long>
    {
        public Usertype? UserType { get; set; }
        public string UserTypeName
        {
            get =>
             this.UserType.HasValue ? Enum.GetName(typeof(Usertype), UserType) : "";
        }
        public long? PositionId { get; set; }
        public string PositionName { get; set; }
        public long CapabilityId { get; set; }
        public string CapabilityName { get; set; }
        public CapabilityType? Type { get; set; }
        public string GuildeLine { get; set; }
        //public string Note { get; set; }
        public float Coefficient { get; set; }
        public bool IsDeleted { get; set; } = false;
    }
    public class CapabilityInCapabilitySettingDto : Entity<long>
    {
        public long CapabilityId { get; set; }
        public string CapabilityName { get; set; }
        public CapabilityType Type { get; set; }
        public string GuildeLine { get; set; }
        //public string Note { get; set; }
        public float Coefficient { get; set; }
    }
    public class GetPagingCapabilitySettingDto
    {
        public Usertype UserType { get; set; }
        public string UserTypeName { get => Enum.GetName(typeof(Usertype), UserType); }
        public long? PositionId { get; set; }
        public string PositionName { get; set; }
        public string GuildeLine { get; set; }
        public List<CapabilityInCapabilitySettingDto> Capabilities { get; set; }
    }
    public class CapabilitySettingCloneDto
    {
        public Usertype FromUserType { get; set; }
        public long FromPositionId { get; set; }
        public Usertype ToUserType { get; set; }
        public long ToPositionId { get; set; }
    }
    public class ResponseCapabilitySettingCloneDto : CapabilitySettingCloneDto
    {
        public Usertype ToUserType { get; set; }
        public string ToUserTypeName { get => Enum.GetName(typeof(Usertype), ToUserType); }
        public string ToPositionName { get; set; }
        public Usertype FromUserType { get; set; }
        public string FromUserTypeName { get => Enum.GetName(typeof(Usertype), FromUserType); }
        public string FromPositionName { get; set; }
    }
    public class ParamCapability
    {
        public GridParam Param { get; set; }
        public CapabilityType? Type { get; set; }
    }
    public class ListCapabilitySettingExist
    {
        public long Id { get; set; }
        public long CapabilityId { get; set; }
        public string GuildeLine { get; set; }
        public float Coefficient { get; set; }
    } 
}