using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Abp.Application.Services;
using Abp.Application.Services.Dto;
using Abp.Authorization;
using Abp.Domain.Entities;
using Abp.Domain.Repositories;
using Abp.Extensions;
using Abp.IdentityFramework;
using Abp.Linq.Extensions;
using Abp.Localization;
using Abp.Runtime.Session;
using Abp.UI;
using Ncc.Authorization;
using Ncc.Authorization.Accounts;
using Ncc.Authorization.Roles;
using Ncc.Authorization.Users;
using Ncc.Roles.Dto;
using Ncc.Users.Dto;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Abp.Net.Mail;
using Timesheet.Users.Dto;
using System;
using Ncc.IoC;
using Ncc.Entities;
using Timesheet.Paging;
using Abp.Authorization.Users;
using Timesheet.Extension;
using Abp.BackgroundJobs;
using Microsoft.AspNetCore.Mvc;
using System.IO;
using Timesheet.Helper;
using OfficeOpenXml;
using static Ncc.Entities.Enum.StatusEnum;
using Microsoft.AspNetCore.Hosting;
using Timesheet.DomainServices.Dto;
using Timesheet.DomainServices;
using Timesheet.Uitls;
using GetUserDto = Timesheet.DomainServices.Dto.GetUserDto;
using Timesheet.UploadFilesService;
using Timesheet.Services.Project;
using Timesheet.Services.Project.Dto;
using Timesheet.Services.HRM;
using Timesheet.Constants;
using Timesheet.Services.HRMv2;
using Timesheet.APIs.RetroDetails.Dto;
using System.Net.Mail;
using Timesheet.APIs.Public;
using Ncc.Net.MimeTypes;
using Timesheet.DataExport;

namespace Ncc.Users
{

    [AbpAuthorize]
    public class UserAppService : AsyncCrudAppService<User, UserDto, long, PagedUserResultRequestDto, CreateUserDto, UserDto>
    {
        private readonly UserManager _userManager;
        private readonly RoleManager _roleManager;
        private readonly IRepository<Role> _roleRepository;
        private readonly IPasswordHasher<User> _passwordHasher;
        private readonly IAbpSession _abpSession;
        private readonly LogInManager _logInManager;
        //private readonly IRepository<Entities.Member, long> _memberRepository;
        private readonly IWorkScope _ws;
        private readonly IBackgroundJobManager _backgroundJobManager;
        private readonly IHostingEnvironment _hostingEnvironment;
        private readonly IUserServices _userServices;
        private readonly UploadAvatarService _filesService;
        private readonly ProjectService _projectService;
        private readonly HRMService _hrmService;
        private readonly HRMv2Service _hrmV2Service;
        private readonly PublicAppService _publicAppService;
        private readonly string templateFolder = Path.Combine("wwwroot", "template");
        public UserAppService(

            IRepository<User, long> repository,
            UserManager userManager,
            RoleManager roleManager,
            IRepository<Role> roleRepository,
            IPasswordHasher<User> passwordHasher,
            IAbpSession abpSession,
            LogInManager logInManager,
            IEmailSender emailSender,
            IWorkScope ws,
            IBackgroundJobManager backgroundJobManager,
            IHostingEnvironment environment,
            IUserServices userServices,
            UploadAvatarService filesService,
            ProjectService projectService,
            HRMService hrmService,
            HRMv2Service hRMv2Service,
            PublicAppService publicAppService
            )
        //IRepository<Entities.Member, long> memberRepository)
            : base(repository)
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _roleRepository = roleRepository;
            _passwordHasher = passwordHasher;
            _abpSession = abpSession;
            _logInManager = logInManager;
            //_memberRepository = memberRepository;
            _ws = ws;
            _backgroundJobManager = backgroundJobManager;
            _hostingEnvironment = environment;
            _userServices = userServices;
            _filesService = filesService;
            _projectService = projectService;
            _hrmService = hrmService;
            _hrmV2Service = hRMv2Service;
            _publicAppService = publicAppService;
        }

