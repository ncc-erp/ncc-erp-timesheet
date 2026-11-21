using Abp.Authorization;
using Abp.BackgroundJobs;
using Abp.Configuration;
using Abp.Domain.Repositories;
using Abp.Domain.Uow;
using Abp.Runtime.Session;
using Abp.UI;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Ncc.Authorization;
using Ncc.Authorization.Users;
using Ncc.Entities;
using Ncc.IoC;
using NSubstitute;
using Shouldly;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Timesheet.APIs.ManageWorkingTimes;
using Timesheet.APIs.ManageWorkingTimes.Dto;
using Timesheet.BackgroundJob;
using Timesheet.DomainServices;
using Timesheet.Entities;
using Timesheet.Services.Komu;
using Timesheet.Services.Mezon;
using Timesheet.Timesheets.Projects;
using Xunit;
using static Ncc.Entities.Enum.StatusEnum;
using Branch = Timesheet.Entities.Branch;
using Task = System.Threading.Tasks.Task;

namespace Timesheet.Application.Tests.API.ManageWorkingTimes
{
    public class ManageWorkingTimeAppService_WorkMode_Tests : TimesheetApplicationTestBase
    {
        private readonly IWorkScope _workScope;

        public ManageWorkingTimeAppService_WorkMode_Tests()
        {
            _workScope = Resolve<IWorkScope>();
        }

        private ManageWorkingTimeAppService CreateManageService(long sessionUserId, bool hasViewByBranch)
        {
            var httpClient = Resolve<HttpClient>();
            var configuration = Substitute.For<IConfiguration>();
            var settingManager = Substitute.For<ISettingManager>();
            var loggerKomu = Resolve<ILogger<KomuService>>();
            configuration.GetValue<string>("KomuService:BaseAddress").Returns("http://www.myserver.com");
            configuration.GetValue<string>("KomuService:SecurityCode").Returns("secretCode");
            configuration.GetValue<string>("KomuService:DevModeChannelId").Returns("_channelIdDevMode");
            configuration.GetValue<string>("KomuService:EnableKomuNotification").Returns("false");

            var komuService = Substitute.For<KomuService>(httpClient, loggerKomu, configuration, settingManager);
            var mezonService = Resolve<MezonService>();
            var backgroundJobManager = Resolve<IBackgroundJobManager>();

            var session = Substitute.For<IAbpSession>();
            session.UserId.Returns(sessionUserId);
            session.TenantId.Returns((int?)null);

            var permissionChecker = Substitute.For<IPermissionChecker>();
            permissionChecker.IsGrantedAsync(PermissionNames.ManageWorkingTime_ViewByBranch).Returns(Task.FromResult(hasViewByBranch));

            return new ManageWorkingTimeAppService(backgroundJobManager, _workScope, komuService, mezonService)
            {
                AbpSession = session,
                UnitOfWorkManager = Resolve<IUnitOfWorkManager>(),
                SettingManager = settingManager,
                PermissionChecker = permissionChecker
            };
        }

        private ProjectAppService CreateProjectService(long sessionUserId, bool hasViewByBranch)
        {
            var session = Substitute.For<IAbpSession>();
            session.UserId.Returns(sessionUserId);
            session.TenantId.Returns((int?)null);

            var permissionChecker = Substitute.For<IPermissionChecker>();
            permissionChecker.IsGrantedAsync(PermissionNames.ManageWorkingTime_ViewByBranch).Returns(Task.FromResult(hasViewByBranch));

            var userServices = Substitute.For<IUserServices>();
            var userManager = Resolve<UserManager>();

            return new ProjectAppService(userServices, _projectService, userManager, _workScope)
            {
                AbpSession = session,
                UnitOfWorkManager = Resolve<IUnitOfWorkManager>(),
                PermissionChecker = permissionChecker
            };
        }

