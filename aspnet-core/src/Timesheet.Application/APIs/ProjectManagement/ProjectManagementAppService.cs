using Abp.Authorization;
using Abp.Configuration;
using Abp.UI;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Ncc;
using Ncc.Configuration;
using Ncc.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Timesheet.APIs.ProjectManagement.Dto;
using Timesheet.Entities;
using static Ncc.Entities.Enum.StatusEnum;
using Ncc.Authorization.Roles;
using Ncc.Authorization.Users;
using Timesheet.DomainServices;
using Task = Ncc.Entities.Task;
using Timesheet.Timesheets.Customers.Dto;
using Abp.Authorization.Users;
using Timesheet.Extension;
using Ncc.IoC;
using Timesheet.NCCAuthen;

namespace Timesheet.APIs.ProjectManagement
{
    public class ProjectManagementAppService : AppServiceBase
    {
        private readonly IUserServices _userServices;
        private readonly UserManager _userManager;
        private readonly RoleManager _roleManager;

        public ProjectManagementAppService(IUserServices userService, UserManager userManager, RoleManager roleManager, IWorkScope workScope) : base(workScope)
        {
            _userServices = userService;
            _userManager = userManager;
            _roleManager = roleManager;
        }

        [HttpGet]
        [AbpAllowAnonymous]
        public async Task<TotalWorkingTimeDto> GetTotalWorkingTime(string projectCode, DateTime startDate, DateTime endDate)
        {
            var q = WorkScope.GetAll<MyTimesheet>()
                .Where(s => s.Status == TimesheetStatus.Approve)
                .Where(s => s.DateAt >= startDate.Date && s.DateAt.Date <= endDate)
                .Where(s => s.ProjectTask.Project.Code == projectCode);

            var normalWorking = await q.Where(s => s.TypeOfWork == TypeOfWork.NormalWorkingHours).SumAsync(s => s.WorkingTime);
            var ot = await q.Where(s => s.TypeOfWork == TypeOfWork.OverTime).SumAsync(s => s.WorkingTime);
            var otNoCharged = await q.Where(s => s.TypeOfWork == TypeOfWork.OverTime && !s.IsCharged).SumAsync(s => s.WorkingTime);

            return new TotalWorkingTimeDto { NormalWorkingMinute = normalWorking, OTMinute = ot, OTNoChargeMinute = otNoCharged };

        }

        [HttpPost]
        [AbpAllowAnonymous]
        public async Task<List<TotalWorkingTimeDto>> GetTimesheetByListProjectCode(List<String> listProjectCode, DateTime startDate, DateTime endDate)
        {

            return await WorkScope.GetAll<MyTimesheet>()
                          .Include(s => s.ProjectTask)
                          .Include(s => s.ProjectTask.Project)
                          .Where(s => s.DateAt >= startDate.Date)
                          .Where(s => s.DateAt.Date <= endDate)
                          .Where(s => s.Status == TimesheetStatus.Approve || s.Status == TimesheetStatus.Pending)
                          .Where(s => listProjectCode.Contains(s.ProjectTask.Project.Code))
                          .GroupBy(s => s.ProjectTask.Project.Code)
                          .Select(s => new TotalWorkingTimeDto
                          {
                              ProjectCode = s.Key,
                              NormalWorkingMinute = s.Where(x => x.TypeOfWork == TypeOfWork.NormalWorkingHours).Sum(x => x.WorkingTime),
                              OTMinute = s.Where(x => x.TypeOfWork == TypeOfWork.OverTime).Sum(p => p.WorkingTime),
                              OTNoChargeMinute = s.Where(x => x.TypeOfWork == TypeOfWork.OverTime && !x.IsCharged).Sum(p => p.WorkingTime),
                          }).ToListAsync();

        }

