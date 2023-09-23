using Abp.AutoMapper;
using Abp.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Text;
using Timesheet.Entities;
using static Ncc.Entities.Enum.StatusEnum;

namespace Timesheet.APIs.RetroDetails.Dto
{
    [AutoMapTo(typeof(RetroResult))]
    public class RetroResultEditDto : Entity<long>
    {
        public long ProjectId { get; set; }
        public long PositionId { get; set; }
        public long? BranchId { get; set; }
        public Usertype? UserType { get; set; }
        public UserLevel? UserLevel { get; set; }
        public double Point { get; set; }
        public string Note { get; set; }
        public long? PmId { get; set; }
    }
}
