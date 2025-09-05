using Abp.Application.Services;
using System.Threading.Tasks;
using Timesheet.APIs.BotReportDaily.Dto;

namespace Timesheet.APIs.BotReportDaily
{
    public interface IBotReportDailyAppService : IApplicationService
    {
        Task<DailyProjectTimelogReportDto> GetDailyProjectTimelogReport(GetDailyProjectTimelogReportInput input);
        
        Task<GoogleSheetsReportDto> GetDailyProjectTimelogGoogleSheetsReport(GetGoogleSheetsReportInput input);

        Task<bool> SendDailyProjectTimelogToMezon(GetDailyProjectTimelogReportInput input);
    }
}