        [AbpAuthorize(Ncc.Authorization.PermissionNames.Admin_Users_AddNew)]
        public override async Task<UserDto> Create(CreateUserDto input)
        {
            var user = await _userServices.CreateUserAsync(input);

            //var user = ObjectMapper.Map<User>(input);
            //user.Id = createUser.Id;
            return MapToEntityDto(user);
            //  return MapToEntityDto(user);
        }

        [AbpAuthorize(Ncc.Authorization.PermissionNames.Admin_Roles_Edit)]
        public async System.Threading.Tasks.Task ChangeUserRole(AddUserToRoleDto input)
        {
            var user = await _userManager.GetUserByIdAsync(input.UserId);
            var listRole = await _userManager.GetRolesAsync(user);
            bool flag = false;

            foreach (var role in listRole)
            {
                if (role.Equals(input.Role))
                {
                    listRole.Remove(role);
                    flag = true;
                    break;
                }
            }

            if (!flag) listRole.Add(input.Role);

            string[] roles = listRole.ToArray();

            if (input.Role != null)
            {
                await _userManager.SetRoles(user, roles);
            }
        }

        [AbpAuthorize(Ncc.Authorization.PermissionNames.Admin_Users_Edit)]
        public override async Task<UserDto> Update(UserDto input)
        {
            var isUpdateUserWorkingTime = await this.IsGrantedAsync(Ncc.Authorization.PermissionNames.Admin_Users_UpdateUserWorkingTime);
            await _userServices.UpdateUserAsync(input, isUpdateUserWorkingTime);
            return await Get(input);
        }

        [AbpAuthorize(Ncc.Authorization.PermissionNames.Admin_Users_EditRole)]
        public async Task<UpdateRoleDto> UpdateRole(UpdateRoleDto input)
        {
            var user = _userManager.GetUserByIdAsync(input.Id).Result;

            if (input.RoleNames.Length == 0)
            {
                input.RoleNames = new string[] { StaticRoleNames.Host.BasicUser };
            }

            await _userManager.SetRoles(user, input.RoleNames);

            return input;
        }

        [AbpAuthorize(Ncc.Authorization.PermissionNames.Admin_Users_Delete)]
        public override async System.Threading.Tasks.Task Delete(EntityDto<long> input)
        {
            var hasProject = await _ws.GetRepo<ProjectUser>().GetAllIncluding(s => s.Project).AnyAsync(s => s.UserId == input.Id);
            if (hasProject)
            {
                throw new UserFriendlyException(String.Format("This User Id {0} is in a project ,You can't delete", input.Id));
            }
            var user = await _userManager.GetUserByIdAsync(input.Id);
            await _userManager.DeleteAsync(user);
        }

        [AbpAuthorize(Ncc.Authorization.PermissionNames.Admin_Users_View)]
        public async Task<ListResultDto<RoleDto>> GetRoles()
        {
            var roles = await _roleRepository.GetAllListAsync();
            return new ListResultDto<RoleDto>(ObjectMapper.Map<List<RoleDto>>(roles));
        }

        public async System.Threading.Tasks.Task ChangeLanguage(ChangeUserLanguageDto input)
        {
            await SettingManager.ChangeSettingForUserAsync(
                AbpSession.ToUserIdentifier(),
                LocalizationSettingNames.DefaultLanguage,
                input.LanguageName
            );
        }


        protected override User MapToEntity(CreateUserDto createInput)
        {
            var user = ObjectMapper.Map<User>(createInput);
            user.SetNormalizedNames();
            return user;
        }


        protected override void MapToEntity(UserDto input, User user)
        {
            ObjectMapper.Map(input, user);
            user.SetNormalizedNames();
        }

        protected override UserDto MapToEntityDto(User user)
        {
            var roles = _roleManager.Roles.Where(r => user.Roles.Any(ur => ur.RoleId == r.Id)).Select(r => r.NormalizedName);
            var userDto = base.MapToEntityDto(user);
            userDto.RoleNames = roles.ToArray();
            return userDto;
        }

