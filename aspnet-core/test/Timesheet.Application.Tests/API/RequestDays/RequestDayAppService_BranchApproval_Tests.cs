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
using Ncc.Configuration;
using Ncc.Entities;
using Ncc.IoC;
using NSubstitute;
using Shouldly;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Timesheet.APIs.RequestDays;
using Timesheet.APIs.RequestDays.Dto;
using Timesheet.DomainServices;
using Timesheet.Entities;
using Timesheet.Services.FaceIdService;
using Timesheet.Services.Komu;
using Timesheet.Services.Mezon;
using Timesheet.Services.Tracker;
using Timesheet.Services.W2;
using Xunit;
using static Ncc.Entities.Enum.StatusEnum;
using Branch = Timesheet.Entities.Branch;
using Task = System.Threading.Tasks.Task;

namespace Timesheet.Application.Tests.API.RequestDays
{
    public class RequestDayAppService_BranchApproval_Tests : TimesheetApplicationTestBase
    {
        private readonly IWorkScope _workScope;

        public RequestDayAppService_BranchApproval_Tests()
        {
            _workScope = Resolve<IWorkScope>();
        }

        private (RequestDayAppService Service, W2Service W2) CreateService(long sessionUserId, bool isViewProject, bool isViewBranch)
        {
            var settingManager = Substitute.For<ISettingManager>();
            settingManager.GetSettingValueForApplicationAsync(AppSettingNames.SendKomuRequest).Returns(Task.FromResult("false"));
            settingManager.GetSettingValueAsync(AppSettingNames.LockTimesheet).Returns(Task.FromResult("false"));

            var komuConfig = new ConfigurationBuilder()
                .AddInMemoryCollection(new Dictionary<string, string>
                {
                    { "KomuService:ChannelIdDevMode", "" },
                    { "KomuService:EnableKomuNotification", "false" },
                    { "KomuService:BaseAddress", "http://example.com" },
                    { "KomuService:SecurityCode", "secret" }
                }).Build();

            var trackerConfig = new ConfigurationBuilder()
                .AddInMemoryCollection(new Dictionary<string, string>
                {
                    { "TrackerService:BaseAddress", "http://example.com" },
                    { "TrackerService:SecurityCode", "secret" }
                }).Build();

            var w2Config = new ConfigurationBuilder()
                .AddInMemoryCollection(new Dictionary<string, string>
                {
                    { "W2Service:BaseAddress", "http://example.com" },
                    { "W2Service:SecurityCode", "secret" }
                }).Build();

            var httpClient = Resolve<HttpClient>();
            var komuService = Substitute.For<KomuService>(httpClient, Resolve<ILogger<KomuService>>(), komuConfig, settingManager);
            var trackerService = Substitute.For<TrackerService>(httpClient, trackerConfig, Resolve<ILogger<TrackerService>>());
            var faceIdService = Substitute.For<FaceIdService>(httpClient, Resolve<ILogger<FaceIdService>>(), settingManager, _workScope);
            var timekeepingService = Substitute.For<TimekeepingServices>(komuService, _workScope, faceIdService, _projectService, settingManager);
            var approveRequestOffService = Substitute.For<ApproveRequestOffServices>(
                Resolve<IRepository<AbsenceDayDetail, long>>(),
                Resolve<IRepository<ProjectUser, long>>(),
                komuService);
            var w2Service = Substitute.For<W2Service>(Resolve<ILogger<W2Service>>(), _workScope, w2Config);
            var mezonService = Resolve<MezonService>();
            var commonService = Substitute.For<CommonServices>(settingManager);

            var session = Substitute.For<IAbpSession>();
            session.UserId.Returns(sessionUserId);
            session.TenantId.Returns((int?)null);

            var permissionChecker = Substitute.For<IPermissionChecker>();
            permissionChecker.IsGrantedAsync(PermissionNames.AbsenceDayByProject_View).Returns(Task.FromResult(isViewProject));
            permissionChecker.IsGrantedAsync(PermissionNames.AbsenceDayByProject_ViewByBranch).Returns(Task.FromResult(isViewBranch));

            var service = new RequestDayAppService(
                Resolve<IBackgroundJobManager>(),
                komuService,
                timekeepingService,
                _workScope,
                approveRequestOffService,
                w2Service,
                mezonService,
                commonService)
            {
                AbpSession = session,
                SettingManager = settingManager,
                UnitOfWorkManager = Resolve<IUnitOfWorkManager>(),
                PermissionChecker = permissionChecker
            };

            return (service, w2Service);
        }

