using Abp.BackgroundJobs;
using Abp.Configuration;
using Abp.Domain.Uow;
using Abp.Runtime.Session;
using Abp.UI;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Ncc.IoC;
using NSubstitute;
using System;
using System.Data.Entity;
using System.Net.Http;
using Timesheet.APIs.MyWorkingTimes;
using Timesheet.APIs.MyWorkingTimes.Dto;
using Timesheet.Entities;
using Timesheet.Services.Komu;
using Xunit;
using static Ncc.Entities.Enum.StatusEnum;
using Task = System.Threading.Tasks.Task;

namespace Timesheet.Application.Tests.MyWorkingTimes
{
    /// <summary>
    /// 6/6 function
    /// 14/14 test cases passed
    /// update day 11/01/2023
    /// </summary>
    public class MyWorkingTimeAppService_Test : TimesheetApplicationTestBase
    {
        private readonly IBackgroundJobManager _backgroundJobManager;
        private readonly MyWorkingTimeAppService _time;
        private readonly IWorkScope _work;

        public MyWorkingTimeAppService_Test()
        {
            _work = Resolve<IWorkScope>();
            _backgroundJobManager = Resolve<IBackgroundJobManager>();
            var httpClient = Resolve<HttpClient>();
            var configuration = Substitute.For<IConfiguration>();
            configuration.GetValue<string>("HRMv2Service:BaseAddress").Returns("http://www.myserver.com");
            configuration.GetValue<string>("HRMv2Service:SecurityCode").Returns("secretCode");
            var logger = Resolve<ILogger<KomuService>>();
            configuration.GetValue<string>("KomuService:BaseAddress").Returns("http://www.myserver.com");
            configuration.GetValue<string>("KomuService:SecurityCode").Returns("secretCode");
            configuration.GetValue<string>("KomuService:DevModeChannelId").Returns("_channelIdDevMode");
            configuration.GetValue<string>("KomuService:EnableKomuNotification").Returns("_isNotifyToKomu");
            var settingManager = Substitute.For<ISettingManager>();
            var _komuService = Substitute.For<KomuService>(httpClient, logger, configuration, settingManager);
            _time = new MyWorkingTimeAppService(_backgroundJobManager, _komuService, _work);
            _time.UnitOfWorkManager = Resolve<IUnitOfWorkManager>();
            _time.AbpSession = Resolve<IAbpSession>();
            _time.SettingManager = Resolve<ISettingManager>();
        }

        [Fact]
        public async Task GetMyCurrentWorkingTime()
        {
            await WithUnitOfWorkAsync(async () =>
            {
                var result = await _time.GetMyCurrentWorkingTime();

                // Assert
                Assert.Equal(3.5, result.MorningWorkingTime);
                Assert.Equal(4.5, result.AfternoonWorkingTime);
                Assert.Equal("08:30", result.MorningStartTime);
                Assert.Equal("12:00", result.MorningEndTime);
                Assert.Equal("13:00", result.AfternoonStartTime);
                Assert.Equal("17:30", result.AfternoonEndTime);
            });
        }

        [Fact]
        public async Task SubmitNewWorkingTime()
        {
            var newData = new ChangeWorkingTimeDto
            {
                MorningStartTime = "08:00",
                MorningEndTime = "12:00",
                MorningWorkingTime = 4,
                AfternoonStartTime = "13:00",
                AfternoonEndTime = "17:00",
                AfternoonWorkingTime = 4,
                ApplyDate = DateTime.Today.AddDays(1)
            };
            WithUnitOfWork(async () =>
            {
                var result = await _time.SubmitNewWorkingTime(newData);
                Assert.Equal(1, result.Id);
                Assert.Equal(newData.MorningWorkingTime, result.MorningWorkingTime);
                Assert.Equal(newData.AfternoonWorkingTime, result.AfternoonWorkingTime);
                Assert.Equal(newData.MorningStartTime, result.MorningStartTime);
                Assert.Equal(newData.MorningEndTime, result.MorningEndTime);
                Assert.Equal(newData.AfternoonStartTime, result.AfternoonStartTime);
                Assert.Equal(newData.AfternoonEndTime, result.AfternoonEndTime);
            });
        }

        [Fact]
        public async Task SubmitNewWorkingTime_MinTime()
        {
            var newData = new ChangeWorkingTimeDto
            {
                MorningStartTime = "09:00",
                MorningEndTime = "12:00",
                MorningWorkingTime = 3,
                AfternoonStartTime = "13:00",
                AfternoonEndTime = "17:00",
                AfternoonWorkingTime = 4,
                ApplyDate = DateTime.Today.AddDays(1)
            };
            await WithUnitOfWorkAsync(async () =>
            {
                var caughtException = Assert.ThrowsAsync<UserFriendlyException>(async () => await _time.SubmitNewWorkingTime(newData));
                Assert.Equal("Total working time min 8 hours", caughtException.Result.Message);
            });
        }