        protected override IQueryable<User> CreateFilteredQuery(PagedUserResultRequestDto input)
        {
            return Repository.GetAllIncluding(x => x.Roles)
                .WhereIf(!input.Keyword.IsNullOrWhiteSpace(), x => x.UserName.Contains(input.Keyword) || x.FullName.Contains(input.Keyword) || x.EmailAddress.Contains(input.Keyword))
                .WhereIf(input.IsActive.HasValue, x => x.IsActive == input.IsActive);
        }

        protected override async Task<User> GetEntityByIdAsync(long id)
        {
            var user = await Repository.GetAllIncluding(x => x.Roles).FirstOrDefaultAsync(x => x.Id == id);

            if (user == null)
            {
                throw new EntityNotFoundException(typeof(User), id);
            }

            return user;
        }

        protected override IQueryable<User> ApplySorting(IQueryable<User> query, PagedUserResultRequestDto input)
        {
            return query.OrderBy(r => r.UserName);
        }

        protected virtual void CheckErrors(IdentityResult identityResult)
        {
            identityResult.CheckErrors(LocalizationManager);
        }

        public async Task<bool> ChangePassword(ChangePasswordDto input)
        {
            if (_abpSession.UserId == null)
            {
                throw new UserFriendlyException("Please log in before attemping to change password.");
            }
            long userId = _abpSession.UserId.Value;
            var user = await _userManager.GetUserByIdAsync(userId);
            var loginAsync = await _logInManager.LoginAsync(user.UserName, input.CurrentPassword, shouldLockout: false);
            if (loginAsync.Result != AbpLoginResultType.Success)
            {
                throw new UserFriendlyException("Your 'Existing Password' did not match the one on record.  Please try again or contact an administrator for assistance in resetting your password.");
            }
            if (!new Regex(AccountAppService.PasswordRegex).IsMatch(input.NewPassword))
            {
                throw new UserFriendlyException("Passwords must be at least 8 characters, contain a lowercase, uppercase, and number.");
            }
            user.Password = _passwordHasher.HashPassword(user, input.NewPassword);
            CurrentUnitOfWork.SaveChanges();

            return true;
        }

        //[AbpAllowAnonymous]
        //public async Task<Object> ResetAllPassword()
        //{
        //    var users = await this.Repository.GetAll().Where(s => s.IsActive).ToListAsync();
        //    foreach (var user in users)
        //    {
        //        user.Password = _passwordHasher.HashPassword(user, "PasswordUserabc");
        //    }

        //    await CurrentUnitOfWork.SaveChangesAsync();

        //    return new { Users = users.Select(s => new { s.UserName, Password = "PasswordUserabc" }).ToList() };
        //}


        [AbpAuthorize(Ncc.Authorization.PermissionNames.Admin_Users_ResetPassword)]
        public async Task<bool> ResetPassword(ResetPasswordDto input)
        {
            if (_abpSession.UserId == null)
            {
                throw new UserFriendlyException("Please log in before attemping to reset password.");
            }
            long currentUserId = _abpSession.UserId.Value;
            var currentUser = await _userManager.GetUserByIdAsync(currentUserId);
            if (currentUser.IsDeleted || !currentUser.IsActive)
            {
                return false;
            }
            var roles = await _userManager.GetRolesAsync(currentUser);
            if (!roles.Contains(StaticRoleNames.Tenants.Admin))
            {
                throw new UserFriendlyException("Only administrators may reset passwords.");
            }

            var user = await _userManager.GetUserByIdAsync(input.UserId);
            if (user != null)
            {
                user.Password = _passwordHasher.HashPassword(user, input.NewPassword);
                CurrentUnitOfWork.SaveChanges();

                //await _backgroundJobManager.EnqueueAsync<EmailBackgroundJob, EmailBackgroundJobArgs>(new EmailBackgroundJobArgs
                //{
                //    TargetEmails = new List<string>() { user.EmailAddress },
                //    Body = $@"<div>Username: <b>{user.UserName}</b></div>
                //                     <div>Password: <b>{input.NewPassword}</b></div>",
                //    Subject = "Your password was reset"
                //}, BackgroundJobPriority.High, new TimeSpan(TimeSpan.TicksPerMinute)
                //);
            }
            return true;
        }