        [Fact]
        public async Task ApproveAndRejectRequest_Should_Allow_Only_SameBranch_Director_For_Remote_And_WorkAtOffice_Without_W2()
        {
            long b1Id = 0, b2Id = 0, dirB1Id = 0, dirB2Id = 0, pmId = 0;
            long rejectedRemoteId = 0, pendingWorkAtOfficeId = 0;

            UsingDbContext(ctx =>
            {
                var b1 = new Branch { Name = "BA_B1", DisplayName = "BA_B1", Code = "BAB1" };
                var b2 = new Branch { Name = "BA_B2", DisplayName = "BA_B2", Code = "BAB2" };
                ctx.Branchs.AddRange(b1, b2);
                ctx.SaveChanges();
                b1Id = b1.Id;
                b2Id = b2.Id;

                var dayOffType = ctx.DayOffTypes.FirstOrDefault() ?? new DayOffType { Name = "UT_Off", Length = 1, Status = OffTypeStatus.CoPhep };
                if (dayOffType.Id == 0)
                {
                    ctx.DayOffTypes.Add(dayOffType);
                    ctx.SaveChanges();
                }

                var dirB1 = new User
                {
                    UserName = "ba_dir_b1", NormalizedUserName = "BA_DIR_B1", Name = "Dir", Surname = "B1",
                    EmailAddress = "ba_dir_b1@ncc.asia", NormalizedEmailAddress = "BA_DIR_B1@NCC.ASIA",
                    Password = "pwd", IsActive = true, BranchId = b1Id
                };
                var dirB2 = new User
                {
                    UserName = "ba_dir_b2", NormalizedUserName = "BA_DIR_B2", Name = "Dir", Surname = "B2",
                    EmailAddress = "ba_dir_b2@ncc.asia", NormalizedEmailAddress = "BA_DIR_B2@NCC.ASIA",
                    Password = "pwd", IsActive = true, BranchId = b2Id
                };
                var pm = new User
                {
                    UserName = "ba_pm", NormalizedUserName = "BA_PM", Name = "PM", Surname = "User",
                    EmailAddress = "ba_pm@ncc.asia", NormalizedEmailAddress = "BA_PM@NCC.ASIA",
                    Password = "pwd", IsActive = true, BranchId = b1Id
                };
                var devB1 = new User
                {
                    UserName = "ba_dev_b1", NormalizedUserName = "BA_DEV_B1", Name = "Dev", Surname = "B1",
                    EmailAddress = "ba_dev_b1@ncc.asia", NormalizedEmailAddress = "BA_DEV_B1@NCC.ASIA",
                    Password = "pwd", IsActive = true, BranchId = b1Id
                };
                ctx.Users.AddRange(dirB1, dirB2, pm, devB1);
                ctx.SaveChanges();
                dirB1Id = dirB1.Id;
                dirB2Id = dirB2.Id;
                pmId = pm.Id;

                var customer = new Customer { Name = "BA_Cust", Code = "BAC" };
                ctx.Customers.Add(customer);
                ctx.SaveChanges();
                var project = new Project { Name = "BA_Proj", Code = "BAP", CustomerId = customer.Id, Status = ProjectStatus.Active };
                ctx.Projects.Add(project);
                ctx.SaveChanges();
                ctx.ProjectUsers.AddRange(
                    new ProjectUser { ProjectId = project.Id, UserId = pm.Id, Type = ProjectUserType.PM },
                    new ProjectUser { ProjectId = project.Id, UserId = devB1.Id, Type = ProjectUserType.Member }
                );

                var rejectedRemote = new AbsenceDayRequest
                {
                    UserId = devB1.Id, Type = RequestType.Remote, Status = RequestStatus.Rejected,
                    Reason = "Auto-rejected remote", DayOffTypeId = dayOffType.Id
                };
                var pendingWao = new AbsenceDayRequest
                {
                    UserId = devB1.Id, Type = RequestType.WorkAtOffice, Status = RequestStatus.Pending,
                    Reason = "Work at office", DayOffTypeId = dayOffType.Id
                };
                ctx.AbsenceDayRequests.AddRange(rejectedRemote, pendingWao);
                ctx.SaveChanges();
                rejectedRemoteId = rejectedRemote.Id;
                pendingWorkAtOfficeId = pendingWao.Id;

                ctx.AbsenceDayDetails.AddRange(
                    new AbsenceDayDetail { RequestId = rejectedRemoteId, DateAt = DateTime.Now.Date.AddDays(3), DateType = DayType.Fullday, Hour = 8 },
                    new AbsenceDayDetail { RequestId = pendingWorkAtOfficeId, DateAt = DateTime.Now.Date.AddDays(4), DateType = DayType.Fullday, Hour = 8 }
                );
                ctx.SaveChanges();
            });

            // 1. PM (no ViewByBranch) cannot approve or reject Remote / WorkAtOffice
            var (pmService, _) = CreateService(pmId, isViewProject: true, isViewBranch: false);
            await WithUnitOfWorkAsync(async () =>
            {
                await Assert.ThrowsAsync<UserFriendlyException>(() => pmService.ApproveRequest(new[] { pendingWorkAtOfficeId }));
                await Assert.ThrowsAsync<UserFriendlyException>(() => pmService.RejectRequest(new[] { pendingWorkAtOfficeId }));
            });

            // 2. Director of Branch 2 cannot approve or reject Branch 1 user's Remote / WorkAtOffice
            var (dirB2Service, _) = CreateService(dirB2Id, isViewProject: false, isViewBranch: true);
            await WithUnitOfWorkAsync(async () =>
            {
                await Assert.ThrowsAsync<UserFriendlyException>(() => dirB2Service.ApproveRequest(new[] { rejectedRemoteId }));
                await Assert.ThrowsAsync<UserFriendlyException>(() => dirB2Service.RejectRequest(new[] { pendingWorkAtOfficeId }));
            });

            // 3. Director of Branch 1 approves auto-rejected Remote directly (NO W2 call) & rejects WorkAtOffice
            var (dirB1Service, w2) = CreateService(dirB1Id, isViewProject: false, isViewBranch: true);
            await WithUnitOfWorkAsync(async () =>
            {
                await dirB1Service.ApproveRequest(new[] { rejectedRemoteId });
                await dirB1Service.RejectRequest(new[] { pendingWorkAtOfficeId });
            });

            w2.DidNotReceiveWithAnyArgs().GetWfhRequest(default, default);

            UsingDbContext(ctx =>
            {
                ctx.AbsenceDayRequests.Find(rejectedRemoteId).Status.ShouldBe(RequestStatus.Approved);
                ctx.AbsenceDayRequests.Find(pendingWorkAtOfficeId).Status.ShouldBe(RequestStatus.Rejected);
            });
        }

