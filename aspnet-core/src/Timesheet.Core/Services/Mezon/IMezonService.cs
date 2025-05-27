using Abp.Domain.Services;
using System;
using System.Threading.Tasks;
using Timesheet.Services.Mezon.Dto;
namespace Timesheet.Services.Mezon
{
    public interface IMezonService
    {
        // Task<OpenTalkListDto[]> GetOpenTalkLogAsync(DateTime? day = null);
        OpenTalkListDto[] GetOpenTalkLog(DateTime? day = null);
    }
}