        //TODO: test CreateProject funtion
        [HttpPost]
        [NccAuthentication]
        public async Task<string> CreateProject(SpecialProjectDto input)
        {
            var isExistName = await WorkScope.GetAll<Project>().AnyAsync(s => s.Name == input.Name);
            if (isExistName)
            {
                return string.Format("Fail! Project name <b>{0}</b> already exist in <b>TIMESHEET TOOL</b>", input.Name);
            }

            var isExistCode = await WorkScope.GetAll<Project>().AnyAsync(s => s.Code == input.Code);
            if (isExistCode)
            {
                return string.Format("Fail! Project code <b>{0}</b> already exist in <b>TIMESHEET TOOL</b>", input.Code);
            }

            if (input.TimeEnd.HasValue && input.TimeStart.Date > input.TimeEnd.Value.Date)
            {
                return "Fail! Start time cannot be greater than end time in <b>TIMESHEET TOOL</b>";
            }

            //Customer
            var customerId = await WorkScope.GetAll<Customer>()
                    .Where(s => s.Code == input.CustomerCode)
                    .Select(s => s.Id).FirstOrDefaultAsync();

            if (customerId == default)
            {
                return string.Format("Fail! Not found Customer code <b>{0}</b> in <b>TIMESHEET TOOL</b>", input.CustomerCode);
            }

            var types = new ProjectType[] { ProjectType.ODC, ProjectType.TimeAndMaterials, ProjectType.FixedFee, ProjectType.Product, ProjectType.NoneBillable, ProjectType.Training, ProjectType.NoSalary };
            var projectType = types[input.ProjectType];

            //User
            var userByEmail = await WorkScope.GetAll<User>()
                   .Where(s => s.EmailAddress.ToLower() == input.EmailPM.ToLower())
                   .Select(s => new
                   {
                       Id = s.Id,
                       Type = s.Type
                   }).FirstOrDefaultAsync();

            if (userByEmail == default)
            {
                return string.Format("Fail! Not found PM with email <b>{0}</b> in <b>TIMESHEET TOOL</b>", input.EmailPM);
            }

            //insert project
            var project = new Project
            {
                Name = input.Name,
                Code = input.Code,
                CustomerId = customerId,
                ProjectType = projectType,
                TimeStart = input.TimeStart,
                TimeEnd = input.TimeEnd,
                Status = ProjectStatus.Active
            };
            input.Id = await WorkScope.InsertAndGetIdAsync<Project>(project);

            //insert ProjectTask
            var commonTask = await WorkScope.GetAll<Task>()
                  .Where(s => s.Type == TaskType.CommonTask)
                  .Select(s => s.Id).ToListAsync();

            var billable = false;

            var projectTypeTask = new ProjectType[] { ProjectType.ODC, ProjectType.TimeAndMaterials, ProjectType.FixedFee };

            if (projectTypeTask.Contains(projectType))
            {
                billable = true;
            }

            foreach (var task in commonTask)
            {
                var projectTask = new ProjectTask
                {
                    ProjectId = input.Id,
                    TaskId = task,
                    Billable = billable
                };
                await WorkScope.GetRepo<ProjectTask, long>().InsertAsync(projectTask);
            }

            //insert projectUser
            var projectUser = new ProjectUser
            {
                ProjectId = input.Id,
                UserId = userByEmail.Id,
                Type = ProjectUserType.PM,
                IsTemp = false
            };
            await WorkScope.GetRepo<ProjectUser, long>().InsertAsync(projectUser);

            var userRoles = await _userServices.UserAllRoles(userByEmail.Id).ToListAsync();
            if (!userRoles.Any(r => r == StaticRoleNames.Host.Admin || r == StaticRoleNames.Host.ProjectAdmin))
            {
                userRoles.Add(StaticRoleNames.Host.ProjectAdmin);
                var user = await WorkScope.GetAsync<User>(projectUser.UserId);
                CheckErrors(await _userManager.SetRoles(user, userRoles.ToArray()));
            }

            return null;
        }
        //TODO: test ChangePmOfProject funtion
        [HttpPost]
        [NccAuthentication]
        public async Task<string> ChangePmOfProject(SpecialProjectDto input)
        {
            //project
            var projectId = await GetProjectIdByCode(input.Code);

            if (projectId == default)
            {
                return string.Format("Fail! Not found project code <b>{0}</b> in <b>TIMESHEET TOOL</b>", input.Code);
            }


            if (string.IsNullOrEmpty(input.EmailPM))
            {
                return "Fail! Email are not allowed to be empty <b>{0}</b> in <b>TIMESHEET TOOL</b>";
            }
            //insert projectUser
            var userId = await GetUserIdByEmail(input.EmailPM);

            if (userId == default)
            {
                return string.Format("Fail! Not found PM with email <b>{0}</b> in <b>TIMESHEET TOOL</b>", input.EmailPM);
            }

            var userInProject = await WorkScope.GetAll<ProjectUser>()
                .Where(s => s.UserId == userId)
                .Where(s => s.ProjectId == projectId)
                .FirstOrDefaultAsync();

            if (userInProject == default)
            {
                var projectUser = new ProjectUser
                {
                    ProjectId = projectId,
                    UserId = userId,
                    Type = ProjectUserType.PM
                };
                await WorkScope.GetRepo<ProjectUser, long>().InsertAsync(projectUser);
            }
            else
            {
                userInProject.Type = ProjectUserType.PM;
                await WorkScope.UpdateAsync(userInProject);
            }

            var userHasRole = _userServices.UserHasRole(userId, StaticRoleNames.Host.ProjectAdmin);
            if (!userHasRole)
            {
                var roleId = _roleManager.GetRoleByNameAsync(StaticRoleNames.Host.ProjectAdmin).Result.Id;
                WorkScope.Insert<UserRole>(new UserRole
                {
                    RoleId = roleId,
                    UserId = userId
                });
            }

            return null;
        }