        [Fact]
        public async Task GetAllRequest_Should_Isolate_BranchFilter_And_ProjectFilter()
        {
            long b1Id = 0, b2Id = 0, dirB1Id = 0, projId = 0, devB1Id = 0, devB2Id = 0;
            var targetDate = new DateTime(2026, 11, 20);

            UsingDbContext(ctx =>
            {
                var b1 = new Branch { Name = "GAR_B1", DisplayName = "GAR_B1", Code = "GB1" };
                var b2 = new Branch { Name = "GAR_B2", DisplayName = "GAR_B2", Code = "GB2" };
                ctx.Branchs.AddRange(b1, b2);
                ctx.SaveChanges();
                b1Id = b1.Id;
                b2Id = b2.Id;

                var dirB1 = new User
                {
                    UserName = "gar_dir_b1", NormalizedUserName = "GAR_DIR_B1", Name = "Dir", Surname = "GAR",
                    EmailAddress = "gar_dir_b1@ncc.asia", NormalizedEmailAddress = "GAR_DIR_B1@NCC.ASIA",
                    Password = "pwd", IsActive = true, BranchId = b1Id
                };
                var devB1 = new User
                {
                    UserName = "gar_dev_b1", NormalizedUserName = "GAR_DEV_B1", Name = "Dev1", Surname = "GAR",
                    EmailAddress = "gar_dev_b1@ncc.asia", NormalizedEmailAddress = "GAR_DEV_B1@NCC.ASIA",
                    Password = "pwd", IsActive = true, BranchId = b1Id
                };
                var devB2 = new User
                {
                    UserName = "gar_dev_b2", NormalizedUserName = "GAR_DEV_B2", Name = "Dev2", Surname = "GAR",
                    EmailAddress = "gar_dev_b2@ncc.asia", NormalizedEmailAddress = "GAR_DEV_B2@NCC.ASIA",
                    Password = "pwd", IsActive = true, BranchId = b2Id
                };
                ctx.Users.AddRange(dirB1, devB1, devB2);
                ctx.SaveChanges();
                dirB1Id = dirB1.Id;
                devB1Id = devB1.Id;
                devB2Id = devB2.Id;

                var cust = new Customer { Name = "GAR_Cust", Code = "GARC" };
                ctx.Customers.Add(cust);
                ctx.SaveChanges();
                var proj = new Project { Name = "GAR_Proj", Code = "GARP", CustomerId = cust.Id, Status = ProjectStatus.Active };
                ctx.Projects.Add(proj);
                ctx.SaveChanges();
                projId = proj.Id;

                ctx.ProjectUsers.AddRange(
                    new ProjectUser { ProjectId = projId, UserId = dirB1Id, Type = ProjectUserType.PM },
                    new ProjectUser { ProjectId = projId, UserId = devB1Id, Type = ProjectUserType.Member },
                    new ProjectUser { ProjectId = projId, UserId = devB2Id, Type = ProjectUserType.Member }
                );

                var dayOffType = ctx.DayOffTypes.FirstOrDefault() ?? new DayOffType { Name = "GAR_Off", Length = 1, Status = OffTypeStatus.CoPhep };
                if (dayOffType.Id == 0)
                {
                    ctx.DayOffTypes.Add(dayOffType);
                    ctx.SaveChanges();
                }

                var r1 = new AbsenceDayRequest { UserId = devB1Id, Type = RequestType.WorkAtOffice, Status = RequestStatus.Pending, Reason = "B1", DayOffTypeId = dayOffType.Id };
                var r2 = new AbsenceDayRequest { UserId = devB2Id, Type = RequestType.WorkAtOffice, Status = RequestStatus.Pending, Reason = "B2", DayOffTypeId = dayOffType.Id };
                ctx.AbsenceDayRequests.AddRange(r1, r2);
                ctx.SaveChanges();

                ctx.AbsenceDayDetails.AddRange(
                    new AbsenceDayDetail { RequestId = r1.Id, DateAt = targetDate, DateType = DayType.Fullday, Hour = 8 },
                    new AbsenceDayDetail { RequestId = r2.Id, DateAt = targetDate, DateType = DayType.Fullday, Hour = 8 }
                );
                ctx.SaveChanges();
            });

            // Filter by branch (empty projectIds) -> ONLY Branch 1 user even though dirB1 is PM of devB2
            var (branchAndPmService, _) = CreateService(dirB1Id, isViewProject: true, isViewBranch: true);
            await WithUnitOfWorkAsync(async () =>
            {
                var byBranch = await branchAndPmService.GetAllRequest(new InputRequestDto
                {
                    startDate = targetDate.AddDays(-1),
                    endDate = targetDate.AddDays(1),
                    projectIds = new List<long>(),
                    type = RequestType.WorkAtOffice,
                    dayOffTypeId = -1,
                    status = -1
                });

                byBranch.ShouldContain(x => x.UserId == devB1Id);
                byBranch.ShouldNotContain(x => x.UserId == devB2Id);

                // Filter by project -> BOTH Branch 1 and Branch 2 project members
                var byProject = await branchAndPmService.GetAllRequest(new InputRequestDto
                {
                    startDate = targetDate.AddDays(-1),
                    endDate = targetDate.AddDays(1),
                    projectIds = new List<long> { projId },
                    type = RequestType.WorkAtOffice,
                    dayOffTypeId = -1,
                    status = -1
                });

                byProject.ShouldContain(x => x.UserId == devB1Id);
                byProject.ShouldContain(x => x.UserId == devB2Id);
            });
        }