        [Fact]
        public async Task GetAll_Should_Filter_By_CurrentBranch_And_Return_WorkMode()
        {
            long branch1Id = 0, branch2Id = 0, directorId = 0, projectId = 0, userB1Id = 0;

            UsingDbContext(ctx =>
            {
                var b1 = new Branch { Name = "MWT_B1", DisplayName = "MWT_B1", Code = "MB1" };
                var b2 = new Branch { Name = "MWT_B2", DisplayName = "MWT_B2", Code = "MB2" };
                ctx.Branchs.AddRange(b1, b2);
                ctx.SaveChanges();
                branch1Id = b1.Id;
                branch2Id = b2.Id;

                var director = new User
                {
                    UserName = "mwt_dir_b1", NormalizedUserName = "MWT_DIR_B1", Name = "Dir", Surname = "B1",
                    EmailAddress = "mwt_dir_b1@ncc.asia", NormalizedEmailAddress = "MWT_DIR_B1@NCC.ASIA",
                    Password = "pwd", IsActive = true, BranchId = branch1Id
                };
                var userB1 = new User
                {
                    UserName = "mwt_dev_b1", NormalizedUserName = "MWT_DEV_B1", Name = "Dev", Surname = "B1",
                    EmailAddress = "mwt_dev_b1@ncc.asia", NormalizedEmailAddress = "MWT_DEV_B1@NCC.ASIA",
                    Password = "pwd", IsActive = true, BranchId = branch1Id
                };
                var userB2 = new User
                {
                    UserName = "mwt_dev_b2", NormalizedUserName = "MWT_DEV_B2", Name = "Dev", Surname = "B2",
                    EmailAddress = "mwt_dev_b2@ncc.asia", NormalizedEmailAddress = "MWT_DEV_B2@NCC.ASIA",
                    Password = "pwd", IsActive = true, BranchId = branch2Id
                };
                ctx.Users.AddRange(director, userB1, userB2);
                ctx.SaveChanges();
                directorId = director.Id;
                userB1Id = userB1.Id;

                var customer = new Customer { Name = "MWT_Cust", Code = "MWTC" };
                ctx.Customers.Add(customer);
                ctx.SaveChanges();

                var project = new Project { Name = "MWT_Proj", Code = "MWTP", CustomerId = customer.Id, Status = ProjectStatus.Active };
                ctx.Projects.Add(project);
                ctx.SaveChanges();
                projectId = project.Id;

                ctx.ProjectUsers.AddRange(
                    new ProjectUser { ProjectId = projectId, UserId = userB1.Id, Type = ProjectUserType.Member },
                    new ProjectUser { ProjectId = projectId, UserId = userB2.Id, Type = ProjectUserType.Member }
                );
                ctx.HistoryWorkingTimes.AddRange(
                    new HistoryWorkingTime
                    {
                        UserId = userB1.Id, MorningStartTime = "08:30", MorningEndTime = "12:00", MorningWorkingTime = 3.5,
                        AfternoonStartTime = "13:00", AfternoonEndTime = "17:30", AfternoonWorkingTime = 4.5,
                        ApplyDate = DateTime.Now.Date, Status = RequestStatus.Pending, WorkMode = WorkMode.WFH
                    },
                    new HistoryWorkingTime
                    {
                        UserId = userB2.Id, MorningStartTime = "08:30", MorningEndTime = "12:00", MorningWorkingTime = 3.5,
                        AfternoonStartTime = "13:00", AfternoonEndTime = "17:30", AfternoonWorkingTime = 4.5,
                        ApplyDate = DateTime.Now.Date, Status = RequestStatus.Pending, WorkMode = WorkMode.Office
                    }
                );
                ctx.SaveChanges();
            });

            var service = CreateManageService(directorId, hasViewByBranch: true);

            await WithUnitOfWorkAsync(async () =>
            {
                var emptyRes = await service.GetAll(new FilterDto { userName = "", projectIds = new List<long>() });
                emptyRes.ShouldBeEmpty();

                var result = await service.GetAll(new FilterDto
                {
                    userName = "",
                    status = RequestStatus.Pending,
                    projectIds = new List<long> { projectId }
                });

                result.Count.ShouldBe(1);
                result[0].UserId.ShouldBe(userB1Id);
                result[0].WorkMode.ShouldBe(WorkMode.WFH);
            });
        }

