using System.Linq;
using Microsoft.EntityFrameworkCore;
using Abp.Authorization;
using Abp.Authorization.Roles;
using Abp.Authorization.Users;
using Abp.MultiTenancy;
using Ncc.Authorization;
using Ncc.Authorization.Roles;
using Ncc.Authorization.Users;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;

namespace Ncc.EntityFrameworkCore.Seed.Host
{
    public class HostRoleAndUserCreator
    {
        private readonly TimesheetDbContext _context;

        public HostRoleAndUserCreator(TimesheetDbContext context)
        {
            _context = context;
        }

        public void Create()
        {
            CreateHostRoleAndUsers();
        }

        private void CreateHostRoleAndUsers()
        {
            // Admin role for host

            var adminRoleForHost = _context.Roles.IgnoreQueryFilters().FirstOrDefault(r => r.TenantId == null && r.Name == StaticRoleNames.Host.Admin);
            if (adminRoleForHost == null)
            {
                adminRoleForHost = _context.Roles.Add(new Role(null, StaticRoleNames.Host.Admin, StaticRoleNames.Host.Admin) { IsStatic = false, IsDefault = false }).Entity;
                _context.SaveChanges();
            }

            // Grant all permissions to admin role for host

            var grantedPermissions = _context.Permissions.IgnoreQueryFilters()
                .OfType<RolePermissionSetting>()
                .Where(p => p.TenantId == null && p.RoleId == adminRoleForHost.Id)
                .Select(p => p.Name)
                .ToList();

            //var permissions = PermissionFinder
            //    .GetAllPermissions(new TimesheetAuthorizationProvider())
            //    .Where(p => p.MultiTenancySides.HasFlag(MultiTenancySides.Host) &&
            //                !grantedPermissions.Contains(p.Name))
            //    .ToList();

           
            var permissions = GrantPermissionRoles.PermissionRoles[StaticRoleNames.Host.Admin].Except(grantedPermissions);
            if (permissions.Any())
            {
                _context.Permissions.AddRange(
                    permissions.Select(permission => new RolePermissionSetting
                    {
                        TenantId = null,
                        Name = permission,
                        IsGranted = true,
                        RoleId = adminRoleForHost.Id
                    })
                );

            }

            // Admin user for host
            var adminUserForHost = _context.Users.IgnoreQueryFilters().FirstOrDefault(u => u.TenantId == null && u.UserName == AbpUserBase.AdminUserName);
            if (adminUserForHost == null)
            {
                var user = new User
                {
                    TenantId = null,
                    UserName = AbpUserBase.AdminUserName,
                    Name = "admin",
                    Surname = "admin",
                    EmailAddress = "admin@aspnetboilerplate.com",
                    IsEmailConfirmed = true,
                    IsActive = true
                };

                user.Password = new PasswordHasher<User>(new OptionsWrapper<PasswordHasherOptions>(new PasswordHasherOptions())).HashPassword(user, "123qwe");
                user.SetNormalizedNames();

                adminUserForHost = _context.Users.Add(user).Entity;
                _context.SaveChanges();

                // Assign Admin role to admin user
                _context.UserRoles.Add(new UserRole(null, adminUserForHost.Id, adminRoleForHost.Id));
                _context.SaveChanges();

                //_context.SaveChanges();
            }

            // Project Admin role for host

            var projectAdminRoleForHost = _context.Roles.IgnoreQueryFilters().FirstOrDefault(r => r.TenantId == null && r.Name == StaticRoleNames.Host.ProjectAdmin);
            if (projectAdminRoleForHost == null)
            {
                projectAdminRoleForHost = _context.Roles.Add(new Role(null, StaticRoleNames.Host.ProjectAdmin, StaticRoleNames.Host.ProjectAdmin) { IsStatic = false, IsDefault = false }).Entity;
                _context.SaveChanges();
            }

            // Grant all permissions to project admin role for host

            grantedPermissions = _context.Permissions.IgnoreQueryFilters()
                .OfType<RolePermissionSetting>()
                .Where(p => p.TenantId == null && p.RoleId == projectAdminRoleForHost.Id)
                .Select(p => p.Name)
                .ToList();

            permissions = GrantPermissionRoles.PermissionRoles[StaticRoleNames.Host.ProjectAdmin].Except(grantedPermissions);
            //permissions = PermissionFinder
            //    .GetAllPermissions(new TimesheetAuthorizationProvider())
            //    .Where(p => p.MultiTenancySides.HasFlag(MultiTenancySides.Host) &&
            //                !grantedPermissions.Contains(p.Name) && GrantPermissionRoles.PermissionRoles[StaticRoleNames.Host.ProjectAdmin].Contains(p.Name))
            //    .ToList();

            if (permissions.Any())
            {
                _context.Permissions.AddRange(
                    permissions.Select(permission => new RolePermissionSetting
                    {
                        TenantId = null,
                        Name = permission,
                        IsGranted = true,
                        RoleId = projectAdminRoleForHost.Id
                    })
                );
            }

            // basic user role for host

            var basicUserRoleForHost = _context.Roles.IgnoreQueryFilters().FirstOrDefault(r => r.TenantId == null && r.Name == StaticRoleNames.Host.BasicUser);
            if (basicUserRoleForHost == null)
            {
                basicUserRoleForHost = _context.Roles.Add(new Role(null, StaticRoleNames.Host.BasicUser, StaticRoleNames.Host.BasicUser) { IsStatic = false, IsDefault = true }).Entity;
                _context.SaveChanges();
            }


            // Grant all permissions to project admin role for host

            grantedPermissions = _context.Permissions.IgnoreQueryFilters()
                .OfType<RolePermissionSetting>()
                .Where(p => p.TenantId == null && p.RoleId == basicUserRoleForHost.Id)
                .Select(p => p.Name)
                .ToList();

            permissions = GrantPermissionRoles.PermissionRoles[StaticRoleNames.Host.BasicUser].Except(grantedPermissions);
            //permissions = PermissionFinder
            //    .GetAllPermissions(new TimesheetAuthorizationProvider())
            //    .Where(p => p.MultiTenancySides.HasFlag(MultiTenancySides.Host) &&
            //                !grantedPermissions.Contains(p.Name) && GrantPermissionRoles.PermissionRoles[StaticRoleNames.Host.BasicUser].Contains(p.Name))
            //    .ToList();

            if (permissions.Any())
            {
                _context.Permissions.AddRange(
                    permissions.Select(permission => new RolePermissionSetting
                    {
                        TenantId = null,
                        Name = permission,
                        IsGranted = true,
                        RoleId = basicUserRoleForHost.Id
                    })
                );
            }

            //Supervisor role for host

            var supervisorRoleForHost = _context.Roles.IgnoreQueryFilters().FirstOrDefault(r => r.TenantId == null && r.Name == StaticRoleNames.Host.Supervisor);
            if (supervisorRoleForHost == null)
            {
                supervisorRoleForHost = _context.Roles.Add(new Role(null, StaticRoleNames.Host.Supervisor, StaticRoleNames.Host.Supervisor) { IsStatic = false, IsDefault = false }).Entity;
                _context.SaveChanges();
            }

            // Grant all permissions to supervisor role for host

            grantedPermissions = _context.Permissions.IgnoreQueryFilters()
                .OfType<RolePermissionSetting>()
                .Where(p => p.TenantId == null && p.RoleId == supervisorRoleForHost.Id)
                .Select(p => p.Name)
                .ToList();

            permissions = GrantPermissionRoles.PermissionRoles[StaticRoleNames.Host.Supervisor].Except(grantedPermissions);
            //permissions = PermissionFinder
            //   .GetAllPermissions(new TimesheetAuthorizationProvider())
            //   .Where(p => p.MultiTenancySides.HasFlag(MultiTenancySides.Host) &&
            //               !grantedPermissions.Contains(p.Name) && GrantPermissionRoles.PermissionRoles[StaticRoleNames.Host.Supervisor].Contains(p.Name))
            //   .ToList();

            if (permissions.Any())
            {
                _context.Permissions.AddRange(
                    permissions.Select(permission => new RolePermissionSetting
                    {
                        TenantId = null,
                        Name = permission,
                        IsGranted = true,
                        RoleId = supervisorRoleForHost.Id
                    })
                );
            }



            //// News Admin role for host

            //var newsAdminRoleForHost = _context.Roles.IgnoreQueryFilters().FirstOrDefault(r => r.TenantId == null && r.Name == StaticRoleNames.Host.NewsAdmin);
            //if (newsAdminRoleForHost == null)
            //{
            //    newsAdminRoleForHost = _context.Roles.Add(new Role(null, StaticRoleNames.Host.NewsAdmin, StaticRoleNames.Host.NewsAdmin) { IsStatic = true, IsDefault = false }).Entity;
            //    _context.SaveChanges();
            //}

            //// Grant all permissions to project admin role for host

            //grantedPermissions = _context.Permissions.IgnoreQueryFilters()
            //    .OfType<RolePermissionSetting>()
            //    .Where(p => p.TenantId == null && p.RoleId == newsAdminRoleForHost.Id)
            //    .Select(p => p.Name)
            //    .ToList();

            //permissions = PermissionFinder
            //    .GetAllPermissions(new TimesheetAuthorizationProvider())
            //    .Where(p => p.MultiTenancySides.HasFlag(MultiTenancySides.Host) &&
            //                !grantedPermissions.Contains(p.Name) && GrantPermissionRoles.PermissionRoles[StaticRoleNames.Host.NewsAdmin].Contains(p.Name))
            //    .ToList();

            //if (permissions.Any())
            //{
            //    _context.Permissions.AddRange(
            //        permissions.Select(permission => new RolePermissionSetting
            //        {
            //            TenantId = null,
            //            Name = permission.Name,
            //            IsGranted = true,
            //            RoleId = newsAdminRoleForHost.Id
            //        })
            //    );
            //}


            // BranchDirector role for host
            var branchDirectorRoleForHost = _context.Roles.IgnoreQueryFilters().FirstOrDefault(r => r.TenantId == null && r.Name == StaticRoleNames.Host.BranchDirector);
            if (branchDirectorRoleForHost == null)
            {
                branchDirectorRoleForHost = _context.Roles.Add(new Role(null, StaticRoleNames.Host.BranchDirector, StaticRoleNames.Host.BranchDirector) { IsStatic = false, IsDefault = false }).Entity;
                _context.SaveChanges();
            }

            // Grant all permissions to Branch Director role for host
            grantedPermissions = _context.Permissions.IgnoreQueryFilters()
                .OfType<RolePermissionSetting>()
                .Where(p => p.TenantId == null && p.RoleId == branchDirectorRoleForHost.Id)
                .Select(p => p.Name)
                .ToList();

            permissions = GrantPermissionRoles.PermissionRoles[StaticRoleNames.Host.BranchDirector].Except(grantedPermissions);

            if (permissions.Any())
            {
                _context.Permissions.AddRange(
                    permissions.Select(permission => new RolePermissionSetting
                    {
                        TenantId = null,
                        Name = permission,
                        IsGranted = true,
                        RoleId = branchDirectorRoleForHost.Id
                    })
                );
            }
        }
    }
}
