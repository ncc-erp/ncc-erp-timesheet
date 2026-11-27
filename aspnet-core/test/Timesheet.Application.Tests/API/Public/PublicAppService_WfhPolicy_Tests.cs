using Abp.Configuration;
using Microsoft.AspNetCore.Http;
using Ncc.Authorization.Users;
using Ncc.IoC;
using NSubstitute;
using Shouldly;
using System;
using System.Linq;
using System.Threading.Tasks;
using Timesheet.APIs.OverTimeHours;
using Timesheet.APIs.Public;
using Timesheet.Entities;
using Xunit;
using static Ncc.Entities.Enum.StatusEnum;

namespace Timesheet.Application.Tests.API.Public
{
    public class PublicAppService_WfhPolicy_Tests : TimesheetApplicationTestBase
    {
        private readonly PublicAppService _publicAppService;
        private readonly IWorkScope _workScope;

        public PublicAppService_WfhPolicy_Tests()
        {
            _workScope = Resolve<IWorkScope>();
            var overTimeHourAppService = Substitute.For<OverTimeHourAppService>(_workScope);
            var httpContextAccessor = Resolve<IHttpContextAccessor>();

            _publicAppService = new PublicAppService(overTimeHourAppService, httpContextAccessor, _workScope)
            {
                SettingManager = Resolve<ISettingManager>()
            };
        }

        private long SeedUser(string prefix, WorkMode workMode, bool isActive = true, double morning = 3.5, double afternoon = 4.5)
        {
            long id = 0;
            UsingDbContext(ctx =>
            {
                var u = new User
                {
                    UserName = prefix,
                    NormalizedUserName = prefix.ToUpperInvariant(),
                    Name = prefix,
                    Surname = "PublicUT",
                    EmailAddress = $"{prefix}@ncc.asia",
                    NormalizedEmailAddress = $"{prefix.ToUpperInvariant()}@NCC.ASIA",
                    Password = "pwd",
                    IsActive = isActive,
                    WorkMode = workMode,
                    MorningWorking = morning,
                    AfternoonWorking = afternoon
                };
                ctx.Users.Add(u);
                ctx.SaveChanges();
                id = u.Id;
            });
            return id;
        }

        private void SeedRequest(long userId, DateTime date, RequestType type, DayType dayType, RequestStatus status = RequestStatus.Approved, double hour = 8)
        {
            UsingDbContext(ctx =>
            {
                var req = new AbsenceDayRequest
                {
                    UserId = userId,
                    Type = type,
                    Status = status,
                    Reason = "UT"
                };
                ctx.AbsenceDayRequests.Add(req);
                ctx.SaveChanges();

                ctx.AbsenceDayDetails.Add(new AbsenceDayDetail
                {
                    RequestId = req.Id,
                    DateAt = date,
                    DateType = dayType,
                    Hour = hour
                });
                ctx.SaveChanges();
            });
        }

        [Fact]
        public async Task GetUserWorkFromHome_Should_Handle_All_Office_And_WFH_Combinations()
        {
            var date = new DateTime(2026, 11, 25);

            var officeRemoteApproved = SeedUser("pub_off_rem_ok", WorkMode.Office);
            var officeRemoteRejected = SeedUser("pub_off_rem_rej", WorkMode.Office);
            var wfhPlain = SeedUser("pub_wfh_plain", WorkMode.WFH);
            var wfhOffMorning = SeedUser("pub_wfh_off_am", WorkMode.WFH);
            var wfhOffAfternoon = SeedUser("pub_wfh_off_pm", WorkMode.WFH);
            var wfhWorkAtOfficeFull = SeedUser("pub_wfh_wao_full", WorkMode.WFH);
            var wfhOffAmAndWaoPm = SeedUser("pub_wfh_offam_waopm", WorkMode.WFH);
            var wfhCustomTardiness = SeedUser("pub_wfh_custom", WorkMode.WFH);
            var wfhInactive = SeedUser("pub_wfh_inactive", WorkMode.WFH, isActive: false);

            SeedRequest(officeRemoteApproved, date, RequestType.Remote, DayType.Fullday, RequestStatus.Approved);
            SeedRequest(officeRemoteRejected, date, RequestType.Remote, DayType.Fullday, RequestStatus.Rejected);
            SeedRequest(wfhOffMorning, date, RequestType.Off, DayType.Morning, RequestStatus.Approved, 3.5);
            SeedRequest(wfhOffAfternoon, date, RequestType.Off, DayType.Afternoon, RequestStatus.Approved, 4.5);
            SeedRequest(wfhWorkAtOfficeFull, date, RequestType.WorkAtOffice, DayType.Fullday, RequestStatus.Approved, 8);
            SeedRequest(wfhOffAmAndWaoPm, date, RequestType.Off, DayType.Morning, RequestStatus.Approved, 3.5);
            SeedRequest(wfhOffAmAndWaoPm, date, RequestType.WorkAtOffice, DayType.Afternoon, RequestStatus.Approved, 4.5);
            SeedRequest(wfhCustomTardiness, date, RequestType.Off, DayType.Custom, RequestStatus.Approved, 1);

            await WithUnitOfWorkAsync(async () =>
            {
                var result = await _publicAppService.GetUserWorkFromHome(date);

                // Office user with Approved Remote -> included
                result.ShouldContain(r => r.UserId == officeRemoteApproved && r.DateType == DayType.Fullday && r.Status == "Approved");
                // Office user with Rejected Remote -> excluded
                result.ShouldNotContain(r => r.UserId == officeRemoteRejected);

                // WFH with no request -> Fullday Approved
                result.ShouldContain(r => r.UserId == wfhPlain && r.DateType == DayType.Fullday && r.Status == "Approved");
                // WFH with Off Morning -> Afternoon WFH
                result.ShouldContain(r => r.UserId == wfhOffMorning && r.DateType == DayType.Afternoon && r.Status == "Approved");
                // WFH with Off Afternoon -> Morning WFH
                result.ShouldContain(r => r.UserId == wfhOffAfternoon && r.DateType == DayType.Morning && r.Status == "Approved");
                // WFH with WorkAtOffice Full -> excluded
                result.ShouldNotContain(r => r.UserId == wfhWorkAtOfficeFull);
                // WFH with Off AM + WorkAtOffice PM -> excluded
                result.ShouldNotContain(r => r.UserId == wfhOffAmAndWaoPm);
                // WFH with Off + Custom (tardiness) -> still Fullday WFH
                result.ShouldContain(r => r.UserId == wfhCustomTardiness && r.DateType == DayType.Fullday);
                // Inactive WFH user -> excluded
                result.ShouldNotContain(r => r.UserId == wfhInactive);
            });
        }

