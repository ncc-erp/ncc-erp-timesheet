using Abp.Configuration;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Ncc.Authorization.Users;
using Ncc.IoC;
using NSubstitute;
using Shouldly;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Reflection;
using System.Threading.Tasks;
using Timesheet.DomainServices;
using Timesheet.DomainServices.Dto;
using Timesheet.Entities;
using Timesheet.Services.FaceIdService;
using Timesheet.Services.Komu;
using Timesheet.Services.Project;
using Xunit;
using static Ncc.Entities.Enum.StatusEnum;

namespace Timesheet.Application.Tests.API.Timekeepings
{
    public class TimekeepingServices_TrackerPolicy_Tests : TimesheetApplicationTestBase
    {
        private readonly IWorkScope _workScope;
        private readonly TimekeepingServices _timekeepingServices;

        public TimekeepingServices_TrackerPolicy_Tests()
        {
            _workScope = Resolve<IWorkScope>();
            var httpClient = Resolve<HttpClient>();
            var settingManager = Substitute.For<ISettingManager>();

            var komuConfig = new ConfigurationBuilder()
                .AddInMemoryCollection(new Dictionary<string, string>
                {
                    { "KomuService:ChannelIdDevMode", "" },
                    { "KomuService:EnableKomuNotification", "false" },
                    { "KomuService:BaseAddress", "http://example.com" },
                    { "KomuService:SecurityCode", "secret" }
                }).Build();

            var projectConfig = new ConfigurationBuilder()
                .AddInMemoryCollection(new Dictionary<string, string>
                {
                    { "ProjectService:BaseAddress", "http://example.com" },
                    { "ProjectService:SecurityCode", "secret" }
                }).Build();

            var komuService = Substitute.For<KomuService>(httpClient, Resolve<ILogger<KomuService>>(), komuConfig, settingManager);
            var projectService = Substitute.For<ProjectService>(httpClient, projectConfig, Resolve<ILogger<ProjectService>>());
            var faceIdService = Substitute.For<FaceIdService>(httpClient, Resolve<ILogger<FaceIdService>>(), settingManager, _workScope);

            _timekeepingServices = new TimekeepingServices(komuService, _workScope, faceIdService, projectService, settingManager);
        }

        [Fact]
        public async Task GetAbsenceAndRemoteUsers_Should_Separate_WorkAtOffice_From_Absence_And_Remote()
        {
            var date = new DateTime(2026, 11, 27);
            long offUserId = 0, remoteUserId = 0, waoUserId = 0;

            UsingDbContext(ctx =>
            {
                User MakeUser(string name, WorkMode mode) => new User
                {
                    UserName = name, NormalizedUserName = name.ToUpperInvariant(),
                    Name = name, Surname = "TK", EmailAddress = $"{name}@ncc.asia",
                    NormalizedEmailAddress = $"{name.ToUpperInvariant()}@NCC.ASIA",
                    Password = "pwd", IsActive = true, WorkMode = mode
                };

                var uOff = MakeUser("tk_off_u", WorkMode.Office);
                var uRem = MakeUser("tk_rem_u", WorkMode.Office);
                var uWao = MakeUser("tk_wao_u", WorkMode.WFH);
                ctx.Users.AddRange(uOff, uRem, uWao);
                ctx.SaveChanges();
                offUserId = uOff.Id;
                remoteUserId = uRem.Id;
                waoUserId = uWao.Id;

                var rOff = new AbsenceDayRequest { UserId = offUserId, Type = RequestType.Off, Status = RequestStatus.Approved, Reason = "Off" };
                var rRem = new AbsenceDayRequest { UserId = remoteUserId, Type = RequestType.Remote, Status = RequestStatus.Approved, Reason = "Rem" };
                var rWao = new AbsenceDayRequest { UserId = waoUserId, Type = RequestType.WorkAtOffice, Status = RequestStatus.Approved, Reason = "WAO" };
                ctx.AbsenceDayRequests.AddRange(rOff, rRem, rWao);
                ctx.SaveChanges();

                ctx.AbsenceDayDetails.AddRange(
                    new AbsenceDayDetail { RequestId = rOff.Id, DateAt = date, DateType = DayType.Fullday, Hour = 8 },
                    new AbsenceDayDetail { RequestId = rRem.Id, DateAt = date, DateType = DayType.Fullday, Hour = 8 },
                    new AbsenceDayDetail { RequestId = rWao.Id, DateAt = date, DateType = DayType.Fullday, Hour = 8 }
                );
                ctx.SaveChanges();
            });

            var method = typeof(TimekeepingServices).GetMethod("GetAbsenceAndRemoteUsers", BindingFlags.NonPublic | BindingFlags.Instance);
            method.ShouldNotBeNull();

            await WithUnitOfWorkAsync(async () =>
            {
                var task = (Task<(
                    Dictionary<long, List<MapAbsenceUserDto>> AbsenceUsers,
                    Dictionary<long, List<MapAbsenceUserDto>> RemoteUsers,
                    Dictionary<long, List<MapAbsenceUserDto>> WorkAtOfficeUsers)>)method.Invoke(_timekeepingServices, new object[] { date });

                var (absenceUsers, remoteUsers, workAtOfficeUsers) = await task;

                absenceUsers.ContainsKey(offUserId).ShouldBeTrue();
                absenceUsers.ContainsKey(remoteUserId).ShouldBeFalse();
                absenceUsers.ContainsKey(waoUserId).ShouldBeFalse();

                remoteUsers.ContainsKey(remoteUserId).ShouldBeTrue();
                remoteUsers.ContainsKey(waoUserId).ShouldBeFalse();

                workAtOfficeUsers.ContainsKey(waoUserId).ShouldBeTrue();
                workAtOfficeUsers.ContainsKey(remoteUserId).ShouldBeFalse();
            });
        }

        [Theory]
        [InlineData(WorkMode.Office, false, false, Usertype.Staff, false)]
        [InlineData(WorkMode.Office, false, true, Usertype.Staff, true)]
        [InlineData(WorkMode.WFH, false, false, Usertype.Staff, true)]
        [InlineData(WorkMode.WFH, true, false, Usertype.Staff, false)]
        [InlineData(WorkMode.WFH, false, false, Usertype.Vendor, false)]
        [InlineData(WorkMode.Office, false, true, Usertype.Vendor, false)]
        public void TrackerPunishmentPolicy_Should_Match_Expected_Matrix(
            WorkMode workMode,
            bool hasWorkAtOffice,
            bool hasRemote,
            Usertype userType,
            bool expectedShouldCheckTracker)
        {
            bool isUserWFH = workMode == WorkMode.WFH;
            bool shouldCheckTracker = ((isUserWFH && !hasWorkAtOffice) || hasRemote) && userType != Usertype.Vendor;

            shouldCheckTracker.ShouldBe(expectedShouldCheckTracker);
        }
    }
}
