using Abp.Authorization.Roles;
using Abp.Authorization.Users;
using Abp.BackgroundJobs;
using Abp.Configuration;
using Abp.Domain.Uow;
using Abp.Runtime.Session;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Ncc.Authorization;
using Ncc.Authorization.Roles;
using Ncc.Authorization.Users;
using Ncc.Configuration;
using Ncc.Entities;
using Ncc.IoC;
using NSubstitute;
using Shouldly;
using System;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Timesheet.APIs.MyWorkingTimes;
using Timesheet.APIs.MyWorkingTimes.Dto;
using Timesheet.Entities;
using Timesheet.Services.Komu;
using Timesheet.Services.Mezon;
using Xunit;
using static Ncc.Entities.Enum.StatusEnum;
using Branch = Timesheet.Entities.Branch;
using Task = System.Threading.Tasks.Task;

namespace Timesheet.Application.Tests.MyWorkingTimes
{
    public class MyWorkingTimeAppService_WorkMode_Tests : TimesheetApplicationTestBase
    {
        private readonly IWorkScope _workScope;
        private readonly IBackgroundJobManager _backgroundJobManager;
        private readonly KomuService _komuService;
        private readonly ISettingManager _settingManager;

        public MyWorkingTimeAppService_WorkMode_Tests()
        {
            _workScope = Resolve<IWorkScope>();
            _backgroundJobManager = Resolve<IBackgroundJobManager>();

            var httpClient = Resolve<HttpClient>();
            var logger = Resolve<ILogger<KomuService>>();
            var configuration = Substitute.For<IConfiguration>();
            configuration.GetValue<string>("KomuService:BaseAddress").Returns("http://www.myserver.com");
            configuration.GetValue<string>("KomuService:SecurityCode").Returns("secretCode");
            configuration.GetValue<string>("KomuService:DevModeChannelId").Returns("_channelIdDevMode");
            configuration.GetValue<string>("KomuService:EnableKomuNotification").Returns("true");

            _settingManager = Substitute.For<ISettingManager>();
            _settingManager.GetSettingValueForApplicationAsync(AppSettingNames.SendKomuRequest).Returns(Task.FromResult("false"));
            _komuService = Substitute.For<KomuService>(httpClient, logger, configuration, _settingManager);
        }

        private MyWorkingTimeAppService CreateService(long sessionUserId)
        {
            var session = Substitute.For<IAbpSession>();
            session.UserId.Returns(sessionUserId);
            session.TenantId.Returns((int?)null);

            var mezonService = Resolve<MezonService>();
            return new MyWorkingTimeAppService(_backgroundJobManager, _komuService, _workScope, mezonService)
            {
                AbpSession = session,
                UnitOfWorkManager = Resolve<IUnitOfWorkManager>(),
                SettingManager = _settingManager
            };
        }

        [Fact]
        public async Task GetMyCurrentWorkingTime_Should_Return_User_WorkMode()
        {
            long userId = 0;
            UsingDbContext(ctx =>
            {
                var user = new User
                {
                    UserName = "mywt_wfh_user",
                    NormalizedUserName = "MYWT_WFH_USER",
                    Name = "WFH",
                    Surname = "User",
                    EmailAddress = "mywt_wfh_user@ncc.asia",
                    NormalizedEmailAddress = "MYWT_WFH_USER@NCC.ASIA",
                    Password = "hashed_password",
                    IsActive = true,
                    MorningStartAt = "08:30",
                    MorningEndAt = "12:00",
                    MorningWorking = 3.5,
                    AfternoonStartAt = "13:00",
                    AfternoonEndAt = "17:30",
                    AfternoonWorking = 4.5,
                    WorkMode = WorkMode.WFH
                };
                ctx.Users.Add(user);
                ctx.SaveChanges();
                userId = user.Id;
            });

            var service = CreateService(userId);
            await WithUnitOfWorkAsync(async () =>
            {
                var result = await service.GetMyCurrentWorkingTime();
                result.WorkMode.ShouldBe(WorkMode.WFH);
            });
        }

