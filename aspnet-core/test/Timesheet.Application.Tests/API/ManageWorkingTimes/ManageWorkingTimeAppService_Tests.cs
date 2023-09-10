using Abp.BackgroundJobs;
using Abp.Configuration;
using Abp.Domain.Uow;
using Abp.Runtime.Session;
using Abp.UI;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Ncc.Authorization.Users;
using Ncc.IoC;
using NSubstitute;
using Shouldly;
using System;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Net.Http;
using System.Threading.Tasks;
using Timesheet.APIs.ManageWorkingTimes;
using Timesheet.APIs.ManageWorkingTimes.Dto;
using Timesheet.Entities;
using Timesheet.Services.Komu;
using Timesheet.Uitls;
using Xunit;
using static Ncc.Entities.Enum.StatusEnum;

namespace Timesheet.Application.Tests.API.ManageWorkingTimes
{
    /// <summary>
    /// 4/4 functions
    /// 7/7 test cases passed
    /// update day 11/01/2023
    /// </summary>

    public class ManageWorkingTimeAppService_Tests : TimesheetApplicationTestBase
    {
        private readonly ManageWorkingTimeAppService _manageWorkingTime;
        private readonly IWorkScope _workScope;

        public ManageWorkingTimeAppService_Tests()
        {
            var httpClient = Resolve<HttpClient>();
            var configuration = Substitute.For<IConfiguration>();
            var settingManager = Substitute.For<ISettingManager>();
            var loggerKomu = Resolve<ILogger<KomuService>>();
            configuration.GetValue<string>("KomuService:BaseAddress").Returns("http://www.myserver.com");
            configuration.GetValue<string>("KomuService:SecurityCode").Returns("secretCode");
            configuration.GetValue<string>("KomuService:DevModeChannelId").Returns("_channelIdDevMode");
            configuration.GetValue<string>("KomuService:EnableKomuNotification").Returns("_isNotifyToKomu");
            var komuService = Substitute.For<KomuService>(httpClient, loggerKomu, configuration, settingManager);

            _workScope = Resolve<IWorkScope>();

            var backgroundJobManager = Resolve<BackgroundJobManager>();
            _manageWorkingTime = new ManageWorkingTimeAppService(backgroundJobManager, _workScope, komuService)
            {
                AbpSession = Resolve<IAbpSession>(),
                UnitOfWorkManager = Resolve<IUnitOfWorkManager>(),
                SettingManager = Resolve<ISettingManager>(),
            };

        }

        [Fact]
        public async Task GetAll_Test1()
        {
            var expectTotalCount = 1;
            var filter = new FilterDto
            {
                userName = "ncc.asia",
                status = RequestStatus.Pending,
                projectIds = new System.Collections.Generic.List<long> { 1, 2, 3, 4, 5, 6 }
            };

            await WithUnitOfWorkAsync(async () =>
            {
                var result = await _manageWorkingTime.GetAll(filter);
                Assert.Equal(expectTotalCount, result.Count);

                result.Last().UserId.ShouldBe(6);
                result.Last().FullName.ShouldBe("Hiếu Trần Trung");
                result.Last().Type.ShouldBe(Usertype.Collaborators);
                result.Last().UserName.ShouldBe("hieu.trantrung@ncc.asia");
                result.Last().Status.ShouldBe(RequestStatus.Pending);
                result.Last().AfternoonStartTime.ShouldBe("13:00");
                result.Last().AfternoonEndTime.ShouldBe("18:00");
                result.Last().AfternoonWorkingTime.ShouldBe(5);
                result.Last().MorningStartTime.ShouldBe("09:00");
                result.Last().MorningEndTime.ShouldBe("12:00");
                result.Last().MorningWorkingTime.ShouldBe(3);
                result.Last().ApplyDate.Date.ShouldBe(new DateTime(2022,12,28).Date);
                result.Last().ReqestTime.Date.ShouldBe(new DateTime(2022, 12, 27).Date);

            });
        }