        [Fact]
        public async Task GetWorkingStatusByUser_Should_Return_Expected_Status_For_WFH_And_Office_Users()
        {
            var date = new DateTime(2026, 11, 26);

            var officeNoReq = SeedUser("gws_off_none", WorkMode.Office);
            var officeOff = SeedUser("gws_off_leave", WorkMode.Office);
            var wfhNoReq = SeedUser("gws_wfh_none", WorkMode.WFH, morning: 3.5, afternoon: 4.5);
            var wfhOffMorning = SeedUser("gws_wfh_off_am", WorkMode.WFH, morning: 3.5, afternoon: 4.5);
            var wfhOffAfternoon = SeedUser("gws_wfh_off_pm", WorkMode.WFH, morning: 3.5, afternoon: 4.5);
            var wfhWaoFull = SeedUser("gws_wfh_wao_full", WorkMode.WFH);

            SeedRequest(officeOff, date, RequestType.Off, DayType.Fullday, RequestStatus.Approved, 8);
            SeedRequest(wfhOffMorning, date, RequestType.Off, DayType.Morning, RequestStatus.Approved, 3.5);
            SeedRequest(wfhOffAfternoon, date, RequestType.Off, DayType.Afternoon, RequestStatus.Approved, 4.5);
            SeedRequest(wfhWaoFull, date, RequestType.WorkAtOffice, DayType.Fullday, RequestStatus.Approved, 8);

            await WithUnitOfWorkAsync(async () =>
            {
                // 1. Office user with no request -> null
                var resOfficeNone = await _publicAppService.GetWorkingStatusByUser("gws_off_none@ncc.asia", date);
                resOfficeNone.ShouldBeNull();

                // 2. Office user with Off -> returns Off
                var resOfficeOff = await _publicAppService.GetWorkingStatusByUser("gws_off_leave@ncc.asia", date);
                resOfficeOff.ShouldNotBeNull();
                resOfficeOff.RequestType.ShouldBe(RequestType.Off);

                // 3. WFH user with no request -> simulated Remote Fullday (8h)
                var resWfhNone = await _publicAppService.GetWorkingStatusByUser("gws_wfh_none@ncc.asia", date);
                resWfhNone.ShouldNotBeNull();
                resWfhNone.RequestType.ShouldBe(RequestType.Remote);
                resWfhNone.DayType.ShouldBe(DayType.Fullday);
                resWfhNone.Hour.ShouldBe(8);
                resWfhNone.Status.ShouldBe(RequestStatus.Approved);

                // 4. WFH user with Off Morning -> simulated Remote Afternoon (4.5h)
                var resWfhOffAm = await _publicAppService.GetWorkingStatusByUser("gws_wfh_off_am@ncc.asia", date);
                resWfhOffAm.ShouldNotBeNull();
                resWfhOffAm.RequestType.ShouldBe(RequestType.Remote);
                resWfhOffAm.DayType.ShouldBe(DayType.Afternoon);
                resWfhOffAm.Hour.ShouldBe(4.5);

                // 5. WFH user with Off Afternoon -> simulated Remote Morning (3.5h)
                var resWfhOffPm = await _publicAppService.GetWorkingStatusByUser("gws_wfh_off_pm@ncc.asia", date);
                resWfhOffPm.ShouldNotBeNull();
                resWfhOffPm.RequestType.ShouldBe(RequestType.Remote);
                resWfhOffPm.DayType.ShouldBe(DayType.Morning);
                resWfhOffPm.Hour.ShouldBe(3.5);

                // 6. WFH user with WorkAtOffice Fullday -> returns WorkAtOffice (not simulated Remote)
                var resWfhWao = await _publicAppService.GetWorkingStatusByUser("gws_wfh_wao_full@ncc.asia", date);
                resWfhWao.ShouldNotBeNull();
                resWfhWao.RequestType.ShouldBe(RequestType.WorkAtOffice);
            });
        }
    }
}
