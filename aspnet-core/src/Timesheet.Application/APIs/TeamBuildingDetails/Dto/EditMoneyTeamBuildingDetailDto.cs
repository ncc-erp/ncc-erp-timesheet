using Abp.AutoMapper;
using Abp.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Text;
using Timesheet.Entities;

namespace Timesheet.APIs.TeamBuildingDetails.Dto
{
    [AutoMapTo(typeof(TeamBuildingDetail))]
    public class EditMoneyTeamBuildingDetailDto : Entity<long>
    {
        public float Money { get; set; }
    }
}
