using Abp.Application.Services;
using Abp.Application.Services.Dto;
using Ncc.MultiTenancy.Dto;

namespace Ncc.MultiTenancy
{
    public interface ITenantAppService : IAsyncCrudAppService<TenantDto, int, PagedTenantResultRequestDto, CreateTenantDto, TenantDto>
    {
    }
}

