using Abp.Application.Services.Dto;

namespace Timesheet.APIs.Public.Dto
{
    public class KmProjectDto : EntityDto<long>
    {
        public string Name { get; set; }
        public string Code { get; set; }
        public string KomuChannelId { get; set; }
    }
}
