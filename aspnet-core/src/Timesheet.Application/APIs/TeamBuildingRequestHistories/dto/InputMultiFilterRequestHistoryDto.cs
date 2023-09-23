using Timesheet.Paging;
using static Ncc.Entities.Enum.StatusEnum;

namespace Timesheet.APIs.TeamBuildingRequestHistories.dto
{
    public class InputMultiFilterRequestHistoryDto
    {
        public GridParam GridParam { get; set; }
        public int Year { get; set; }
        public int? Month { get; set; }
        public TeamBuildingRequestStatus? Status { get; set; }
    }
}