        [Fact]
        public async Task SubmitNewWorkingTime_WorkingTimeMissing()
        {
            var newData = new ChangeWorkingTimeDto
            {
                MorningStartTime = "08:00",
                MorningWorkingTime = 4,
                AfternoonStartTime = "13:00",
                AfternoonEndTime = "17:00",
                AfternoonWorkingTime = 4,
                ApplyDate = DateTime.Today.AddDays(1)
            };
            await WithUnitOfWorkAsync(async () =>
            {
                var caughtException = Assert.ThrowsAsync<UserFriendlyException>(async () => await _time.SubmitNewWorkingTime(newData));
                Assert.Equal("Working time need to be completed", caughtException.Result.Message);
            });
        }

        [Fact]
        public async Task SubmitNewWorkingTime_WorkingTimeFormat()
        {
            var newData = new ChangeWorkingTimeDto
            {
                MorningStartTime = "08:00",
                MorningEndTime = "12:00:00",
                MorningWorkingTime = 4,
                AfternoonStartTime = "13:00",
                AfternoonEndTime = "17:00",
                AfternoonWorkingTime = 4,
                ApplyDate = DateTime.Today.AddDays(1)
            };
            await WithUnitOfWorkAsync(async () =>
            {
                var caughtException = Assert.ThrowsAsync<UserFriendlyException>(async () => await _time.SubmitNewWorkingTime(newData));
                Assert.Equal("Wrong working time format must be (HH:mm)", caughtException.Result.Message);
            });
        }

        [Fact]
        public async Task SubmitNewWorkingTime_ApplyDateSmaller()
        {
            var newData = new ChangeWorkingTimeDto
            {
                MorningStartTime = "08:00",
                MorningEndTime = "12:00",
                MorningWorkingTime = 4,
                AfternoonStartTime = "13:00",
                AfternoonEndTime = "17:00",
                AfternoonWorkingTime = 4,
                ApplyDate = DateTime.Today.AddDays(-2)
            };
            await WithUnitOfWorkAsync(async () =>
            {
                var caughtException = Assert.ThrowsAsync<UserFriendlyException>(async () => await _time.SubmitNewWorkingTime(newData));
                Assert.Equal("Apply time has to greater today", caughtException.Result.Message);
            });
        }

        [Fact]
        public async Task GetAllMyHistoryWorkingTime()
        {
            WithUnitOfWork(() =>
            {
                var result = _time.GetAllMyHistoryWorkingTime();
                Assert.Empty(result);
            });
        }

        [Fact]
        public async Task DeleteWorkingTime()
        {
            var id = 1;
            WithUnitOfWork(async () =>
            {
                await _time.DeleteWorkingTime(id);
            });
            WithUnitOfWork(async () =>
            {
                var historyWorkingTimes = _work.GetAll<HistoryWorkingTime>();
                var historyWorkingTime = await historyWorkingTimes.AnyAsync(x => x.Id == id);
                Assert.False(historyWorkingTime);
            });
        }

        [Fact]
        public async Task DeleteWorkingTime_ItemDoesNotExist()
        {
            WithUnitOfWork(async () =>
            {
                var caughtException = Assert.ThrowsAsync<UserFriendlyException>(async() => await _time.DeleteWorkingTime(2));
                Assert.Equal("This working time does not exist", caughtException.Result.Message);
            });
        }

        [Fact]
        public async Task DeleteWorkingTime_ItemIsApproved()
        {
            WithUnitOfWork(async () =>
            {
                var caughtException = Assert.ThrowsAsync<UserFriendlyException>(async () => await _time.DeleteWorkingTime(1));
                Assert.Equal("This working time is approved", caughtException.Result.Message);
            });
        }

        [Fact]
        public async Task EditWorkingTime()
        {
            var newData = new HistoryWorkingTimeDto
            {
                Id = 2,
                Status = RequestStatus.Rejected
            };
            WithUnitOfWork(async () =>
            {
                var result = await _time.EditWorkingTime(newData);
                Assert.Equal(RequestStatus.Rejected, result.Status);
            });
        }

        [Fact]
        public async Task EditWorkingTime_ItemIsApproved()
        {
            var newData = new HistoryWorkingTimeDto
            {
                Id = 1,
                Status = RequestStatus.Rejected
            };
            WithUnitOfWork(async () =>
            {
                var caughtException = Assert.ThrowsAsync<UserFriendlyException>(async () => await _time.EditWorkingTime(newData));
                Assert.Equal("This working time is approved", caughtException.Result.Message);
            });
        }

        [Fact]
        public async Task EditWorkingTime_ApplyDateIsNull()
        {
            var newData = new HistoryWorkingTimeDto
            {
                Id = 1,
                AfternoonEndTime = "10:20",
            };
            WithUnitOfWork(async () =>
            {
                var caughtException = Assert.ThrowsAsync<UserFriendlyException>(async () => await _time.EditWorkingTime(null));
                Assert.Equal("This working time is approved", caughtException.Result.Message);
            });
        }

        [Fact]
        public async void getReceiverListSubmit()
        {
            WithUnitOfWork(async () =>
            {
                var result = await _time.getReceiverListSubmit(1);

                // Assert
                Assert.Equal(3, result.Count);
                Assert.Equal(1, result[0].ProjectId);
                Assert.Single(result[0].PMs);
                Assert.Equal(2, result[1].PMs.Count);
                Assert.Single(result[2].PMs);
            });
        }
    }
}
