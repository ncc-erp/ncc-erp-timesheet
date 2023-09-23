using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Abp.Application.Services;
using Abp.Application.Services.Dto;
using Abp.Authorization;
using Abp.Domain.Repositories;
using Abp.Extensions;
using Abp.IdentityFramework;
using Abp.Linq.Extensions;
using Ncc.Authorization;
using Ncc.Authorization.Roles;
using Ncc.Authorization.Users;
using Ncc.Roles.Dto;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Timesheet.Users.Dto;
using Ncc.IoC;
using Abp.Collections.Extensions;
using Ncc.Users;
using Abp.Authorization.Users;
using Timesheet.Roles.Dto;
using Timesheet.DomainServices;

namespace Ncc.Roles
{
    [AbpAuthorize(PermissionNames.Admin_Roles)]
    public class RoleAppService : AsyncCrudAppService<Role, RoleDto, int, PagedRoleResultRequestDto, CreateRoleDto, RoleDto>
    {
        private readonly RoleManager _roleManager;
        private readonly UserManager _userManager;
        private readonly UserServices _userServices;
        private List<PermissionDto> listPermisstion = new List<PermissionDto>();

        public RoleAppService(IRepository<Role> repository, RoleManager roleManager, UserManager userManager, UserServices userServices)
            : base(repository)
        {
            _roleManager = roleManager;
            _userManager = userManager;
            _userServices = userServices;
        }

        [AbpAuthorize(PermissionNames.Admin_Roles_AddNew)]
        public override async Task<RoleDto> Create(CreateRoleDto input)
        {
            CheckCreatePermission();

            var role = ObjectMapper.Map<Role>(input);
            role.SetNormalizedName();

            CheckErrors(await _roleManager.CreateAsync(role));

            return MapToEntityDto(role);
        }

        [AbpAuthorize(PermissionNames.Admin_Roles_View)]
        public async Task<ListResultDto<RoleListDto>> GetRolesAsync(GetRolesInput input)
        {
            var roles = await _roleManager
                .Roles
                .WhereIf(
                    !input.Permission.IsNullOrWhiteSpace(),
                    r => r.Permissions.Any(rp => rp.Name == input.Permission && rp.IsGranted)
                )
                .ToListAsync();

            return new ListResultDto<RoleListDto>(ObjectMapper.Map<List<RoleListDto>>(roles));
        }

        [AbpAuthorize(PermissionNames.Admin_Roles_Edit)]
        public override async Task<RoleDto> Update(RoleDto input)
        {
            CheckUpdatePermission();

            var role = await _roleManager.GetRoleByIdAsync(input.Id);

            ObjectMapper.Map(input, role);

            CheckErrors(await _roleManager.UpdateAsync(role));

            return input;
        }

        public async Task<RolePermissionDto> ChangeRolePermission(RolePermissionDto input)
        {
            CheckUpdatePermission();

            var role = await _roleManager.GetRoleByIdAsync(input.Id);

            var grantedPermissions = PermissionManager
               .GetAllPermissions()
               .Where(p => input.Permissions.Contains(p.Name))
               .ToList();

            await _roleManager.SetGrantedPermissionsAsync(role, grantedPermissions);

            return input;
        } 

        [AbpAuthorize(PermissionNames.Admin_Roles_Delete)]
        public override async Task Delete(EntityDto<int> input)
        {
            CheckDeletePermission();

            var role = await _roleManager.FindByIdAsync(input.Id.ToString());
            var users = await _userManager.GetUsersInRoleAsync(role.NormalizedName);
            foreach (var user in users)
            {
                var listRole = await _userManager.GetRolesAsync(user);
                bool flag = false;

                foreach (var r in listRole)
                {
                    if (r.Equals(role.Name))
                    {
                        listRole.Remove(r);
                        flag = true;
                        break;
                    }
                }

                if (!flag) listRole.Add(role.Name);

                string[] roles = listRole.ToArray();
                CheckErrors(await _userManager.SetRoles(user, roles));
            }

            CheckErrors(await _roleManager.DeleteAsync(role));
        }

        public Task<ListResultDto<SystemPermission>> GetAllPermissions()
        {
            var permissions = SystemPermission.TreePermissions;
            return Task.FromResult(new ListResultDto<SystemPermission>(permissions));
        }

        protected override IQueryable<Role> CreateFilteredQuery(PagedRoleResultRequestDto input)
        {
            return Repository.GetAllIncluding(x => x.Permissions)
                .WhereIf(!input.Keyword.IsNullOrWhiteSpace(), x => x.Name.Contains(input.Keyword)
                || x.DisplayName.Contains(input.Keyword)
                || x.Description.Contains(input.Keyword));
        }

        [AbpAuthorize(PermissionNames.Admin_Roles_View)]
        protected override async Task<Role> GetEntityByIdAsync(int id)
        {
            return await Repository.GetAllIncluding(x => x.Permissions).FirstOrDefaultAsync(x => x.Id == id);
        }

        protected override IQueryable<Role> ApplySorting(IQueryable<Role> query, PagedRoleResultRequestDto input)
        {
            return query.OrderBy(r => r.DisplayName);
        }

        protected virtual void CheckErrors(IdentityResult identityResult)
        {
            identityResult.CheckErrors(LocalizationManager);
        }

        [AbpAuthorize(PermissionNames.Admin_Roles_View)]
        public async Task<GetRoleForEditOutput> GetRoleForEdit(EntityDto input)
        {
            var permissions = SystemPermission.TreePermissions;
            var role = await _roleManager.GetRoleByIdAsync(input.Id);
            var grantedPermissions = (await _roleManager.GetGrantedPermissionsAsync(role)).ToArray();
            var users = (await _userManager.GetUsersInRoleAsync(role.NormalizedName)).ToArray();

            var roleEditDto = ObjectMapper.Map<RoleEditDto>(role);


            return new GetRoleForEditOutput
            {
                Role = roleEditDto,
                Permissions = permissions,
                GrantedPermissionNames = grantedPermissions.Select(p => p.Name).ToList(),
                Users = await _userServices.GetListUserByRoleId(input.Id)
            };
        }
    }
}

