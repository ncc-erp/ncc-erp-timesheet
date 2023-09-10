using Abp.Application.Services.Dto;
using Abp.AutoMapper;

namespace Timesheet.APIs.TeamBuildingDetailsPM.Dto
{
    [AutoMapTo]
    public class TeamBuildingRequestHistoryFileDto : EntityDto<long>
    {
        public string FileName { get; set; }
        public string Url { get; set; }
    }
}
