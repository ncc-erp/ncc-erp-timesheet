using System;
using System.Collections.Generic;
using System.Text;
using Timesheet.Paging;
using Timesheet.Uitls;
using Timesheet.Extension;

namespace Timesheet.APIs.InternInfo.Dto
{
    public class InputInternInfoDto : GridParam
    {
        public DateFilterType DateFilterType { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public List<long> BasicTrainerIds { get; set; }
        public List<long> BranchIds { get; set; }
        public bool IsFilterBranch => BranchIds != null && !BranchIds.IsEmpty();
        public bool IsFilterTrainer => BasicTrainerIds != null && !BasicTrainerIds.IsEmpty();
    }
    public enum DateFilterType
    {
        OnBoardDate,
        BeStaffDate,
        OutDate
    }
}
