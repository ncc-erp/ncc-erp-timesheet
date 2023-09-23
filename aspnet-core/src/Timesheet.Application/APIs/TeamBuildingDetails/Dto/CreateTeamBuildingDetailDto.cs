using Abp.AutoMapper;
using Abp.Domain.Entities;

namespace Timesheet.APIs.TeamBuildingDetails.Dto
{
    [AutoMapTo(typeof(Entities.TeamBuildingDetail))]
    public class CreateTeamBuildingDetailDto : Entity<long>
    {
        public int Month { get; set; }
        public int Year { get; set; }
        public long EmployeeId { get; set; }
    }
}