        [Fact]
        public async Task ApproveWorkingTime_Should_Approve()
        {
            var workingTimeId = 2;

            await WithUnitOfWorkAsync(async () =>
            {
                await _manageWorkingTime.ApproveWorkingTime(workingTimeId);
            });

            await WithUnitOfWorkAsync(async () =>
            {
                var workingTime = await _workScope.GetAsync<HistoryWorkingTime>(workingTimeId);
                workingTime.Status.ShouldBe(RequestStatus.Approved);

                var user = await _workScope.GetAsync<User>(workingTime.UserId);
                user.MorningStartAt.ShouldBe(workingTime.MorningStartTime);
                user.MorningEndAt.ShouldBe(workingTime.MorningEndTime);
                user.MorningWorking.ShouldBe(workingTime.MorningWorkingTime);
                user.AfternoonStartAt.ShouldBe(workingTime.AfternoonStartTime);
                user.AfternoonEndAt.ShouldBe(workingTime.AfternoonEndTime);
                user.AfternoonWorking.ShouldBe(workingTime.AfternoonWorkingTime);
                user.isWorkingTimeDefault.ShouldBe(false);

                var deleteBgrdjs = _workScope.GetAll<BackgroundJobInfo>()
                    .Where(s => s.JobType.StartsWith("Timesheet.BackgroundJob.WorkingTimeBackgroundJob") && s.JobArgs.Contains($"\"UserId\":{workingTime.UserId}"));
                deleteBgrdjs.Count().ShouldBe(0);

                var absenceDayDetails = _workScope.GetAll<AbsenceDayDetail>()
                                                    .Where(s => s.Request.UserId == workingTime.UserId)
                                                    .Where(s => s.DateAt.Date >= DateTimeUtils.GetNow().Date)
                                                    .Where(s => s.Request.Type == RequestType.Off)
                                                    .Where(s => s.DateType == DayType.Morning || s.DateType == DayType.Afternoon)
                                                    .ToList();

                foreach (var item in absenceDayDetails)
                {
                    if (item.DateType == DayType.Morning)
                        item.Hour.ShouldBe(workingTime.MorningWorkingTime);
                    else
                        item.Hour.ShouldBe(workingTime.AfternoonWorkingTime);
                }
            });
        }

        [Fact]
        public async Task ApproveWorkingTime_Should_Not_Approve_Because_You_Are_Not_PM_Of_User()
        {
            var workingTimeId = 0L;

            var newUser = new User
            {
                UserName = "test@ncc.asia",
                EmailAddress = "test@ncc.asia",
                Surname = "test",
                Name = "test",
                Password = "testpassword",
                BeginLevel = UserLevel.Intern_3,
                Level = UserLevel.Fresher,
                Salary = 9000000,
                Sex = Sex.Male,
                Type = Usertype.Staff,
                BranchId = 1,
                PositionId = 1,
            };

            await WithUnitOfWorkAsync(async () =>
            {
                newUser.Id = await _workScope.InsertAndGetIdAsync<User>(newUser);
                newUser.Id.ShouldNotBeNull();
            });

            await WithUnitOfWorkAsync(async () =>
            {
                var newHistoryWorkingTime = new HistoryWorkingTime
                {
                    UserId = newUser.Id,
                    MorningStartTime = "08:00",
                    MorningEndTime = "12:00",
                    MorningWorkingTime = 4,
                    AfternoonStartTime = "13:00",
                    AfternoonEndTime = "17:00",
                    AfternoonWorkingTime = 4,
                    Status = RequestStatus.Pending
                };
                workingTimeId = await _workScope.InsertAndGetIdAsync<HistoryWorkingTime>(newHistoryWorkingTime);
                workingTimeId.ShouldNotBe(0L);
            });

            await WithUnitOfWorkAsync(async () =>
            {
                var exception = await Assert.ThrowsAsync<UserFriendlyException>(async () =>
                {
                    await _manageWorkingTime.ApproveWorkingTime(workingTimeId);
                });
                Assert.Equal("You are not PM of user test test", exception.Message);
            });
        }