        public async Task<List<GetUserDto>> GetUserNotPagging()
        {
            return await this.Repository.GetAll()
                .Select(s => new GetUserDto
                {
                    Id = s.Id,
                    Name = s.FullName,
                    EmailAddress = s.EmailAddress,
                    IsActive = s.IsActive,
                    Type = s.Type,
                    JobTitle = s.JobTitle,
                    //Level = s.Level,
                    UserCode = s.UserCode,
                    Branch = s.BranchOld,
                    BranchColor = s.Branch.Color,
                    BranchId = s.Branch.Id,
                    BranchDisplayName = s.Branch.DisplayName,
                    AvatarPath = s.AvatarPath != null ? s.AvatarPath : "",
                    PositionId = s.Position.Id,
                    PositionName = s.Position.Name
                }).ToListAsync();
        }

        [HttpPost]
        [AbpAuthorize(Ncc.Authorization.PermissionNames.Admin_Users_View)]
        public async Task<PagedResultDto<GetAllUserDto>> GetAllPagging(GridParam input)
        {
            var qUserRoles = from ur in _ws.GetAll<UserRole>()
                             join r in _ws.GetAll<Role, int>() on ur.RoleId equals r.Id
                             select new
                             {
                                 ur.UserId,
                                 RoleName = r.Name
                             };
            var qprojectUsers = from pu in _ws.GetAll<ProjectUser>().Where(s => s.User.IsActive == true)
                                join p in _ws.GetAll<Project>().Where(s => s.Status == ProjectStatus.Active) on pu.ProjectId equals p.Id
                                where pu.Type != ProjectUserType.DeActive
                                select new
                                {
                                    pu.ProjectId,
                                    p.Code,
                                    p.Name,
                                    pu.UserId,
                                    pu.Type
                                };

            var query = from u in _ws.GetAll<User>()
                        join pu in qprojectUsers on u.Id equals pu.UserId into pusers
                        join ur in qUserRoles on u.Id equals ur.UserId into roles
                        join mu in _ws.GetAll<User>() on u.ManagerId equals mu.Id into muu
                        select new GetAllUserDto
                        {
                            Id = u.Id,
                            UserName = u.UserName,
                            Name = u.Name,
                            Surname = u.Surname,
                            FullName = u.FullName,
                            Address = u.Address,
                            IsActive = u.IsActive,
                            EmailAddress = u.EmailAddress,
                            PhoneNumber = u.PhoneNumber,
                            RoleNames = roles.Select(s => s.RoleName).ToArray(),
                            ProjectUsers = pusers.Select(s => new PUDto
                            {
                                ProjectId = s.ProjectId,
                                ProjectName = s.Name,
                                ProjectCode = s.Code,
                                ProjectUserType = s.Type
                            }).ToList(),
                            Type = u.Type,
                            JobTitle = u.JobTitle,
                            Level = u.Level,
                            RegisterWorkDay = u.RegisterWorkDay,
                            StartDateAt = u.StartDateAt,
                            UserCode = u.UserCode,
                            Salary = u.Salary,
                            SalaryAt = u.SalaryAt,
                            AllowedLeaveDay = u.AllowedLeaveDay,
                            BranchDisplayName = u.Branch.DisplayName,
                            BranchId = u.BranchId,
                            AvatarPath = u.AvatarPath,
                            ManagerId = u.ManagerId,
                            Sex = u.Sex,
                            CreationTime = u.CreationTime,
                            ManagerName = muu.FirstOrDefault() != null ? muu.FirstOrDefault().FullName : "",
                            ManagerAvatarPath = muu.FirstOrDefault() != null ? muu.FirstOrDefault().AvatarPath : "",
                            MorningStartAt = u.MorningStartAt,
                            MorningEndAt = u.MorningEndAt,
                            MorningWorking = u.MorningWorking,
                            AfternoonStartAt = u.AfternoonStartAt,
                            AfternoonEndAt = u.AfternoonEndAt,
                            AfternoonWorking = u.AfternoonWorking,
                            PositionId = u.Position.Id,
                            PositionName = u.Position.Name
                        };

            query = query.OrderByDescending(s => s.CreationTime);
            var temp = await query.GetGridResult(query, input);

            var projectIds = new HashSet<long>();
            foreach (var user in temp.Items)
            {
                projectIds.UnionWith(user.ProjectUsers.Select(s => s.ProjectId));
            }

            var projects = (_ws.GetAll<ProjectUser>()
                    .Where(s => projectIds.Contains(s.ProjectId) && s.Type == ProjectUserType.PM)
                    .Select(s => new { s.ProjectId, s.User.FullName })
                    .GroupBy(s => s.ProjectId))
                    .Select(s => new { s.Key, pms = s.Select(f => f.FullName).ToList() }).ToList();

            foreach (var user in temp.Items)
            {
                foreach (var pu in user.ProjectUsers)
                {
                    pu.Pms = projects.Where(s => s.Key == pu.ProjectId).Select(s => s.pms).FirstOrDefault();
                }
            }
            return new PagedResultDto<GetAllUserDto>(temp.TotalCount, temp.Items);
        }