        [Fact]
        public async Task ApproveWorkingTime_Should_Update_User_WorkMode_For_SameBranch_And_Reject_OtherBranch_Or_MissingPermission()
        {
            long branch1Id = 0, branch2Id = 0, dirB1Id = 0, dirB2Id = 0, userB1Id = 0, historyId = 0;

            UsingDbContext(ctx =>
            {
                var b1 = new Branch { Name = "APP_B1", DisplayName = "APP_B1", Code = "AB1" };
                var b2 = new Branch { Name = "APP_B2", DisplayName = "APP_B2", Code = "AB2" };
                ctx.Branchs.AddRange(b1, b2);
                ctx.SaveChanges();
                branch1Id = b1.Id;
                branch2Id = b2.Id;

                var dirB1 = new User
                {
                    UserName = "app_dir_b1", NormalizedUserName = "APP_DIR_B1", Name = "Dir", Surname = "One",
                    EmailAddress = "app_dir_b1@ncc.asia", NormalizedEmailAddress = "APP_DIR_B1@NCC.ASIA",
                    Password = "pwd", IsActive = true, BranchId = branch1Id
                };
                var dirB2 = new User
                {
                    UserName = "app_dir_b2", NormalizedUserName = "APP_DIR_B2", Name = "Dir", Surname = "Two",
                    EmailAddress = "app_dir_b2@ncc.asia", NormalizedEmailAddress = "APP_DIR_B2@NCC.ASIA",
                    Password = "pwd", IsActive = true, BranchId = branch2Id
                };
                var userB1 = new User
                {
                    UserName = "app_dev_b1", NormalizedUserName = "APP_DEV_B1", Name = "Dev", Surname = "One",
                    EmailAddress = "app_dev_b1@ncc.asia", NormalizedEmailAddress = "APP_DEV_B1@NCC.ASIA",
                    Password = "pwd", IsActive = true, BranchId = branch1Id, WorkMode = WorkMode.Office
                };
                ctx.Users.AddRange(dirB1, dirB2, userB1);
                ctx.SaveChanges();
                dirB1Id = dirB1.Id;
                dirB2Id = dirB2.Id;
                userB1Id = userB1.Id;

                var hwt = new HistoryWorkingTime
                {
                    UserId = userB1Id,
                    MorningStartTime = "08:00", MorningEndTime = "12:00", MorningWorkingTime = 4,
                    AfternoonStartTime = "13:00", AfternoonEndTime = "17:00", AfternoonWorkingTime = 4,
                    ApplyDate = DateTime.Now.Date,
                    Status = RequestStatus.Pending,
                    WorkMode = WorkMode.WFH
                };
                ctx.HistoryWorkingTimes.Add(hwt);
                ctx.SaveChanges();
                historyId = hwt.Id;
            });

            // 1. Not found -> throws
            var dirB1Service = CreateManageService(dirB1Id, hasViewByBranch: true);
            await WithUnitOfWorkAsync(async () =>
            {
                var ex = await Assert.ThrowsAsync<UserFriendlyException>(() => dirB1Service.ApproveWorkingTime(999999));
                ex.Message.ShouldContain("not found");
            });

            // 2. Missing ViewByBranch permission -> throws
            var noPermService = CreateManageService(dirB1Id, hasViewByBranch: false);
            await WithUnitOfWorkAsync(async () =>
            {
                await Assert.ThrowsAsync<UserFriendlyException>(() => noPermService.ApproveWorkingTime(historyId));
                await Assert.ThrowsAsync<UserFriendlyException>(() => noPermService.RejectWorkingTime(historyId));
            });

            // 3. Different branch director -> throws
            var dirB2Service = CreateManageService(dirB2Id, hasViewByBranch: true);
            await WithUnitOfWorkAsync(async () =>
            {
                await Assert.ThrowsAsync<UserFriendlyException>(() => dirB2Service.ApproveWorkingTime(historyId));
                await Assert.ThrowsAsync<UserFriendlyException>(() => dirB2Service.RejectWorkingTime(historyId));
            });

            // 4. Same branch director -> approves and updates user.WorkMode immediately (ApplyDate <= Today)
            await WithUnitOfWorkAsync(async () =>
            {
                await dirB1Service.ApproveWorkingTime(historyId);
            });

            UsingDbContext(ctx =>
            {
                var hwt = ctx.HistoryWorkingTimes.Find(historyId);
                hwt.Status.ShouldBe(RequestStatus.Approved);

                var user = ctx.Users.Find(userB1Id);
                user.WorkMode.ShouldBe(WorkMode.WFH);
            });
        }

