using System;
using System.Collections.Generic;
using System.Text;

namespace Timesheet.APIs.TeamBuildingDetailsPM.Dto
{
    public class NotifyPMRequestingTeamBuildingDto
    {
        public ulong? KomuPmRequestId { get; set; }
        public string PmRequestEmailAddress { get; set; }
        public List<ProjectInfoTeamBuildingDto> ProjectInfos { get; set; }
        public string KomuAccountTagRequester()
        {
            return KomuPmRequestId.HasValue ? $"<@{KomuPmRequestId}>" : $"**{PmRequestEmailAddress}**";
        }
    }

    public class ProjectInfoTeamBuildingDto
    {
        public List<PmInfoTeambuildingDto> PmInfos { get; set; }
        public List<TeamBuildingDetailDto> Users { get; set; }
        public int CountUsers => Users.Count;
    }

    public class PmInfoTeambuildingDto
    {
        public ulong? KomuPmId { get; set; }
        public string PmEmailAddress { get; set; }
        public string KomuAccountTag()
        {
            return KomuPmId.HasValue ? $"<@{KomuPmId}>" : $"**{PmEmailAddress}**";
        }
    }

    public class TeamBuildingDetailDto
    {
        public long Id { get; set; }
        public long EmployeeId { get; set; }
        public string EmployeeName { get; set; }
        public double Money { get; set; }
        public int Month { get; set; }
        public int Year { get; set; }
    }
}
