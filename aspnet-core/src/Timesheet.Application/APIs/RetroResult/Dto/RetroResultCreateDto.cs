using Abp.AutoMapper;
using Abp.Domain.Entities;
using System.Collections.Generic;
using Timesheet.Entities;
using static Ncc.Entities.Enum.StatusEnum;
using Timesheet.Paging;
using Timesheet.Extension;

namespace Timesheet.APIs.RetroDetails.Dto
{
    [AutoMapTo(typeof(RetroResult))]
    public class RetroResultCreateDto : Entity<long>
    {
        public double Point { get; set; }
        public string Note { get; set; }
        public long UserId { get; set; }
        public long ProjectId { get; set; }
        public long PositionId { get; set; }
        public long RetroId { get; set; }
        public long? PmId { get; set; }
    }

    public class InputMultiFilterRetroResultPagingDto
    {
        public GridParam GridParam { get; set; }
        public List<long> ProjecIds { get; set; }
        public List<UserLevel> Userlevels { get; set; }
        public List<Usertype> Usertypes { get; set; }
        public List<long> BranchIds { get; set; }
        public List<long> PositionIds { get; set; }
        public float LeftPoint { get; set; }
        public float RightPoint { get; set; }
    }
}