        [HttpPost]
        [AbpAuthorize(Ncc.Authorization.PermissionNames.Admin_Users_AddNew)]
        public async Task<Object> ImportUsersFromFile([FromForm] FileInputDto input)
        {
            var successList = new List<string>();
            var failedList = new List<string>();
            if (input != null)
            {
                if (Path.GetExtension(input.File.FileName).Equals(".xlsx"))
                {
                    using (var stream = new MemoryStream())
                    {
                        await input.File.CopyToAsync(stream);

                        using (var package = new ExcelPackage(stream))
                        {
                            ExcelWorksheet worksheet = package.Workbook.Worksheets[0];
                            var rowCount = worksheet.Dimension.Rows;
                            User user = null;

                            for (int row = 2; row <= rowCount; row++)
                            {
                                try
                                {
                                    user = new User
                                    {
                                        Name = worksheet.Cells[row, 1].Value.ToString().Trim(),
                                        Surname = worksheet.Cells[row, 2].Value.ToString().Trim(),
                                        EmailAddress = worksheet.Cells[row, 3].Value.ToString().Trim(),
                                    };

                                    if (worksheet.Cells[row, 4] != null && worksheet.Cells[row, 4].Value != null)
                                        user.PhoneNumber = worksheet.Cells[row, 4].Value.ToString().Trim();

                                    if (worksheet.Cells[row, 5] != null && worksheet.Cells[row, 5].Value != null)
                                        user.Address = worksheet.Cells[row, 5].Value.ToString().Trim();


                                    user.UserName = user.EmailAddress.Split("@")[0];
                                    user.IsActive = true;
                                    await _userManager.CreateAsync(user, RandomPasswordHelper.CreateRandomPassword(8));
                                    CheckErrors(await _userManager.SetRoles(user, new string[] { StaticRoleNames.Host.BasicUser }));
                                    successList.Add(user.EmailAddress);
                                }
                                catch (Exception e)
                                {
                                    failedList.Add(user.EmailAddress + " error =>" + e.Message);
                                }
                            }
                        }
                    }
                }
                else
                {
                    throw new UserFriendlyException(String.Format("No file upload!"));
                }
            }
            return new { successList, failedList };
        }