        [Fact]
        public async Task GetProjectWorkingTimePM_Should_Return_Active_Projects_With_Active_Branch_Members()
        {
            long branch1Id = 0, dirId = 0, activeProjId = 0;

            UsingDbContext(ctx =>
            {
                var b1 = new Branch { Name = "PM_B1", DisplayName = "PM_B1", Code = "PB1" };
                ctx.Branchs.Add(b1);
                ctx.SaveChanges();
                branch1Id = b1.Id;

                var dir = new User
                {
                    UserName = "pm_dir_b1", NormalizedUserName = "PM_DIR_B1", Name = "Dir", Surname = "PM",
                    EmailAddress = "pm_dir_b1@ncc.asia", NormalizedEmailAddress = "PM_DIR_B1@NCC.ASIA",
                    Password = "pwd", IsActive = true, BranchId = branch1Id
                };
                var dev = new User
                {
                    UserName = "pm_dev_b1", NormalizedUserName = "PM_DEV_B1", Name = "Dev", Surname = "PM",
                    EmailAddress = "pm_dev_b1@ncc.asia", NormalizedEmailAddress = "PM_DEV_B1@NCC.ASIA",
                    Password = "pwd", IsActive = true, BranchId = branch1Id
                };
                ctx.Users.AddRange(dir, dev);
                ctx.SaveChanges();
                dirId = dir.Id;

                var cust = new Customer { Name = "PM_Cust", Code = "PMC" };
                ctx.Customers.Add(cust);
                ctx.SaveChanges();

                var activeProj = new Project { Name = "ActiveBranchProj", Code = "ABP", CustomerId = cust.Id, Status = ProjectStatus.Active };
                var deactiveProj = new Project { Name = "DeactiveBranchProj", Code = "DBP", CustomerId = cust.Id, Status = ProjectStatus.Deactive };
                ctx.Projects.AddRange(activeProj, deactiveProj);
                ctx.SaveChanges();
                activeProjId = activeProj.Id;

                ctx.ProjectUsers.AddRange(
                    new ProjectUser { ProjectId = activeProj.Id, UserId = dev.Id, Type = ProjectUserType.Member },
                    new ProjectUser { ProjectId = deactiveProj.Id, UserId = dev.Id, Type = ProjectUserType.Member }
                );
                ctx.SaveChanges();
            });

            var withPerm = CreateProjectService(dirId, hasViewByBranch: true);
            var withoutPerm = CreateProjectService(dirId, hasViewByBranch: false);

            await WithUnitOfWorkAsync(async () =>
            {
                var empty = await withoutPerm.GetProjectWorkingTimePM();
                empty.ShouldBeEmpty();

                var projects = await withPerm.GetProjectWorkingTimePM();
                projects.ShouldContain(p => p.Id == activeProjId);
                projects.ShouldNotContain(p => p.Name == "DeactiveBranchProj");
            });
        }