        [Fact]
        public async Task SubmitAndEditWorkingTime_Should_Persist_And_Return_WorkMode()
        {
            long userId = 0;
            UsingDbContext(ctx =>
            {
                var user = new User
                {
                    UserName = "mywt_submit_user",
                    NormalizedUserName = "MYWT_SUBMIT_USER",
                    Name = "Submit",
                    Surname = "User",
                    EmailAddress = "mywt_submit_user@ncc.asia",
                    NormalizedEmailAddress = "MYWT_SUBMIT_USER@NCC.ASIA",
                    Password = "hashed_password",
                    IsActive = true,
                    MorningStartAt = "08:30",
                    MorningEndAt = "12:00",
                    MorningWorking = 3.5,
                    AfternoonStartAt = "13:00",
                    AfternoonEndAt = "17:30",
                    AfternoonWorking = 4.5,
                    WorkMode = WorkMode.Office
                };
                ctx.Users.Add(user);
                ctx.SaveChanges();
                userId = user.Id;
            });

            var service = CreateService(userId);
            long historyId = 0;

            await WithUnitOfWorkAsync(async () =>
            {
                var submitted = await service.SubmitNewWorkingTime(new ChangeWorkingTimeDto
                {
                    MorningStartTime = "08:00",
                    MorningEndTime = "12:00",
                    MorningWorkingTime = 4,
                    AfternoonStartTime = "13:00",
                    AfternoonEndTime = "17:00",
                    AfternoonWorkingTime = 4,
                    ApplyDate = DateTime.Now.Date.AddDays(5),
                    WorkMode = WorkMode.WFH
                });
                historyId = submitted.Id;
            });

            await WithUnitOfWorkAsync(async () =>
            {
                var history = service.GetAllMyHistoryWorkingTime();
                var item = history.FirstOrDefault(h => h.Id == historyId);
                item.ShouldNotBeNull();
                item.WorkMode.ShouldBe(WorkMode.WFH);
                item.Status.ShouldBe(RequestStatus.Pending);

                await service.EditWorkingTime(new HistoryWorkingTimeDto
                {
                    Id = historyId,
                    MorningStartTime = "08:30",
                    MorningEndTime = "12:00",
                    MorningWorkingTime = 3.5,
                    AfternoonStartTime = "13:00",
                    AfternoonEndTime = "17:30",
                    AfternoonWorkingTime = 4.5,
                    ApplyDate = DateTime.Now.Date.AddDays(6),
                    WorkMode = WorkMode.Office
                });
            });

            await WithUnitOfWorkAsync(() =>
            {
                var history = service.GetAllMyHistoryWorkingTime();
                var updated = history.First(h => h.Id == historyId);
                updated.WorkMode.ShouldBe(WorkMode.Office);
                return Task.CompletedTask;
            });
        }

