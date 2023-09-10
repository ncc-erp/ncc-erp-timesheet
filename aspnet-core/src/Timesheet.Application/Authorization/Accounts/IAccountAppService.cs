using System.Threading.Tasks;
using Abp.Application.Services;
using Ncc.Authorization.Accounts.Dto;

namespace Ncc.Authorization.Accounts
{
    public interface IAccountAppService : IApplicationService
    {
        Task<IsTenantAvailableOutput> IsTenantAvailable(IsTenantAvailableInput input);

        Task<RegisterOutput> Register(RegisterInput input);
    }
}
