using Abp.Application.Services.Dto;
using Abp.BackgroundJobs;
using Abp.Configuration;
using Abp.Domain.Repositories;
using Abp.Domain.Uow;
using Abp.Runtime.Session;
using Abp.UI;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
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
using Timesheet.APIs.MyAbsenceDays.Dto;
using Timesheet.APIs.RequestDays;
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
    public class RequestDayAppService_WorkModePolicy_Tests : TimesheetApplicationTestBase
    {
        private readonly IWorkScope _workScope;

        public RequestDayAppService_WorkModePolicy_Tests()
        {
            _workScope = Resolve<IWorkScope>();
        }

        private (RequestDayAppService Service, ISettingManager Settings) CreateService(
            long sessionUserId,
            string wfhSetting = "1",
            string officeDaysInAdvance = "7",
            string wfhMaxOffice = "1",
            string wfhDaysInAdvance = "7",
            string allowInternRemote = "true",
            string allowProbationRemote = "true")
        {
            var settingManager = Substitute.For<ISettingManager>();
            settingManager.GetSettingValueAsync(AppSettingNames.WFHSetting).Returns(Task.FromResult(wfhSetting));
            settingManager.GetSettingValueAsync(AppSettingNames.OfficeDaysInAdvanceToRequestRemote).Returns(Task.FromResult(officeDaysInAdvance));
            settingManager.GetSettingValueAsync(AppSettingNames.WfhMaxOfficeDaysPerWeek).Returns(Task.FromResult(wfhMaxOffice));
            settingManager.GetSettingValueAsync(AppSettingNames.WfhDaysInAdvanceToRequestOffice).Returns(Task.FromResult(wfhDaysInAdvance));
            settingManager.GetSettingValueAsync(AppSettingNames.AllowInternToWorkRemote).Returns(Task.FromResult(allowInternRemote));
            settingManager.GetSettingValueAsync(AppSettingNames.AllowProbationToWorkRemote).Returns(Task.FromResult(allowProbationRemote));
            settingManager.GetSettingValueAsync(AppSettingNames.TimesCanLateAndEarlyInMonth).Returns(Task.FromResult("2"));
            settingManager.GetSettingValueAsync(AppSettingNames.TimesCanLateAndEarlyInWeek).Returns(Task.FromResult("1"));
            settingManager.GetSettingValueForApplicationAsync(AppSettingNames.TotalTimeAbsenceTime).Returns(Task.FromResult("2"));
            settingManager.GetSettingValueForApplicationAsync(AppSettingNames.SendKomuRequest).Returns(Task.FromResult("false"));

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
                UnitOfWorkManager = Resolve<IUnitOfWorkManager>()
            };

            return (service, settingManager);
        }

        private long CreateTestUser(string prefix, WorkMode workMode, Usertype type = Usertype.Staff)
        {
            long userId = 0;
            UsingDbContext(ctx =>
            {
                var branch = ctx.Branchs.FirstOrDefault() ?? new Branch { Name = "UT_Branch", DisplayName = "UT_Branch", Code = "UTB" };
                if (branch.Id == 0)
                {
                    ctx.Branchs.Add(branch);
                    ctx.SaveChanges();
                }

                var user = new User
                {
                    UserName = prefix,
                    NormalizedUserName = prefix.ToUpperInvariant(),
                    Name = prefix,
                    Surname = "Tester",
                    EmailAddress = $"{prefix}@ncc.asia",
                    NormalizedEmailAddress = $"{prefix.ToUpperInvariant()}@NCC.ASIA",
                    Password = "pwd",
                    IsActive = true,
                    BranchId = branch.Id,
                    WorkMode = workMode,
                    Type = type,
                    MorningWorking = 3.5,
                    AfternoonWorking = 4.5
                };
                ctx.Users.Add(user);
                ctx.SaveChanges();
                userId = user.Id;
            });
            return userId;
        }

        /// <summary>
        /// Returns a future weekday (Monday) at least minDaysAhead days from today so it never falls on weekend.
        /// </summary>
        private static DateTime GetFutureMonday(int minDaysAhead)
        {
            var d = DateTime.Now.Date.AddDays(minDaysAhead);
            while (d.DayOfWeek != DayOfWeek.Monday)
            {
                d = d.AddDays(1);
            }
            return d;
        }

        [Fact]
        public async Task ProcessSubmitToPendingNew_Should_Require_NonWhitespace_Reason_For_All_Types()
        {
            var officeUserId = CreateTestUser("ut_reason_office", WorkMode.Office);
            var (service, _) = CreateService(officeUserId);
            var targetDate = GetFutureMonday(10);

            await WithUnitOfWorkAsync(async () =>
            {
                var ex = await Assert.ThrowsAsync<UserFriendlyException>(() => service.SubmitToPendingNew(new MyRequestDto
                {
                    Type = RequestType.Remote,
                    Reason = "   ",
                    Absences = new List<AbsenceDayDetailDto>
                    {
                        new AbsenceDayDetailDto { DateAt = targetDate, DateType = DayType.Fullday, Hour = 8 }
                    }
                }));
                ex.Message.ShouldContain("Reason is require!");
            });
        }

        [Fact]
        public async Task ProcessSubmitToPendingNew_Should_Block_Cross_WorkMode_Requests()
        {
            var officeUserId = CreateTestUser("ut_cross_office", WorkMode.Office);
            var wfhUserId = CreateTestUser("ut_cross_wfh", WorkMode.WFH);
            var targetDate = GetFutureMonday(10);

            var (officeService, _) = CreateService(officeUserId);
            await WithUnitOfWorkAsync(async () =>
            {
                var ex = await Assert.ThrowsAsync<UserFriendlyException>(() => officeService.SubmitToPendingNew(new MyRequestDto
                {
                    Type = RequestType.WorkAtOffice,
                    Reason = "Test",
                    Absences = new List<AbsenceDayDetailDto>
                    {
                        new AbsenceDayDetailDto { DateAt = targetDate, DateType = DayType.Fullday, Hour = 8 }
                    }
                }));
                ex.Message.ShouldContain("Office users cannot submit Work at Office requests");
            });

            var (wfhService, _) = CreateService(wfhUserId);
            await WithUnitOfWorkAsync(async () =>
            {
                var ex = await Assert.ThrowsAsync<UserFriendlyException>(() => wfhService.SubmitToPendingNew(new MyRequestDto
                {
                    Type = RequestType.Remote,
                    Reason = "Test",
                    Absences = new List<AbsenceDayDetailDto>
                    {
                        new AbsenceDayDetailDto { DateAt = targetDate, DateType = DayType.Fullday, Hour = 8 }
                    }
                }));
                ex.Message.ShouldContain("WFH users cannot submit Remote requests");
            });
        }

        [Fact]
        public async Task ProcessSubmitToPendingNew_Should_Enforce_HalfDay_Requires_Opposite_Off_Request()
        {
            var wfhUserId = CreateTestUser("ut_halfday_wfh", WorkMode.WFH);
            var (service, _) = CreateService(wfhUserId, wfhDaysInAdvance: "0");
            var targetDate = GetFutureMonday(10);

            // Morning WorkAtOffice without Off Afternoon -> throws
            await WithUnitOfWorkAsync(async () =>
            {
                var ex = await Assert.ThrowsAsync<UserFriendlyException>(() => service.SubmitToPendingNew(new MyRequestDto
                {
                    Type = RequestType.WorkAtOffice,
                    Reason = "Half day office",
                    Absences = new List<AbsenceDayDetailDto>
                    {
                        new AbsenceDayDetailDto { DateAt = targetDate, DateType = DayType.Morning, Hour = 3.5 }
                    }
                }));
                ex.Message.ShouldContain("only allowed if you have an Off request in the afternoon");
            });

            // Seed Off Afternoon on targetDate -> Morning WorkAtOffice should now succeed
            UsingDbContext(ctx =>
            {
                var offReq = new AbsenceDayRequest { UserId = wfhUserId, Type = RequestType.Off, Status = RequestStatus.Approved, Reason = "Off PM" };
                ctx.AbsenceDayRequests.Add(offReq);
                ctx.SaveChanges();
                ctx.AbsenceDayDetails.Add(new AbsenceDayDetail { RequestId = offReq.Id, DateAt = targetDate, DateType = DayType.Afternoon, Hour = 4.5 });
                ctx.SaveChanges();
            });

            await WithUnitOfWorkAsync(async () =>
            {
                var result = await service.SubmitToPendingNew(new MyRequestDto
                {
                    Type = RequestType.WorkAtOffice,
                    Reason = "Half day office with Off PM",
                    Absences = new List<AbsenceDayDetailDto>
                    {
                        new AbsenceDayDetailDto { DateAt = targetDate, DateType = DayType.Morning, Hour = 3.5 }
                    }
                });
                result.Absences[0].ErrorMessage.ShouldBeNullOrEmpty();
            });
        }

        [Fact]
        public async Task ApplyRemotePolicy_Should_AutoReject_When_Insufficient_Advance_Notice_Or_Exceeding_Weekly_Quota()
        {
            var officeUserId = CreateTestUser("ut_remote_policy", WorkMode.Office);
            var (service, _) = CreateService(officeUserId, wfhSetting: "1", officeDaysInAdvance: "7");

            // 1. Submitted only 2 days in advance (< 7) -> Auto-Reject
            var nearDate = DateTime.Now.Date.AddDays(2);
            if (nearDate.DayOfWeek == DayOfWeek.Saturday) nearDate = nearDate.AddDays(2);
            if (nearDate.DayOfWeek == DayOfWeek.Sunday) nearDate = nearDate.AddDays(1);

            await WithUnitOfWorkAsync(async () =>
            {
                var res = await service.SubmitToPendingNew(new MyRequestDto
                {
                    Type = RequestType.Remote,
                    Reason = "Urgent remote",
                    Absences = new List<AbsenceDayDetailDto>
                    {
                        new AbsenceDayDetailDto { DateAt = nearDate, DateType = DayType.Fullday, Hour = 8 }
                    }
                });
                res.Absences[0].ErrorMessage.ShouldContain("at least 7 day(s) in advance");
            });

            // 2. Submit 2 days in same future week when quota = 1 -> Day 1 Pending, Day 2 Auto-Rejected
            var mon = GetFutureMonday(14);
            var tue = mon.AddDays(1);

            await WithUnitOfWorkAsync(async () =>
            {
                var res = await service.SubmitToPendingNew(new MyRequestDto
                {
                    Type = RequestType.Remote,
                    Reason = "Batch remote",
                    Absences = new List<AbsenceDayDetailDto>
                    {
                        new AbsenceDayDetailDto { DateAt = mon, DateType = DayType.Fullday, Hour = 8 },
                        new AbsenceDayDetailDto { DateAt = tue, DateType = DayType.Fullday, Hour = 8 }
                    }
                });

                res.Absences[0].ErrorMessage.ShouldBeNullOrEmpty();
                res.Absences[1].ErrorMessage.ShouldContain("exceeded the maximum allowed remote days (1 days)");
            });

            UsingDbContext(ctx =>
            {
                var monReq = ctx.AbsenceDayDetails.Where(d => d.Request.UserId == officeUserId && d.DateAt == mon).Select(d => d.Request.Status).First();
                var tueReq = ctx.AbsenceDayDetails.Where(d => d.Request.UserId == officeUserId && d.DateAt == tue).Select(d => d.Request.Status).First();
                monReq.ShouldBe(RequestStatus.Pending);
                tueReq.ShouldBe(RequestStatus.Rejected);
            });
        }

        [Fact]
        public async Task ApplyWorkAtOfficePolicy_Should_AutoReject_AdvanceNotice_And_WeeklyQuota()
        {
            var wfhUserId = CreateTestUser("ut_wao_policy", WorkMode.WFH);
            var (service, _) = CreateService(wfhUserId, wfhMaxOffice: "1", wfhDaysInAdvance: "7");

            var mon = GetFutureMonday(14);
            var wed = mon.AddDays(2);

            await WithUnitOfWorkAsync(async () =>
            {
                var res = await service.SubmitToPendingNew(new MyRequestDto
                {
                    Type = RequestType.WorkAtOffice,
                    Reason = "Work at office",
                    Absences = new List<AbsenceDayDetailDto>
                    {
                        new AbsenceDayDetailDto { DateAt = mon, DateType = DayType.Fullday, Hour = 8 },
                        new AbsenceDayDetailDto { DateAt = wed, DateType = DayType.Fullday, Hour = 8 }
                    }
                });

                res.Absences[0].ErrorMessage.ShouldBeNullOrEmpty();
                res.Absences[1].ErrorMessage.ShouldContain("exceeded the maximum allowed Work at Office days (1 days)");
            });
        }

        [Fact]
        public async Task CancelRequest_Should_Block_Canceling_Off_When_Active_HalfDay_WorkAtOffice_Exists()
        {
            var wfhUserId = CreateTestUser("ut_cancel_guard", WorkMode.WFH);
            var targetDate = GetFutureMonday(10);
            long offRequestId = 0;
            long waoRequestId = 0;

            UsingDbContext(ctx =>
            {
                var offReq = new AbsenceDayRequest { UserId = wfhUserId, Type = RequestType.Off, Status = RequestStatus.Pending, Reason = "Off AM" };
                var waoReq = new AbsenceDayRequest { UserId = wfhUserId, Type = RequestType.WorkAtOffice, Status = RequestStatus.Pending, Reason = "Office PM" };
                ctx.AbsenceDayRequests.AddRange(offReq, waoReq);
                ctx.SaveChanges();
                offRequestId = offReq.Id;
                waoRequestId = waoReq.Id;

                ctx.AbsenceDayDetails.AddRange(
                    new AbsenceDayDetail { RequestId = offRequestId, DateAt = targetDate, DateType = DayType.Morning, Hour = 3.5 },
                    new AbsenceDayDetail { RequestId = waoRequestId, DateAt = targetDate, DateType = DayType.Afternoon, Hour = 4.5 }
                );
                ctx.SaveChanges();
            });

            var (service, _) = CreateService(wfhUserId);

            // Trying to cancel Off while half-day WorkAtOffice is active -> throws
            await WithUnitOfWorkAsync(async () =>
            {
                var ex = await Assert.ThrowsAsync<UserFriendlyException>(() => service.CancelRequest(new EntityDto<long>(offRequestId)));
                ex.Message.ShouldContain("Please cancel the Remote/Work at Office request first");
            });

            // Cancel WorkAtOffice first -> then canceling Off succeeds in subsequent request
            await WithUnitOfWorkAsync(async () =>
            {
                await service.CancelRequest(new EntityDto<long>(waoRequestId));
            });

            await WithUnitOfWorkAsync(async () =>
            {
                await service.CancelRequest(new EntityDto<long>(offRequestId));
            });

            UsingDbContext(ctx =>
            {
                ctx.AbsenceDayRequests.Any(r => r.Id == offRequestId && !r.IsDeleted).ShouldBeFalse();
            });
        }

        [Fact]
        public async Task SubmitToPendingNew_Should_Block_Intern_And_Probation_Remote_When_Disallowed()
        {
            var internId = CreateTestUser("ut_intern_rem", WorkMode.Office, Usertype.Internship);
            var probationId = CreateTestUser("ut_prob_rem", WorkMode.Office, Usertype.ProbationaryStaff);
            var targetDate = GetFutureMonday(10);

            var (internService, _) = CreateService(internId, allowInternRemote: "false");
            await WithUnitOfWorkAsync(async () =>
            {
                var ex = await Assert.ThrowsAsync<UserFriendlyException>(() => internService.SubmitToPendingNew(new MyRequestDto
                {
                    Type = RequestType.Remote,
                    Reason = "Intern remote",
                    Absences = new List<AbsenceDayDetailDto>
                    {
                        new AbsenceDayDetailDto { DateAt = targetDate, DateType = DayType.Fullday, Hour = 8 }
                    }
                }));
                ex.Message.ShouldContain("Intern is not allowed to work REMOTE");
            });

            var (probService, _) = CreateService(probationId, allowProbationRemote: "false");
            await WithUnitOfWorkAsync(async () =>
            {
                var ex = await Assert.ThrowsAsync<UserFriendlyException>(() => probService.SubmitToPendingNew(new MyRequestDto
                {
                    Type = RequestType.Remote,
                    Reason = "Probation remote",
                    Absences = new List<AbsenceDayDetailDto>
                    {
                        new AbsenceDayDetailDto { DateAt = targetDate, DateType = DayType.Fullday, Hour = 8 }
                    }
                }));
                ex.Message.ShouldContain("Probation is not allowed to work REMOTE");
            });
        }

        [Fact]
        public async Task ApplyRemotePolicy_Should_Respect_FullyRemote_Whitelist_And_RemoteBlacklist_Penalty()
        {
            var whitelistUserId = CreateTestUser("ut_wl_remote", WorkMode.Office);
            var penaltyUserId = CreateTestUser("ut_pen_remote", WorkMode.Office);
            var mon = GetFutureMonday(14);

            UsingDbContext(ctx =>
            {
                var wl = new WhitelistSystem { Name = "UT_FullyRemote", Type = WhitelistType.FullyRemote, IsActive = true };
                ctx.WhitelistSystems.Add(wl);
                ctx.SaveChanges();
                ctx.UserWhitelists.Add(new UserWhitelist { UserId = whitelistUserId, WhitelistSystemId = wl.Id });
                ctx.RemoteBlacklists.Add(new RemoteBlacklist { UserId = penaltyUserId, PenaltyDays = 1 });
                ctx.SaveChanges();
            });

            // Whitelist FullyRemote (5 days/week) with setting=1 -> 2 days in same week both succeed as Pending
            var (wlService, _) = CreateService(whitelistUserId, wfhSetting: "1", officeDaysInAdvance: "0");
            await WithUnitOfWorkAsync(async () =>
            {
                var res = await wlService.SubmitToPendingNew(new MyRequestDto
                {
                    Type = RequestType.Remote,
                    Reason = "Fully remote whitelist",
                    Absences = new List<AbsenceDayDetailDto>
                    {
                        new AbsenceDayDetailDto { DateAt = mon, DateType = DayType.Fullday, Hour = 8 },
                        new AbsenceDayDetailDto { DateAt = mon.AddDays(1), DateType = DayType.Fullday, Hour = 8 }
                    }
                });
                res.Absences[0].ErrorMessage.ShouldBeNullOrEmpty();
                res.Absences[1].ErrorMessage.ShouldBeNullOrEmpty();
            });

            // PenaltyDays = 1 with setting=1 -> MAX_ALLOW_REMOTE_DAY = 0 -> Auto-Reject with penalty message
            var (penService, _) = CreateService(penaltyUserId, wfhSetting: "1", officeDaysInAdvance: "0");
            await WithUnitOfWorkAsync(async () =>
            {
                var res = await penService.SubmitToPendingNew(new MyRequestDto
                {
                    Type = RequestType.Remote,
                    Reason = "Penalized user",
                    Absences = new List<AbsenceDayDetailDto>
                    {
                        new AbsenceDayDetailDto { DateAt = mon, DateType = DayType.Fullday, Hour = 8 }
                    }
                });
                res.Absences[0].ErrorMessage.ShouldContain("reduced to 0 due to a penalty of 1 day(s)");
            });
        }

        [Fact]
        public async Task ApplyRemotePolicy_Should_AutoReject_When_Insufficient_Office_Days_Last_Week()
        {
            var userId = CreateTestUser("ut_low_office_prev", WorkMode.Office);
            var targetMon = GetFutureMonday(14);
            var prevMon = targetMon.AddDays(-7);

            // wfhSetting = 1, last week has 1 holiday (DayOffSetting) -> threshold = Max(0, 1 - 1) = 0
            // User also took 1 Off day last week (1 > 0) -> insufficient office working days
            UsingDbContext(ctx =>
            {
                ctx.DayOffSettings.Add(new DayOffSetting { DayOff = prevMon.AddDays(1), Name = "UT Holiday" });
                var prevOff = new AbsenceDayRequest { UserId = userId, Type = RequestType.Off, Status = RequestStatus.Approved, Reason = "Prev off" };
                ctx.AbsenceDayRequests.Add(prevOff);
                ctx.SaveChanges();
                ctx.AbsenceDayDetails.Add(new AbsenceDayDetail { RequestId = prevOff.Id, DateAt = prevMon, DateType = DayType.Fullday, Hour = 8 });
                ctx.SaveChanges();
            });

            var (service, _) = CreateService(userId, wfhSetting: "1", officeDaysInAdvance: "0");
            await WithUnitOfWorkAsync(async () =>
            {
                var res = await service.SubmitToPendingNew(new MyRequestDto
                {
                    Type = RequestType.Remote,
                    Reason = "Remote after off week",
                    Absences = new List<AbsenceDayDetailDto>
                    {
                        new AbsenceDayDetailDto { DateAt = targetMon, DateType = DayType.Fullday, Hour = 8 }
                    }
                });
                res.Absences[0].ErrorMessage.ShouldContain("insufficient office working days in the previous week");
            });
        }

        [Fact]
        public async Task ApplyRemotePolicy_Should_Throw_When_Custom_Off_Exists_On_Same_Day()
        {
            var userId = CreateTestUser("ut_custom_conflict", WorkMode.Office);
            var targetDate = GetFutureMonday(10);

            UsingDbContext(ctx =>
            {
                var customReq = new AbsenceDayRequest { UserId = userId, Type = RequestType.Off, Status = RequestStatus.Pending, Reason = "Late" };
                ctx.AbsenceDayRequests.Add(customReq);
                ctx.SaveChanges();
                ctx.AbsenceDayDetails.Add(new AbsenceDayDetail
                {
                    RequestId = customReq.Id,
                    DateAt = targetDate,
                    DateType = DayType.Custom,
                    AbsenceTime = OnDayType.DiMuon,
                    Hour = 1
                });
                ctx.SaveChanges();
            });

            var (service, _) = CreateService(userId, officeDaysInAdvance: "0");
            await WithUnitOfWorkAsync(async () =>
            {
                var ex = await Assert.ThrowsAsync<UserFriendlyException>(() => service.SubmitToPendingNew(new MyRequestDto
                {
                    Type = RequestType.Remote,
                    Reason = "Remote on late day",
                    Absences = new List<AbsenceDayDetailDto>
                    {
                        new AbsenceDayDetailDto { DateAt = targetDate, DateType = DayType.Fullday, Hour = 8 }
                    }
                }));
                ex.Message.ShouldContain("existing Tardiness/Early request");
            });
        }

        [Fact]
        public async Task SubmitToPendingNew_Should_Block_Custom_DayType_For_Remote_And_WorkAtOffice()
        {
            var wfhUserId = CreateTestUser("ut_custom_wao", WorkMode.WFH);
            var targetDate = GetFutureMonday(10);
            var (service, _) = CreateService(wfhUserId, wfhDaysInAdvance: "0");

            await WithUnitOfWorkAsync(async () =>
            {
                var ex = await Assert.ThrowsAsync<UserFriendlyException>(() => service.SubmitToPendingNew(new MyRequestDto
                {
                    Type = RequestType.WorkAtOffice,
                    Reason = "Invalid custom",
                    Absences = new List<AbsenceDayDetailDto>
                    {
                        new AbsenceDayDetailDto { DateAt = targetDate, DateType = DayType.Custom, Hour = 1 }
                    }
                }));
                ex.Message.ShouldContain("only allowed for Full day, or combined with an Off request");
            });
        }

        [Fact]
        public async Task CancelRequest_Should_Allow_Canceling_Off_When_HalfDay_Request_Is_Rejected()
        {
            var wfhUserId = CreateTestUser("ut_cancel_rejected_ok", WorkMode.WFH);
            var targetDate = GetFutureMonday(10);
            long offRequestId = 0;

            UsingDbContext(ctx =>
            {
                var offReq = new AbsenceDayRequest { UserId = wfhUserId, Type = RequestType.Off, Status = RequestStatus.Pending, Reason = "Off AM" };
                var rejWaoReq = new AbsenceDayRequest { UserId = wfhUserId, Type = RequestType.WorkAtOffice, Status = RequestStatus.Rejected, Reason = "Rejected WAO PM" };
                ctx.AbsenceDayRequests.AddRange(offReq, rejWaoReq);
                ctx.SaveChanges();
                offRequestId = offReq.Id;

                ctx.AbsenceDayDetails.AddRange(
                    new AbsenceDayDetail { RequestId = offRequestId, DateAt = targetDate, DateType = DayType.Morning, Hour = 3.5 },
                    new AbsenceDayDetail { RequestId = rejWaoReq.Id, DateAt = targetDate, DateType = DayType.Afternoon, Hour = 4.5 }
                );
                ctx.SaveChanges();
            });

            var (service, _) = CreateService(wfhUserId);
            await WithUnitOfWorkAsync(async () =>
            {
                await service.CancelRequest(new EntityDto<long>(offRequestId));
            });

            UsingDbContext(ctx =>
            {
                ctx.AbsenceDayRequests.Any(r => r.Id == offRequestId && !r.IsDeleted).ShouldBeFalse();
            });
        }

        [Fact]
        public async Task SubmitToPendingNew_Should_Block_Off_With_Custom_DayType()
        {
            var userId = CreateTestUser("ut_off_custom_block", WorkMode.Office);
            var (service, _) = CreateService(userId);
            var targetDate = GetFutureMonday(10);

            await WithUnitOfWorkAsync(async () =>
            {
                var ex = await Assert.ThrowsAsync<UserFriendlyException>(() => service.SubmitToPendingNew(new MyRequestDto
                {
                    Type = RequestType.Off,
                    Reason = "Late",
                    Absences = new List<AbsenceDayDetailDto>
                    {
                        new AbsenceDayDetailDto { DateAt = targetDate, DateType = DayType.Custom, AbsenceTime = OnDayType.DiMuon, Hour = 1 }
                    }
                }));
                ex.Message.ShouldContain("Cannot send tardiness/early requests!");
            });
        }

        [Fact]
        public async Task SubmitToPendingNew_Should_Validate_Empty_Absences_And_Conflicting_Combination()
        {
            var userId = CreateTestUser("ut_val_conflict", WorkMode.Office);
            var (service, _) = CreateService(userId, officeDaysInAdvance: "0");
            var targetDate = GetFutureMonday(10);

            // Empty Absences -> throws
            await WithUnitOfWorkAsync(async () =>
            {
                await Assert.ThrowsAsync<UserFriendlyException>(() => service.SubmitToPendingNew(new MyRequestDto
                {
                    Type = RequestType.Remote,
                    Reason = "Empty",
                    Absences = new List<AbsenceDayDetailDto>()
                }));
            });

            // Existing Off Fullday on targetDate -> submitting Remote Fullday on same date throws
            UsingDbContext(ctx =>
            {
                var offReq = new AbsenceDayRequest { UserId = userId, Type = RequestType.Off, Status = RequestStatus.Approved, Reason = "Off full" };
                ctx.AbsenceDayRequests.Add(offReq);
                ctx.SaveChanges();
                ctx.AbsenceDayDetails.Add(new AbsenceDayDetail { RequestId = offReq.Id, DateAt = targetDate, DateType = DayType.Fullday, Hour = 8 });
                ctx.SaveChanges();
            });

            await WithUnitOfWorkAsync(async () =>
            {
                await Assert.ThrowsAsync<UserFriendlyException>(() => service.SubmitToPendingNew(new MyRequestDto
                {
                    Type = RequestType.Remote,
                    Reason = "Conflict with Off full",
                    Absences = new List<AbsenceDayDetailDto>
                    {
                        new AbsenceDayDetailDto { DateAt = targetDate, DateType = DayType.Fullday, Hour = 8 }
                    }
                }));
            });
        }

        [Fact]
        public async Task SubmitToPendingNew_Should_Validate_OffType_For_NonStaff_And_MaxLength()
        {
            var internId = CreateTestUser("ut_offtype_intern", WorkMode.Office, Usertype.Internship);
            var staffId = CreateTestUser("ut_offtype_staff", WorkMode.Office, Usertype.Staff);
            var mon = GetFutureMonday(10);
            long coPhepTypeId = 0;

            UsingDbContext(ctx =>
            {
                var dot = new DayOffType { Name = "UT_CoPhep_1Day", Length = 1, Status = OffTypeStatus.CoPhep };
                ctx.DayOffTypes.Add(dot);
                ctx.SaveChanges();
                coPhepTypeId = dot.Id;
            });

            // Non-staff (Intern) requesting CoPhep -> throws
            var (internService, _) = CreateService(internId);
            await WithUnitOfWorkAsync(async () =>
            {
                var ex = await Assert.ThrowsAsync<UserFriendlyException>(() => internService.SubmitToPendingNew(new MyRequestDto
                {
                    Type = RequestType.Off,
                    DayOffTypeId = coPhepTypeId,
                    Reason = "Intern paid off",
                    Absences = new List<AbsenceDayDetailDto>
                    {
                        new AbsenceDayDetailDto { DateAt = mon, DateType = DayType.Fullday, Hour = 8 }
                    }
                }));
                ex.Message.ShouldContain("allow for STAFF Only");
            });

            // Staff requesting 2 full days when max length is 1 -> throws
            var (staffService, _) = CreateService(staffId);
            await WithUnitOfWorkAsync(async () =>
            {
                var ex = await Assert.ThrowsAsync<UserFriendlyException>(() => staffService.SubmitToPendingNew(new MyRequestDto
                {
                    Type = RequestType.Off,
                    DayOffTypeId = coPhepTypeId,
                    Reason = "Exceed off length",
                    Absences = new List<AbsenceDayDetailDto>
                    {
                        new AbsenceDayDetailDto { DateAt = mon, DateType = DayType.Fullday, Hour = 8 },
                        new AbsenceDayDetailDto { DateAt = mon.AddDays(1), DateType = DayType.Fullday, Hour = 8 }
                    }
                }));
                ex.Message.ShouldContain("quá số ngày phép cho phép");
            });
        }

        [Fact]
        public async Task SubmitToPendingNew_Should_Block_When_Date_Is_Holiday_DayOffSetting()
        {
            var userId = CreateTestUser("ut_holiday_block", WorkMode.Office);
            var holiday = GetFutureMonday(12);

            UsingDbContext(ctx =>
            {
                ctx.DayOffSettings.Add(new DayOffSetting { DayOff = holiday, Name = "UT National Holiday" });
                ctx.SaveChanges();
            });

            var (service, _) = CreateService(userId, officeDaysInAdvance: "0");
            await WithUnitOfWorkAsync(async () =>
            {
                var ex = await Assert.ThrowsAsync<UserFriendlyException>(() => service.SubmitToPendingNew(new MyRequestDto
                {
                    Type = RequestType.Remote,
                    Reason = "Holiday remote",
                    Absences = new List<AbsenceDayDetailDto>
                    {
                        new AbsenceDayDetailDto { DateAt = holiday, DateType = DayType.Fullday, Hour = 8 }
                    }
                }));
                ex.Message.ShouldContain("is not a working day");
            });
        }

        [Fact]
        public async Task SubmitToPendingNew_Should_AutoCalculate_Hour_For_Fullday_Morning_And_Afternoon()
        {
            var userId = CreateTestUser("ut_auto_hour", WorkMode.Office);
            var mon = GetFutureMonday(14);
            long khongPhepTypeId = 0;

            UsingDbContext(ctx =>
            {
                var dot = new DayOffType { Name = "UT_KhongPhep", Length = 5, Status = OffTypeStatus.KhongPhep };
                ctx.DayOffTypes.Add(dot);
                ctx.SaveChanges();
                khongPhepTypeId = dot.Id;
            });

            var (service, _) = CreateService(userId);
            await WithUnitOfWorkAsync(async () =>
            {
                await service.SubmitToPendingNew(new MyRequestDto
                {
                    Type = RequestType.Off,
                    DayOffTypeId = khongPhepTypeId,
                    Reason = "Auto hour calc",
                    Absences = new List<AbsenceDayDetailDto>
                    {
                        new AbsenceDayDetailDto { DateAt = mon, DateType = DayType.Fullday, Hour = 0 },
                        new AbsenceDayDetailDto { DateAt = mon.AddDays(1), DateType = DayType.Morning, Hour = 0 },
                        new AbsenceDayDetailDto { DateAt = mon.AddDays(2), DateType = DayType.Afternoon, Hour = 0 }
                    }
                });
            });

            UsingDbContext(ctx =>
            {
                var details = ctx.AbsenceDayDetails.Where(d => d.Request.UserId == userId).ToList();
                details.First(d => d.DateAt == mon).Hour.ShouldBe(8);
                details.First(d => d.DateAt == mon.AddDays(1)).Hour.ShouldBe(3.5);
                details.First(d => d.DateAt == mon.AddDays(2)).Hour.ShouldBe(4.5);
            });
        }

        [Fact]
        public async Task SubmitToPendingNew_Should_Delete_Existing_Rejected_Request_On_Same_Date()
        {
            var userId = CreateTestUser("ut_del_rejected", WorkMode.Office);
            var targetDate = GetFutureMonday(14);
            long oldRejectedReqId = 0;

            UsingDbContext(ctx =>
            {
                var oldReq = new AbsenceDayRequest { UserId = userId, Type = RequestType.Remote, Status = RequestStatus.Rejected, Reason = "Old rejected" };
                ctx.AbsenceDayRequests.Add(oldReq);
                ctx.SaveChanges();
                oldRejectedReqId = oldReq.Id;
                ctx.AbsenceDayDetails.Add(new AbsenceDayDetail { RequestId = oldRejectedReqId, DateAt = targetDate, DateType = DayType.Fullday, Hour = 8 });
                ctx.SaveChanges();
            });

            var (service, _) = CreateService(userId, wfhSetting: "1", officeDaysInAdvance: "0");
            await WithUnitOfWorkAsync(async () =>
            {
                await service.SubmitToPendingNew(new MyRequestDto
                {
                    Type = RequestType.Remote,
                    Reason = "Resubmit remote",
                    Absences = new List<AbsenceDayDetailDto>
                    {
                        new AbsenceDayDetailDto { DateAt = targetDate, DateType = DayType.Fullday, Hour = 8 }
                    }
                });
            });

            UsingDbContext(ctx =>
            {
                ctx.AbsenceDayRequests.Any(r => r.Id == oldRejectedReqId && !r.IsDeleted).ShouldBeFalse();
                ctx.AbsenceDayRequests.Any(r => r.UserId == userId && r.Reason == "Resubmit remote" && r.Status == RequestStatus.Pending && !r.IsDeleted).ShouldBeTrue();
            });
        }

        [Fact]
        public async Task SubmitToPendingNew_Should_Block_Off_When_Custom_Off_Exists_On_Same_Day()
        {
            var userId = CreateTestUser("ut_off_vs_custom", WorkMode.Office);
            var targetDate = GetFutureMonday(10);
            long khongPhepTypeId = 0;

            UsingDbContext(ctx =>
            {
                var dot = new DayOffType { Name = "UT_KhongPhep2", Length = 5, Status = OffTypeStatus.KhongPhep };
                ctx.DayOffTypes.Add(dot);
                ctx.SaveChanges();
                khongPhepTypeId = dot.Id;

                var customReq = new AbsenceDayRequest { UserId = userId, Type = RequestType.Off, Status = RequestStatus.Pending, Reason = "Late", DayOffTypeId = khongPhepTypeId };
                ctx.AbsenceDayRequests.Add(customReq);
                ctx.SaveChanges();
                ctx.AbsenceDayDetails.Add(new AbsenceDayDetail
                {
                    RequestId = customReq.Id,
                    DateAt = targetDate,
                    DateType = DayType.Custom,
                    AbsenceTime = OnDayType.DiMuon,
                    Hour = 1
                });
                ctx.SaveChanges();
            });

            var (service, _) = CreateService(userId);
            await WithUnitOfWorkAsync(async () =>
            {
                var ex = await Assert.ThrowsAsync<UserFriendlyException>(() => service.SubmitToPendingNew(new MyRequestDto
                {
                    Type = RequestType.Off,
                    DayOffTypeId = khongPhepTypeId,
                    Reason = "Off morning on late day",
                    Absences = new List<AbsenceDayDetailDto>
                    {
                        new AbsenceDayDetailDto { DateAt = targetDate, DateType = DayType.Morning, Hour = 3.5 }
                    }
                }));
                ex.Message.ShouldContain("Cannot submit Off request");
                ex.Message.ShouldContain("existing Tardiness/Early request");
            });
        }

        [Fact]
        public async Task ApplyWorkAtOfficePolicy_Should_AutoReject_When_Insufficient_Advance_Notice()
        {
            var wfhUserId = CreateTestUser("ut_wao_advance", WorkMode.WFH);
            var (service, _) = CreateService(wfhUserId, wfhMaxOffice: "2", wfhDaysInAdvance: "7");

            var nearDate = DateTime.Now.Date.AddDays(2);
            if (nearDate.DayOfWeek == DayOfWeek.Saturday) nearDate = nearDate.AddDays(2);
            if (nearDate.DayOfWeek == DayOfWeek.Sunday) nearDate = nearDate.AddDays(1);

            await WithUnitOfWorkAsync(async () =>
            {
                var res = await service.SubmitToPendingNew(new MyRequestDto
                {
                    Type = RequestType.WorkAtOffice,
                    Reason = "Urgent office day",
                    Absences = new List<AbsenceDayDetailDto>
                    {
                        new AbsenceDayDetailDto { DateAt = nearDate, DateType = DayType.Fullday, Hour = 8 }
                    }
                });
                res.Absences[0].ErrorMessage.ShouldContain("at least 7 day(s) in advance");
                res.Absences[0].Status.ShouldBe(RequestStatus.Rejected);
            });
        }

        [Fact]
        public async Task SubmitToPendingNew_Should_Allow_Onsite_Request()
        {
            var userId = CreateTestUser("ut_onsite_ok", WorkMode.Office);
            var targetDate = GetFutureMonday(10);
            var (service, _) = CreateService(userId);

            await WithUnitOfWorkAsync(async () =>
            {
                var res = await service.SubmitToPendingNew(new MyRequestDto
                {
                    Type = RequestType.Onsite,
                    Reason = "Client site",
                    Absences = new List<AbsenceDayDetailDto>
                    {
                        new AbsenceDayDetailDto { DateAt = targetDate, DateType = DayType.Fullday, Hour = 8 }
                    }
                });
                res.Absences[0].Status.ShouldBe(RequestStatus.Pending);
                res.Absences[0].ErrorMessage.ShouldBeNullOrEmpty();
            });
        }
    }
}