        [Fact]
        public async Task SubmitNewWorkingTime_Should_Notify_Only_SameBranch_Approvers_With_Both_Permissions()
        {
            _settingManager.GetSettingValueForApplicationAsync(AppSettingNames.SendKomuRequest).Returns(Task.FromResult("true"));

            long branch1Id = 0;
            long branch2Id = 0;
            long requesterId = 0;

            UsingDbContext(ctx =>
            {
                var b1 = new Branch { Name = "UT_HN1", DisplayName = "UT_HN1", Code = "UTH1" };
                var b2 = new Branch { Name = "UT_DN1", DisplayName = "UT_DN1", Code = "UTD1" };
                ctx.Branchs.AddRange(b1, b2);
                ctx.SaveChanges();
                branch1Id = b1.Id;
                branch2Id = b2.Id;

                // Two roles splitting the required permissions to verify multi-role aggregation
                var roleView = new Role { Name = "UT_ViewBranchRole", DisplayName = "UT_ViewBranchRole", NormalizedName = "UT_VIEWBRANCHROLE" };
                var roleApprove = new Role { Name = "UT_ApproveWTRole", DisplayName = "UT_ApproveWTRole", NormalizedName = "UT_APPROVEWTROLE" };
                ctx.Roles.AddRange(roleView, roleApprove);
                ctx.SaveChanges();

                ctx.RolePermissions.Add(new RolePermissionSetting
                {
                    RoleId = roleView.Id,
                    Name = PermissionNames.ManageWorkingTime_ViewByBranch,
                    IsGranted = true
                });
                ctx.RolePermissions.Add(new RolePermissionSetting
                {
                    RoleId = roleApprove.Id,
                    Name = PermissionNames.ManageWorkingTime_Approval,
                    IsGranted = true
                });

                var requester = new User
                {
                    UserName = "ut_req_wt",
                    NormalizedUserName = "UT_REQ_WT",
                    Name = "Req",
                    Surname = "User",
                    EmailAddress = "ut_req_wt@ncc.asia",
                    NormalizedEmailAddress = "UT_REQ_WT@NCC.ASIA",
                    Password = "pwd",
                    IsActive = true,
                    BranchId = branch1Id,
                    WorkMode = WorkMode.Office
                };
                // Valid approver in Branch 1 (has both roles)
                var approverBranch1 = new User
                {
                    UserName = "director_hn1",
                    NormalizedUserName = "DIRECTOR_HN1",
                    Name = "Director",
                    Surname = "HN1",
                    EmailAddress = "director.hn1@ncc.asia",
                    NormalizedEmailAddress = "DIRECTOR.HN1@NCC.ASIA",
                    Password = "pwd",
                    IsActive = true,
                    BranchId = branch1Id
                };
                // Approver in Branch 2 (wrong branch -> must NOT be notified)
                var approverBranch2 = new User
                {
                    UserName = "director_dn1",
                    NormalizedUserName = "DIRECTOR_DN1",
                    Name = "Director",
                    Surname = "DN1",
                    EmailAddress = "director.dn1@ncc.asia",
                    NormalizedEmailAddress = "DIRECTOR.DN1@NCC.ASIA",
                    Password = "pwd",
                    IsActive = true,
                    BranchId = branch2Id
                };
                // User in Branch 1 with only ViewByBranch (missing Approval -> must NOT be notified)
                var partialUser = new User
                {
                    UserName = "viewer_hn1",
                    NormalizedUserName = "VIEWER_HN1",
                    Name = "Viewer",
                    Surname = "HN1",
                    EmailAddress = "viewer.hn1@ncc.asia",
                    NormalizedEmailAddress = "VIEWER.HN1@NCC.ASIA",
                    Password = "pwd",
                    IsActive = true,
                    BranchId = branch1Id
                };

                ctx.Users.AddRange(requester, approverBranch1, approverBranch2, partialUser);
                ctx.SaveChanges();
                requesterId = requester.Id;

                ctx.UserRoles.AddRange(
                    new UserRole(null, approverBranch1.Id, roleView.Id),
                    new UserRole(null, approverBranch1.Id, roleApprove.Id),
                    new UserRole(null, approverBranch2.Id, roleView.Id),
                    new UserRole(null, approverBranch2.Id, roleApprove.Id),
                    new UserRole(null, partialUser.Id, roleView.Id)
                );
                ctx.SaveChanges();
            });

            var service = CreateService(requesterId);
            await WithUnitOfWorkAsync(async () =>
            {
                await service.SubmitNewWorkingTime(new ChangeWorkingTimeDto
                {
                    MorningStartTime = "08:30",
                    MorningEndTime = "12:00",
                    MorningWorkingTime = 3.5,
                    AfternoonStartTime = "13:00",
                    AfternoonEndTime = "17:30",
                    AfternoonWorkingTime = 4.5,
                    ApplyDate = DateTime.Now.Date.AddDays(3),
                    WorkMode = WorkMode.WFH
                });
            });

            _komuService.Received(1).SendSimpleNotificationToUser(
                Arg.Is<string>(msg => msg.Contains("Work mode: WFH")),
                "director.hn1");
            _komuService.DidNotReceive().SendSimpleNotificationToUser(Arg.Any<string>(), "director.dn1");
            _komuService.DidNotReceive().SendSimpleNotificationToUser(Arg.Any<string>(), "viewer.hn1");
        }
    }
}