        [Fact]
        public async Task ApproveAndRejectRequest_Should_Allow_PM_For_Off_And_Onsite_Without_ViewByBranch()
        {
            long pmId = 0, offReqId = 0, onsiteReqId = 0;

            UsingDbContext(ctx =>
            {
                var branch = new Branch { Name = "PM_OFF_B", DisplayName = "PM_OFF_B", Code = "POB" };
                ctx.Branchs.Add(branch);
                ctx.SaveChanges();

                var dayOffType = ctx.DayOffTypes.FirstOrDefault() ?? new DayOffType { Name = "PM_OffType", Length = 1, Status = OffTypeStatus.CoPhep };
                if (dayOffType.Id == 0)
                {
                    ctx.DayOffTypes.Add(dayOffType);
                    ctx.SaveChanges();
                }

                var pm = new User
                {
                    UserName = "pm_off_mgr", NormalizedUserName = "PM_OFF_MGR", Name = "PM", Surname = "Off",
                    EmailAddress = "pm_off_mgr@ncc.asia", NormalizedEmailAddress = "PM_OFF_MGR@NCC.ASIA",
                    Password = "pwd", IsActive = true, BranchId = branch.Id
                };
                var dev = new User
                {
                    UserName = "pm_off_dev", NormalizedUserName = "PM_OFF_DEV", Name = "Dev", Surname = "Off",
                    EmailAddress = "pm_off_dev@ncc.asia", NormalizedEmailAddress = "PM_OFF_DEV@NCC.ASIA",
                    Password = "pwd", IsActive = true, BranchId = branch.Id
                };
                ctx.Users.AddRange(pm, dev);
                ctx.SaveChanges();
                pmId = pm.Id;

                var cust = new Customer { Name = "PM_Off_Cust", Code = "POC" };
                ctx.Customers.Add(cust);
                ctx.SaveChanges();
                var proj = new Project { Name = "PM_Off_Proj", Code = "POP", CustomerId = cust.Id, Status = ProjectStatus.Active };
                ctx.Projects.Add(proj);
                ctx.SaveChanges();
                ctx.ProjectUsers.AddRange(
                    new ProjectUser { ProjectId = proj.Id, UserId = pm.Id, Type = ProjectUserType.PM },
                    new ProjectUser { ProjectId = proj.Id, UserId = dev.Id, Type = ProjectUserType.Member }
                );

                var offReq = new AbsenceDayRequest { UserId = dev.Id, Type = RequestType.Off, Status = RequestStatus.Pending, Reason = "Off", DayOffTypeId = dayOffType.Id };
                var onsiteReq = new AbsenceDayRequest { UserId = dev.Id, Type = RequestType.Onsite, Status = RequestStatus.Pending, Reason = "Onsite", DayOffTypeId = dayOffType.Id };
                ctx.AbsenceDayRequests.AddRange(offReq, onsiteReq);
                ctx.SaveChanges();
                offReqId = offReq.Id;
                onsiteReqId = onsiteReq.Id;

                ctx.AbsenceDayDetails.AddRange(
                    new AbsenceDayDetail { RequestId = offReqId, DateAt = DateTime.Now.Date.AddDays(5), DateType = DayType.Fullday, Hour = 8 },
                    new AbsenceDayDetail { RequestId = onsiteReqId, DateAt = DateTime.Now.Date.AddDays(6), DateType = DayType.Fullday, Hour = 8 }
                );
                ctx.SaveChanges();
            });

            var (pmService, _) = CreateService(pmId, isViewProject: true, isViewBranch: false);
            await WithUnitOfWorkAsync(async () =>
            {
                await pmService.ApproveRequest(new[] { offReqId });
                await pmService.RejectRequest(new[] { onsiteReqId });
            });

            UsingDbContext(ctx =>
            {
                ctx.AbsenceDayRequests.Find(offReqId).Status.ShouldBe(RequestStatus.Approved);
                ctx.AbsenceDayRequests.Find(onsiteReqId).Status.ShouldBe(RequestStatus.Rejected);
            });
        }
    }
}
