using Abp.Application.Services.Dto;

namespace Timesheet.APIs.TeamBuildingRequestHistories.dto
{
    public class RequestHistoryFileDto : EntityDto<long>
    {
        public string FileName { get; set; }
        public string Url { get; set; }
        public long RequestHistoryId { get; set; }
    }
}
