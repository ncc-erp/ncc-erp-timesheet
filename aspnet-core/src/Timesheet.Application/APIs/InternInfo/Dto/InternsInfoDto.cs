using Abp.Extensions;
using System;
using System.Collections.Generic;
using Timesheet.Paging;
using Timesheet.Uitls;
using static Ncc.Entities.Enum.StatusEnum;

namespace Timesheet.APIs.InternInfo.Dto
{
    public class InternsInfoOutputDto
    {
        public GridResult<InternsInfoDto> ListInternInfo { get; set; }
        public List<string> ListMonth { get; set; }
    }
    public class InternsInfoDto
    {
        public InternsInfoUserInfo MyInfo { get; set; }
        public List<MonthInfo> ReviewDetails { get; set; }
        public string BasicTrannerFullName{ get; set; }
        public UserLevel? BeginLevel { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
    }
    public class InternsInfoUserInfo
    {
        public long Id { get; set; }
        public string FullName { get; set; }
        public string EmailAddress { get; set; }
        public string AvatarPath { get; set; }
        public string AvatarFullPath => FileUtils.FullFilePath(AvatarPath);
        public Branch? Branch { get; set; }
        public UserLevel? Level { get; set; }
        public Usertype? Type { get; set; }
        public string BranchColor { get; set; }
        public string BranchDisplayName { get; set; }
        public bool IsActive { get; set; }
    }
    public class MonthInfo
    {
        public UserLevel? NewLevel { get; set; }
        public float? RateStar { get; set; }
        public int Month { get; set; }
        public int Year { get; set; }
        public string ReviewerName { get; set; }
        //public int IndexColumnExcel { get; set; }
        public bool IsStart { get; set; }
        public bool IsStop { get; set; }
        public string Display { get; set; }
        public bool HasReview => !Display.IsNullOrEmpty();
        public bool BeStaff => NewLevel.HasValue && NewLevel >= UserLevel.FresherMinus;
        public string Note { get; set; }
        public CellColor CellColor
        {
            get
            {
                if(IsStart && IsStop)
                    return CellColor.BeginAndEnd;
                if(IsStart && BeStaff)
                    return CellColor.BeginAndStaff;
                if (IsStart && NewLevel.HasValue)
                    return CellColor.BeginHasRivew;
                if (IsStop && NewLevel.HasValue)
                    return CellColor.EndHasRivew;
                if(IsStart)
                    return CellColor.Begin;
                if (IsStop)
                    return CellColor.End;
                if(BeStaff)
                    return CellColor.Staff;
                return CellColor.Normal;    
            }
        }
    }
    public enum CellColor
    {
        Normal,
        Begin,
        Staff,
        End,
        BeginHasRivew,
        EndHasRivew,
        BeginAndEnd,
        BeginAndStaff
    }
}