        [Fact]
        public async Task RejectWorkingTime_Should_Reject()
        {
            var workingTimeId = 2;

            await WithUnitOfWorkAsync(async () =>
            {
                await _manageWorkingTime.RejectWorkingTime(workingTimeId);
            });

            await WithUnitOfWorkAsync(async () =>
            {
                var workingTime = await _workScope.GetAsync<HistoryWorkingTime>(workingTimeId);
                var getDeletedBackgroundJob = _workScope.GetAll<BackgroundJobInfo>()
                        .Where(b => b.JobArgs.Contains($"\"Id\":{workingTimeId}") && b.JobType.Contains("Timesheet.BackgroundJob.WorkingTimeBackgroundJob"));

                getDeletedBackgroundJob.Count().ShouldBe(0);
                workingTime.Status.ShouldBe(RequestStatus.Rejected);
            });
        }

        [Fact]
        public async Task RejectWorkingTime_Should_Not_Reject_Because_You_Are_Not_PM_Of_User()
        {
            var workingTimeId = 0L;

            var newUser = new User
            {
                UserName = "test@ncc.asia",
                EmailAddress = "test@ncc.asia",
                Surname = "test",
                Name = "test",
                Password = "testpassword",
                BeginLevel = UserLevel.Intern_3,
                Level = UserLevel.Fresher,
                Salary = 9000000,
                Sex = Sex.Male,
                Type = Usertype.Staff,
                BranchId = 1,
                PositionId = 1,
            };

            await WithUnitOfWorkAsync(async () =>
            {
                newUser.Id = await _workScope.InsertAndGetIdAsync<User>(newUser);
                newUser.Id.ShouldNotBeNull();
            });

            await WithUnitOfWorkAsync(async () =>
            {
                var newHistoryWorkingTime = new HistoryWorkingTime
                {
                    UserId = newUser.Id,
                    MorningStartTime = "08:00",
                    MorningEndTime = "12:00",
                    MorningWorkingTime = 4,
                    AfternoonStartTime = "13:00",
                    AfternoonEndTime = "17:00",
                    AfternoonWorkingTime = 4,
                    Status = RequestStatus.Pending
                };
                workingTimeId = await _workScope.InsertAndGetIdAsync<HistoryWorkingTime>(newHistoryWorkingTime);
                workingTimeId.ShouldNotBe(0L);
            });

            await WithUnitOfWorkAsync(async () =>
            {
                var exception = await Assert.ThrowsAsync<UserFriendlyException>(async () =>
                {
                    await _manageWorkingTime.RejectWorkingTime(workingTimeId);
                });
                Assert.Equal($"You are not PM of user test test", exception.Message);
            });
        }

        [Fact]
        public async Task RejectWorkingTime_Should_Not_Reject_Because_WorkingTime_Is_Approved()
        {
            var workingTimeId = 1;
            await WithUnitOfWorkAsync(async () =>
            {
                var exception = await Assert.ThrowsAsync<UserFriendlyException>(async () =>
                {
                    await _manageWorkingTime.RejectWorkingTime(workingTimeId);
                });
                Assert.Equal("This working time is approved", exception.Message);
            });
        }

        [Fact]
        public async Task getReceiverListApproveChangeWorkingTime_Test()
        {
            var requesterId = 1;
            var expectTotalCount = 3;

            await WithUnitOfWorkAsync(async () =>
            {
                var result = await _manageWorkingTime.getReceiverListApproveChangeWorkingTime(requesterId);

                Assert.Equal(expectTotalCount, result.Count);
                result.Last().ProjectId.ShouldBe(6);
                result.Last().IsNotifyEmail.ShouldBe(false);
                result.Last().IsNotifyKomu.ShouldBe(false);
                result.Last().IsNoticeKMSubmitTS.ShouldBe(false);
                result.Last().IsNoticeKMRequestOffDate.ShouldBe(false);
                result.Last().IsNoticeKMApproveRequestOffDate.ShouldBe(false);
                result.Last().IsNoticeKMRequestChangeWorkingTime.ShouldBe(false);
                result.Last().IsNoticeKMApproveChangeWorkingTime.ShouldBe(false);

                result.Last().PMs.Last().UserId.ShouldBe(1);
                result.Last().PMs.Last().EmailAddress.ShouldBe("admin@aspnetboilerplate.com");
                result.Last().PMs.Last().FullName.ShouldBe("admin admin");
                result.Last().PMs.Last().KomuAccountInfo.ShouldBe("**admin admin** [ - NoType]");
            });
        }
    }
}