        private async Task<long> GetProjectIdByCode(string code)
        {
            return await WorkScope.GetAll<Project>()
                    .Where(s => s.Code == code).Select(s => s.Id)
                    .FirstOrDefaultAsync();
        }

        private async Task<long> GetUserIdByEmail(string email)
        {
            if(email.IsEmpty())
            {
                return default;
            }    
            return await WorkScope.GetAll<User>()
                    .Where(s => s.EmailAddress == email).Select(s => s.Id)
                    .FirstOrDefaultAsync();
        }
        private async Task<User> GetUserByEmail(string email)
        {
            return await WorkScope.GetAll<User>()
                    .Where(s => s.EmailAddress == email)
                    .FirstOrDefaultAsync();
        }

        [HttpPost]
        [NccAuthentication]
        public async Task<string> UserJoinProject(UserJoinProjectDto input)
        {
            //project
            var projectId = await GetProjectIdByCode(input.ProjectCode);

            if (projectId == default)
            {
                return string.Format("Fail! Not found project code <b>{0}</b> in <b>TIMESHEET TOOL</b>", input.ProjectCode);
            }

            //user
            var user = await GetUserByEmail(input.EmailAddress);
            var basicTranerId = await GetUserIdByEmail(input.PMEmail);

            if (user == default)
            {
                return string.Format("Fail! Not found user with email <b>{0}</b> in <b>TIMESHEET TOOL</b>", input.EmailAddress);
            }
            if(basicTranerId == default)
            {
                Logger.Error(string.Format("Update Basic Traner Fail! Not found user with email <b>{0}</b> in <b>TIMESHEET TOOL</b>", input.PMEmail));
            }
            else
            {
                user.ManagerId = basicTranerId;
            }
            var pu = await WorkScope.GetAll<ProjectUser>()
                .Where(s => s.UserId == user.Id)
                .Where(s => s.ProjectId == projectId)
                .FirstOrDefaultAsync();
            int ProjectUserTypePM = 0;
            if (pu == default)
            {
                var projectUser = new ProjectUser
                {
                    ProjectId = projectId,
                    UserId = user.Id,
                    Type = input.Role == ProjectUserTypePM ? ProjectUserType.PM : ProjectUserType.Member,
                    IsTemp = input.IsPool
                };
                await WorkScope.GetRepo<ProjectUser, long>().InsertAsync(projectUser);
            }
            else
            {
                pu.IsTemp = input.IsPool;
                if(pu.Type == ProjectUserType.DeActive)
                {
                    pu.Type = ProjectUserType.Member;
                }
                await WorkScope.UpdateAsync(pu);
                UpdateTimeSheetToTempOrOfficial(user.Id, projectId, input.StartDate, pu.IsTemp);
            }
            CurrentUnitOfWork.SaveChanges();

            return null;
        }
        

        private void UpdateTimeSheetToTempOrOfficial(long userId, long projectId, DateTime startDate, bool isTemp)
        {
            var timesheets = WorkScope.GetAll<MyTimesheet>()
                .Where(s => s.UserId == userId)
                .Where(s => s.ProjectTask.ProjectId == projectId)
                .Where(s => s.DateAt >= startDate.Date)
                .ToList();

            timesheets.ForEach(s => s.IsTemp = isTemp);
            CurrentUnitOfWork.SaveChanges();
        }

