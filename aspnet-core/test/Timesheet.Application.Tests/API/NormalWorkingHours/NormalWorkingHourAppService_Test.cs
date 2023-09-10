using Ncc.IoC;
using Shouldly;
using System;
using System.Collections.Generic;
using Timesheet.APIs.NormalWorkingHours;
using Timesheet.APIs.NormalWorkingHours.Dto;
using Timesheet.Entities;
using Timesheet.Paging;
using Xunit;
using static Ncc.Entities.Enum.StatusEnum;

namespace Timesheet.Application.Tests.API.NormalWorkingHours
{
    /// <summary>
    /// 3/3 function
    /// 4/4 test cases passed
    /// update day 16/01/2023
    /// </summary>
    public class NormalWorkingHourAppService_Test : TimesheetApplicationTestBase
    {
        private readonly NormalWorkingHourAppService _normal;
        private readonly IWorkScope _work;

        public NormalWorkingHourAppService_Test()
        {
            _work = Resolve<IWorkScope>();
            _normal = Resolve<NormalWorkingHourAppService>();
            //_project.UnitOfWorkManager = Resolve<IUnitOfWorkManager>();
        }

        [Fact]
        public async void TestGetAllPagging()
        {
            var input = new GridParam();
            int year = 2020;
            int month = 1;
            long? branchId = null;
            long? projectId = null;
            bool isThanDefaultWorking = false;
            int? checkInFilter = null;
            int tsStatusFilter = 0;

            await WithUnitOfWorkAsync(async () =>
            {
                var result = await _normal.GetAllPagging(input, year, month, branchId, projectId, isThanDefaultWorking, checkInFilter, tsStatusFilter);
                Assert.NotNull(result);
                Assert.Equal(10, result.Items.Count);
                Assert.Equal(21, result.TotalCount);
                Assert.IsType<GridResult<GetNormalWorkingHourDto>>(result);
                result.Items.ShouldContain(item => item.AvatarFullPath == "");
                result.Items.ShouldContain(item => item.AvatarPath == "");
                result.Items.ShouldContain(item => item.Branch == null);
                result.Items.ShouldContain(item => item.EmailAddress == "tien.nguyenhuu@ncc.asia");
                result.Items.ShouldContain(item => item.FullName == "Nguyễn Hữu Tiến");
                result.Items.ShouldContain(item => item.IsPM == true);
                result.Items.ShouldContain(item => item.Level == null);
                result.Items.ShouldContain(item => item.Name == "Tiến");
                result.Items.ShouldContain(item => item.Surname == "Nguyễn Hữu");
            });
        }

        [Fact]
        public async void TestGetAllPagging_BranchHN()
        {
            var input = new GridParam();
            int year = 2022;
            int month = 10;
            long? branchId = 1;
            long? projectId = null;
            bool isThanDefaultWorking = false;
            int? checkInFilter = null;
            int tsStatusFilter = 1;

            await WithUnitOfWorkAsync(async () =>
            {
                var result = await _normal.GetAllPagging(input, year, month, branchId, projectId, isThanDefaultWorking, checkInFilter, tsStatusFilter);
                Assert.NotNull(result);
                Assert.Equal(8, result.Items.Count);
                Assert.Equal(8, result.TotalCount);
                Assert.IsType<GridResult<GetNormalWorkingHourDto>>(result);
                result.Items.ShouldContain(item => item.AvatarFullPath == "");
                result.Items.ShouldContain(item => item.AvatarPath == "");
                result.Items.ShouldContain(item => item.Branch == null);
                result.Items.ShouldContain(item => item.EmailAddress == "hien.ngothu@ncc.asia");
                result.Items.ShouldContain(item => item.FullName == "Ngô Thu Hiền");
                result.Items.ShouldContain(item => item.IsPM == false);
                result.Items.ShouldContain(item => item.Level == null);
                result.Items.ShouldContain(item => item.Name == "Hiền");
                result.Items.ShouldContain(item => item.Surname == "Ngô Thu");
            });
        }

        [Fact]
        public async void TestExportNormalWorking()
        {
            var input = new GridParam();
            int year = 2020;
            int month = 1;
            long? branchId = null;
            long? projectId = null;
            bool isThanDefaultWorking = false;
            int? checkInFilter = null;
            int tsStatusFilter = 0;

            await WithUnitOfWorkAsync(async () =>
            {
                var result = await _normal.ExportNormalWorking(input, year, month, branchId, projectId, isThanDefaultWorking, checkInFilter, tsStatusFilter);
                Assert.NotNull(result);
                Assert.IsType<Byte[]>(result);
            });
        }

        [Fact]
        public async void GetNormalWorkingHourByUserLogin()
        {
            var input = new GridParam();
            long? branchId = null;
            long? projectId = null;
            bool isThanDefaultWorking = false;
            int? checkInFilter = null;
            int tsStatusFilter = 0;
            var year = 2022;
            var month = 2;
            var now = new DateTime(2022, 2, 15);
            var dayOfWeek = (int)Enum.Parse(typeof(System.DayOfWeek), now.DayOfWeek.ToString());
            byte[] arrDayOfWeek = { 7, 1, 2, 3, 4, 5, 6 };
            var lockMonth = now.AddDays(-(now.Day));
            var lockWeek = now.AddDays(-arrDayOfWeek[dayOfWeek]);
            var lockDate = lockWeek > lockMonth ? lockWeek : lockMonth;
            var lockDateAfterUnlock = new DateTime(2022, 1, 1);

            var unlockTSs = new List<UnlockTimesheet>
            {
                new UnlockTimesheet { UserId = 1, Type = LockUnlockTimesheetType.MyTimesheet },
                new UnlockTimesheet { UserId = 2, Type = LockUnlockTimesheetType.ApproveRejectTimesheet },
            };
            var dayOffSettings = new List<DateTime> { new DateTime(2022, 2, 1), new DateTime(2022, 2, 2) };
            var absencedays = new List<AbsenceDayDetailNormal>
            {
                new AbsenceDayDetailNormal { UserId = 1, DateAt = new DateTime(2022, 2, 3), DateType = DayType.Fullday, Hour = 8, Type = RequestType.Off },
                new AbsenceDayDetailNormal { UserId = 1, DateAt = new DateTime(2022, 2, 4), DateType = DayType.Fullday, Hour = 8, Type = RequestType.Remote, AbsenceTime = OnDayType.DiMuon },
            };
            var listTimekeeping = new List<CheckInDto>
            {
                new CheckInDto { UserId = 1, CheckIn = "8:00 1/2/2022", CheckOut = "18:00 1/2/2022", DateAt = new DateTime(2022, 2, 1) },
                new CheckInDto { UserId = 1, CheckIn = "8:00 2/2/2022", CheckOut = "18:00 1/2/2022", DateAt = new DateTime(2022, 2, 2) },
                new CheckInDto { UserId = 1, CheckIn = "8:00 3/2/2022", CheckOut = "18:00 1/2/2022", DateAt = new DateTime(2022, 2, 3) },
            };
            var listMyTimesheet = new List<MyTimesheet>
            {
                new MyTimesheet { UserId = 1, DateAt = new DateTime(2022, 2, 1), Status = TimesheetStatus.All }
            };

            await WithUnitOfWorkAsync(async () =>
            {
                var result = await _normal.ExportNormalWorking(input, year, month, branchId, projectId, isThanDefaultWorking, checkInFilter, tsStatusFilter);
                Assert.NotNull(result);
                Assert.IsType<Byte[]>(result);
            });
        }
    }
}
