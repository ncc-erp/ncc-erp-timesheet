using Abp.Application.Services.Dto;
using Abp.AutoMapper;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;
using Timesheet.Entities;
using static Ncc.Entities.Enum.StatusEnum;

namespace Timesheet.DomainServices.Dto
{
    public class GetUserWhitelistDto
    {
        public long Id { get; set; }
        public long UserId { get; set; }
        public string FullName { get; set; }
        public string UserName { get; set; }
        public string WhitelistName { get; set; }
        public WhitelistType WhitelistType { get; set; }
    }

    public class AddUserWhitelistDto
    {
        [Required]
        public long UserId { get; set; }
        [Required]
        public long WhitelistSystemId { get; set; }
    }

    public class ImportUserWhitelistRowDto
    {
        public int Row { get; set; }
        public string MezonUserId { get; set; }
        public string Email { get; set; }
        public string WhitelistType { get; set; }
        public long? UserId { get; set; }
    }

    public class ImportUserWhitelistResultDto
    {
        public int SuccessCount { get; set; }
        public int FailCount { get; set; }
        public List<string> FailedList { get; set; }
    }
}
