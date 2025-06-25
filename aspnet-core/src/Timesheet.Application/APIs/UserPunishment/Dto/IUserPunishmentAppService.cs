using Abp.Application.Services;
using Timesheet.APIs.UserPunishments.Dto;
using Abp.Application.Services.Dto;
using System.Threading.Tasks;

namespace TimesheetApplication.UserPunishment
{
    public interface IUserPunishmentAppService : IApplicationService
    {
        Task<UserPunishmentDto> CreateUserPunishmentAsync(CreateUserPunishmentDto input);
        Task<UserPunishmentDto> GetUserPunishmentAsync(EntityDto<long> input);
        Task UpdateUserPunishmentAsync(UpdateUserPunishmentDto input);
        Task DeleteUserPunishmentAsync(EntityDto<long> input);
        //Task<int> ApplyPMReportPunishmentsAsync();
        ////Task<PagedResultDto<UserPunishmentDto>> GetUserPunishmentsAsync(GetUserPunishmentsInput input);
    }
}