        public async Task<List<ProjectManagerDto>> GetProjectManagerOfUser(EntityDto<long> input)
        {
            var projectIds = _ws.GetAll<ProjectUser>().Where(s => s.UserId == input.Id).Select(s => s.ProjectId).Distinct();
            var projectManagers = await _ws.GetRepo<ProjectUser>().GetAllIncluding(s => s.User, s => s.Project)
                .Where(s => s.Type == ProjectUserType.PM && s.Project.Status == ProjectStatus.Active && s.UserId != input.Id)
                .Join(projectIds, s => s.ProjectId, s => s, (pu, id) => new ProjectManagerDto
                {
                    Id = pu.UserId,
                    Name = pu.User.Name,
                    Surname = pu.User.Surname
                }).Distinct().ToListAsync();

            return projectManagers;
        }

        [HttpPost]
        [AbpAuthorize(Ncc.Authorization.PermissionNames.Admin_Users_UploadAvatar)]
        public async Task<string> UpdateAvatar([FromForm] AvatarDto input)
        {
            User user = await _userManager.GetUserByIdAsync(input.UserId);
            //set avatar name = milisecond + id + name + extension
            String avatarPath = await _filesService.UploadAvatarAsync(input.File);
            user.AvatarPath = avatarPath;
            var avartarUserInfo = new UpdateAvatarDto
            {
                AvatarPath = avatarPath,
                EmailAddress = user.EmailAddress
            };
            await _userManager.UpdateAsync(user);
            if (ConstantUploadFile.Provider != ConstantUploadFile.INTERNAL)
            {
                _projectService.UpdateAvatarToProject(avartarUserInfo);
                _hrmService.UpdateAvatarToHRM(avartarUserInfo);
                _hrmV2Service.UpdateAvatarToHrm(avartarUserInfo);
            }
            return avatarPath;
        }


        [HttpPost]
        public async Task<string> UpdateYourOwnAvatar([FromForm] FileInputDto input)
        {
            User user = await _userManager.GetUserByIdAsync(AbpSession.UserId.Value);
            //set avatar name = milisecond + id + name + extension
            String avatarPath = await _filesService.UploadAvatarAsync(input.File);
            user.AvatarPath = avatarPath;
            var avartarUserInfo = new UpdateAvatarDto
            {
                AvatarPath = avatarPath,
                EmailAddress = user.EmailAddress
            };
            await _userManager.UpdateAsync(user);
            if (ConstantUploadFile.Provider != ConstantUploadFile.INTERNAL)
            {
                _projectService.UpdateAvatarToProject(avartarUserInfo);
                _hrmService.UpdateAvatarToHRM(avartarUserInfo);
                _hrmV2Service.UpdateAvatarToHrm(avartarUserInfo);
            }
            return FileUtils.FullFilePath(avatarPath);

        }


        [AbpAuthorize(Ncc.Authorization.PermissionNames.Project, Ncc.Authorization.PermissionNames.Admin_Users)]
        public async Task<List<GetUserDto>> GetAllManager()
        {
            var allManagerId = this.Repository.GetAll().Select(s => s.ManagerId).ToHashSet();
            return await this.Repository.GetAll().Where(s => allManagerId.Contains(s.Id)).Select(s => new GetUserDto
            {
                Id = s.Id,
                Name = s.FullName,
                IsActive = s.IsActive,
                Type = s.Type,
                JobTitle = s.JobTitle,
                Level = s.Level,
                UserCode = s.UserCode,
                AvatarPath = s.AvatarPath != null ? s.AvatarPath : ""
            }).ToListAsync();
        }

        [AbpAuthorize(Ncc.Authorization.PermissionNames.Admin_Users_ChangeStatus)]
        public async System.Threading.Tasks.Task DeactiveUser(EntityDto<long> input)
        {
            var user = await _ws.GetAsync<User>(input.Id);
            if (user != null)
            {
                user.EndDateAt = user.EndDateAt.HasValue ? user.EndDateAt : DateTimeUtils.GetNow();
                user.IsActive = false;
                await _ws.GetRepo<User, long>().UpdateAsync(user);
            }
            else
            {
                throw new UserFriendlyException(string.Format("User is not exist"));
            }

        }