        [HttpPost]
        [NccAuthentication]
        public async Task<string> CloseProject(string code)
        {
            //project
            var project = await WorkScope.GetAll<Project>()
                    .Where(s => s.Code == code)
                    .FirstOrDefaultAsync();

            if (project == default)
            {
                return string.Format("Fail! Not found project code <b>{0}</b> in <b>TIMESHEET TOOL</b>", code);
            }

            project.Status = ProjectStatus.Deactive;
            await WorkScope.UpdateAsync(project);

            return null;
        }
        //TODO: test CreateCustomer funtion
        [HttpPost]
        [NccAuthentication]
        public async Task<string> CreateCustomer(CustomerDto input)
        {
            var isExistName = await WorkScope.GetAll<Customer>().AnyAsync(s => s.Name == input.Name);
            if (isExistName)
                return string.Format("<p style='color:#dc3545'>Fail! Customer name <b>{0}</b> already exist in <b>TIMESHEET TOOL</b></p>", input.Name);

            var isExistCode = await WorkScope.GetAll<Customer>().AnyAsync(s => s.Code == input.Code);
            if (isExistCode)
                return string.Format("<p style='color:#dc3545'>Fail! Customer code <b>{0}</b> already exist in <b>TIMESHEET TOOL</b></p>", input.Code);

            var item = ObjectMapper.Map<Customer>(input);
            await WorkScope.InsertAsync(item);

            return string.Format("<p style='color:#28a745'>Create client name <b>{0}</b> in <b>TIMESHEET TOOL</b> successful!</p>", input.Name);
        }
        [HttpPost]
        [NccAuthentication]
        public async Task<List<RetroReviewInternHistoriesDto>> GetRetroReviewInternHistories(InputRetroReviewInternHistoriesDto input)
        {
            var userPointHistoryInRetro = WorkScope.GetAll<RetroResult>()
                .Select(s => new
                {
                    s.User.EmailAddress,
                    s.UserType,
                    s.Point,
                    s.Retro.StartDate,
                    s.Note,
                    s.Project.Name
                })
                .Where(x => x.UserType != Usertype.Internship)
                .Where(x => input.Emails.Contains(x.EmailAddress))
                .AsEnumerable()
                .GroupBy(s => s.EmailAddress)
                .Select(x => new RetroReviewInternHistoriesDto
                {
                    Email = x.Key,
                    PointHistories = x.OrderByDescending(s => s.StartDate)
                    .Select(s => new PointDto
                    {
                        StartDate = s.StartDate,
                        Point = s.Point,
                        isRetro = true,
                        Note = s.Note.Replace("<strong>","").Replace("</strong>", ""),
                        ProjectName = s.Name
                    })
                    .ToList()
                })
                .ToList();

            var userPointHistoryInReviewIntern = WorkScope.GetAll<ReviewDetail>()
                .Select(s => new
                {
                    s.InterShip.EmailAddress,
                    s.RateStar,
                    s.Review.Month,
                    s.Review.Year,
                    s.Note
                })
                .Where(x => input.Emails.Contains(x.EmailAddress))
                .AsEnumerable()
                .GroupBy(s => s.EmailAddress)
                .Select(x => new RetroReviewInternHistoriesDto
                {
                    Email = x.Key,
                    PointHistories = x.OrderByDescending(s => new DateTime(s.Year, s.Month, 1))
                    .Select(s => new PointDto
                    {
                        StartDate = new DateTime(s.Year, s.Month, 1),
                        Point = s.RateStar,
                        isRetro = false,
                        Note = s.Note.Replace("<strong>","").Replace("</strong>", ""),
                        ProjectName = "Basic Training"
                    })
                    .ToList()
                })
                .ToList();

            return userPointHistoryInRetro
                .Concat(userPointHistoryInReviewIntern)
                .GroupBy(s => s.Email)
                .Select(x => new RetroReviewInternHistoriesDto
                {
                    Email = x.Key,
                    PointHistories = x.SelectMany(q => q.PointHistories).Take(input.MaxCountHistory).ToList(),
                })
                .ToList();
        }
    }
}
