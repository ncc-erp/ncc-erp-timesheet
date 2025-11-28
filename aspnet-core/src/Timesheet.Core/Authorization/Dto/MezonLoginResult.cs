using Abp.Authorization;
using Abp.Authorization.Users;
using Ncc.Authorization.Roles;
using Ncc.Authorization.Users;
using Ncc.MultiTenancy;

namespace Ncc.Authorization.Dto
{
    public class MezonLoginResult
    {
        public AbpLoginResult<Tenant, User> LoginResult { get; set; }
        public string IdToken { get; set; }
    }
}