        [AbpAuthorize(Ncc.Authorization.PermissionNames.Admin_Users_ChangeStatus)]
        public async System.Threading.Tasks.Task ActiveUser(EntityDto<long> input)
        {
            var user = await _ws.GetAsync<User>(input.Id);
            if (user != null)
            {
                user.IsActive = true;
                await _ws.GetRepo<User, long>().UpdateAsync(user);
            }
            else
            {
                throw new UserFriendlyException(string.Format("User is not exist"));
            }

        }

        [AbpAuthorize(Ncc.Authorization.PermissionNames.Admin_Users_ImportWorkingTime)]
        public async Task<Object> ImportWorkingTimeFromFile([FromForm] FileInputDto input)
        {
            var successList = new List<UpdateWorkingTimeDto>();
            var failedList = new List<string>();
            if (input == null)
            {
                throw new UserFriendlyException(String.Format("No file upload!"));
            }
            var path = new String[] { ".xlsx", ".xltx" };
            if (!path.Contains(Path.GetExtension(input.File.FileName)))
            {
                throw new UserFriendlyException(String.Format("Invalid file upload."));
            }

            List<UpdateWorkingTimeDto> updateWorkingTime = new List<UpdateWorkingTimeDto>();
            using (var stream = new MemoryStream())
            {
                await input.File.CopyToAsync(stream);

                using (var package = new ExcelPackage(stream))
                {
                    ExcelWorksheet worksheet = package.Workbook.Worksheets[0];
                    var rowCount = worksheet.Dimension.Rows;

                    for (int row = 2; row <= rowCount; row++)
                    {
                        updateWorkingTime.Add(new UpdateWorkingTimeDto
                        {
                            UserId = Convert.ToInt64((worksheet.Cells[row, 1].Value.ToString().Trim())),
                            MorningStartAt = worksheet.Cells[row, 3].Value.ToString().Trim(),
                            MorningEndAt = worksheet.Cells[row, 4].Value.ToString().Trim(),
                            AfternoonStartAt = worksheet.Cells[row, 5].Value.ToString().Trim(),
                            AfternoonEndAt = worksheet.Cells[row, 6].Value.ToString().Trim()
                        });
                    }
                }
            }

            var users = _ws.GetAll<User>().Where(x => updateWorkingTime.Select(y => y.UserId).Contains(x.Id));
            foreach (var u in users)
            {
                try
                {
                    var updateU = updateWorkingTime.Where(x => x.UserId == u.Id).First();
                    u.MorningStartAt = updateU.MorningStartAt;
                    u.MorningEndAt = updateU.MorningEndAt;
                    u.MorningWorking = CommonUtils.SubtractHHmm(u.MorningEndAt, u.MorningStartAt) / 60;
                    u.AfternoonStartAt = updateU.AfternoonStartAt;
                    u.AfternoonEndAt = updateU.AfternoonEndAt;
                    u.AfternoonWorking = CommonUtils.SubtractHHmm(u.AfternoonEndAt, u.AfternoonStartAt) / 60;
                    u.isWorkingTimeDefault = false;
                    await _ws.UpdateAsync(u);
                    successList.Add(updateU);
                }
                catch (Exception e)
                {
                    failedList.Add("Error: Row " + u.Id + ". " + e.Message);
                }
            }

            if (successList.Count() < 1)
            {
                throw new UserFriendlyException(String.Format("Invalid excel data."));
            }
            if (failedList.Count() < 1)
            {
                failedList.Add("List is empty.");
            }

            return new { successList, failedList };
        }


