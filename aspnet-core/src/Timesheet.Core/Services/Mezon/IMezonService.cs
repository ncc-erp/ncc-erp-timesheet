using Abp.Domain.Services;
using System.Threading.Tasks;
using Timesheet.Services.Mezon.Dto;
namespace Timesheet.Services.Mezon
{
    public interface IMezonService
    {
        Task<OpenTalkListDto[]> GetOpenTalkLog();
    }
}
