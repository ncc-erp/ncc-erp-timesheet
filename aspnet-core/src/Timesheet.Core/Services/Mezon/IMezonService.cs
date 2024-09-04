using Abp.Domain.Services;
using System;
using System.Threading.Tasks;
using Timesheet.Services.Mezon.Dto;
namespace Timesheet.Services.Mezon
{
    public interface IMezonService
    {
        OpenTalkListDto[] GetOpenTalkLog(DateTime? day = null);
    }
}
