using System.Threading.Tasks;
using Abp.Application.Services;
using Ncc.Sessions.Dto;

namespace Ncc.Sessions
{
    public interface ISessionAppService : IApplicationService
    {
        Task<GetCurrentLoginInformationsOutput> GetCurrentLoginInformations();
    }
}