        public async Task<List<GetAllPMDto>> GetAllPM()
        {
            var Result = from pu in _ws.GetAll<ProjectUser>().Where(x => x.Type == ProjectUserType.PM)
                         join u in _ws.GetAll<User>().Where(x => x.IsActive == true)
                         on pu.UserId equals u.Id
                         group pu by new { pu.UserId, u.UserName, u.EmailAddress, u.AvatarPath } into g
                         select new GetAllPMDto
                         {
                             PMId = g.Key.UserId,
                             PMFullName = g.Key.UserName,
                             PMEmailAddress = g.Key.EmailAddress,
                             PMAvatarPath = g.Key.AvatarPath != null ? g.Key.AvatarPath : ""
                         };

            return await Result.Distinct().ToListAsync();
        }

        [HttpGet]
        public async Task<List<GetInternshipDto>> GetAllInternship()
        {

            return await (_ws.GetAll<User>()
                .Where(x => x.IsActive == true && x.Type == Usertype.Internship)
                .Select(x => new GetInternshipDto
                {
                    InternshipId = x.Id,
                    InternshipName = x.FullName
                })).ToListAsync();
        }

        [HttpGet]
        public string GetUserEmailById(long id)
        {
            var userEmail = _ws.GetAll<User>()
                .Where(x => x.Id == id)
                .Select(x => x.EmailAddress)
                .FirstOrDefault();
            return userEmail;
        }

        [HttpGet]
        public GetAvatarPathDto GetUserAvatarById(long id)
        {
            var userAvatarPath = _ws.GetAll<User>()
                .Where(x => x.Id == id)
                .Select(x => new GetAvatarPathDto
                {
                    AvatarPath = x.AvatarPath,
                })
                .FirstOrDefault();
            return userAvatarPath;
        }

        [HttpGet]
        [AbpAuthorize(Ncc.Authorization.PermissionNames.TimesheetSupervision_View)]
        public async Task<List<UserFilterDto>> GetAllActiveUser()
        {
            var users = await _ws.GetAll<User>()
                .Where(s => s.IsActive)
                .Where(s => !s.IsStopWork)
               .Select(s => new UserFilterDto
               {
                   Id = s.Id,
                   EmailAddress = s.EmailAddress,
                   FullName = s.FullName
               })
               .ToListAsync();

            return users;
        }

        // export file Data Checkpoint
        private void FillDataCheckpointResult(ExcelPackage excelPackageIn, List<DataCheckPointDto> listDataCheckpointInput)
        {
            var sheet = excelPackageIn.Workbook.Worksheets[0];
            var rowIndex = 4;
            foreach (var item in listDataCheckpointInput)
            {
                sheet.Cells[1, 1].Value = item.CheckPointName;
                sheet.Cells[rowIndex, 1].Value = rowIndex - 3;            
                sheet.Cells[rowIndex, 2].Value = item.EmailAddress;
                sheet.Cells[rowIndex, 3].Value = item.ProjectNames;
                sheet.Cells[rowIndex, 4].Value = item.ReviewerEmail;
                rowIndex++;
            }
        }

        [HttpGet]
        [AbpAuthorize(Ncc.Authorization.PermissionNames.Admin_Users_ExportDataCheckpoint)]
        public async Task<FileBase64Dto> ExportDataCheckpoint(DateTime startDate, DateTime endDate)
        {
            var listDataCheckpointInput = _publicAppService.GetDataForCheckPoint(startDate, endDate);
            var templateFilePath = Path.Combine(templateFolder, "ExportDataCheckPoint.xlsx");
            using (var memoryStream = new MemoryStream(File.ReadAllBytes(templateFilePath)))
            {
                using (var excelPackageIn = new ExcelPackage(memoryStream))
                {
                    FillDataCheckpointResult(excelPackageIn, await listDataCheckpointInput);
                    string fileBase64 = Convert.ToBase64String(excelPackageIn.GetAsByteArray());

                    return new FileBase64Dto
                    {
                        FileName = "Checkpoint_data_" + startDate.ToString("yyyy-MM-dd") + " - " + endDate.ToString("yyyy-MM-dd"),
                        FileType = MimeTypeNames.ApplicationVndOpenxmlformatsOfficedocumentSpreadsheetmlSheet,
                        Base64 = fileBase64
                    };
                }
            }
        }
    }
}


