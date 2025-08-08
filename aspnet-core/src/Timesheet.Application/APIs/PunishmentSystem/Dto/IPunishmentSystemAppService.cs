using Abp.Application.Services;
using Abp.Application.Services.Dto;
using System.Threading.Tasks;
using Timesheet.APIs.PunishmentSystems.Dto;

namespace TimesheetApplication.PunishmentSystem
{
    public interface IPunishmentSystemAppService : IApplicationService
    {
        Task<PunishmentSystemDto> CreatePunishmentSystemAsync(CreatePunishmentSystemDto input);
        Task<PunishmentSystemDto> GetPunishmentSystemAsync(EntityDto<long> input);
        Task UpdatePunishmentSystemAsync(UpdatePunishmentSystemDto input);
        Task DeletePunishmentSystemAsync(EntityDto<long> input);
        //Task<PagedResultDto<PunishmentSystemDto>> GetPunishmentSystemsAsync(GetPunishmentSystemsInput input);
        //Task<ListResultDto<PunishmentSystemDto>> GetAllActivePunishmentSystemsAsync();
        Task<PagedResultDto<PunishmentSystemDto>> GetAllActivePunishmentSystemsAsync(GetAllActivePunishmentSystemsInput input);
    }
}