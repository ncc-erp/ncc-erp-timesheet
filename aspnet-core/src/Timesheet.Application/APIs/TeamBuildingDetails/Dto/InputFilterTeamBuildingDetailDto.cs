using Timesheet.Paging;
using static Ncc.Entities.Enum.StatusEnum;

namespace Timesheet.APIs.TeamBuildingDetails.Dto
{
    public class InputFilterTeamBuildingDetailPagingDto
    {
        public GridParam GridParam { get; set; }
        public int Year { get; set; }
        public int? Month { get; set; }
        public TeamBuildingStatus? Status { get; set; }
    }
}