        [Fact]
        public async Task WorkingTimeBackgroundJob_Should_Update_User_WorkMode()
        {
            long userId = 0;
            UsingDbContext(ctx =>
            {
                var user = new User
                {
                    UserName = "bgjob_wt_user", NormalizedUserName = "BGJOB_WT_USER", Name = "Job", Surname = "User",
                    EmailAddress = "bgjob_wt_user@ncc.asia", NormalizedEmailAddress = "BGJOB_WT_USER@NCC.ASIA",
                    Password = "pwd", IsActive = true, WorkMode = WorkMode.Office
                };
                ctx.Users.Add(user);
                ctx.SaveChanges();
                userId = user.Id;
            });

            var job = new WorkingTimeBackgroundJob(
                Resolve<IRepository<User, long>>(),
                Resolve<IRepository<AbsenceDayDetail, long>>())
            {
                UnitOfWorkManager = Resolve<IUnitOfWorkManager>()
            };

            await WithUnitOfWorkAsync(() =>
            {
                job.Execute(new WorkingTimeBackgroundJobArgs
                {
                    Target = new HistoryWorkingTime
                    {
                        UserId = userId,
                        MorningStartTime = "08:30", MorningEndTime = "12:00", MorningWorkingTime = 3.5,
                        AfternoonStartTime = "13:00", AfternoonEndTime = "17:30", AfternoonWorkingTime = 4.5,
                        ApplyDate = DateTime.Now.Date,
                        WorkMode = WorkMode.WFH
                    }
                });
                return Task.CompletedTask;
            });

            UsingDbContext(ctx =>
            {
                ctx.Users.Find(userId).WorkMode.ShouldBe(WorkMode.WFH);
            });
        }

        [Fact]
        public async Task ApproveWorkingTime_Should_Keep_Current_WorkMode_When_ApplyDate_Is_In_Future()
        {
            long branchId = 0, dirId = 0, devId = 0, historyId = 0;

            UsingDbContext(ctx =>
            {
                var b = new Branch { Name = "FUT_B", DisplayName = "FUT_B", Code = "FUTB" };
                ctx.Branchs.Add(b);
                ctx.SaveChanges();
                branchId = b.Id;

                var dir = new User
                {
                    UserName = "fut_dir", NormalizedUserName = "FUT_DIR", Name = "Dir", Surname = "Fut",
                    EmailAddress = "fut_dir@ncc.asia", NormalizedEmailAddress = "FUT_DIR@NCC.ASIA",
                    Password = "pwd", IsActive = true, BranchId = branchId
                };
                var dev = new User
                {
                    UserName = "fut_dev", NormalizedUserName = "FUT_DEV", Name = "Dev", Surname = "Fut",
                    EmailAddress = "fut_dev@ncc.asia", NormalizedEmailAddress = "FUT_DEV@NCC.ASIA",
                    Password = "pwd", IsActive = true, BranchId = branchId, WorkMode = WorkMode.Office
                };
                ctx.Users.AddRange(dir, dev);
                ctx.SaveChanges();
                dirId = dir.Id;
                devId = dev.Id;

                var hwt = new HistoryWorkingTime
                {
                    UserId = devId,
                    MorningStartTime = "08:00", MorningEndTime = "12:00", MorningWorkingTime = 4,
                    AfternoonStartTime = "13:00", AfternoonEndTime = "17:00", AfternoonWorkingTime = 4,
                    ApplyDate = DateTime.Now.Date.AddDays(5),
                    Status = RequestStatus.Pending,
                    WorkMode = WorkMode.WFH
                };
                ctx.HistoryWorkingTimes.Add(hwt);
                ctx.SaveChanges();
                historyId = hwt.Id;
            });

            var dirService = CreateManageService(dirId, hasViewByBranch: true);
            await WithUnitOfWorkAsync(async () =>
            {
                await dirService.ApproveWorkingTime(historyId);
            });

            UsingDbContext(ctx =>
            {
                ctx.HistoryWorkingTimes.Find(historyId).Status.ShouldBe(RequestStatus.Approved);
                // ApplyDate is in the future -> user's current WorkMode remains Office until background job executes
                ctx.Users.Find(devId).WorkMode.ShouldBe(WorkMode.Office);
            });
        }
    }
}
