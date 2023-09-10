using Abp.Configuration;
using Microsoft.EntityFrameworkCore;
using Ncc.Configuration;
using Ncc.IoC;
using Shouldly;
using System;
using System.Collections.Generic;
using System.Linq;
using Timesheet.APIs.Info;
using Timesheet.APIs.Info.Dto;
using Timesheet.Entities;
using Xunit;
using static Ncc.Entities.Enum.StatusEnum;

namespace Timesheet.Application.Tests.API.Info
{
    public class InfoAppServiceTest : InfoAppServiceTestBase
    {
        // 4 function, 4 test case
        // Some function can't write unit test when they using _httpContextAccessor.HttpContext.Request.Headers (null)

        private readonly InfoAppService _infoAppService;

        public InfoAppServiceTest()
        {
            _infoAppService = InfoAppServiceInstance();
        }

        [Fact]
        public async void GetAllTimeSheetLockByEmail()
        {
            List<EmployeeLockedWeekDto> listLockedDate = null;
            int timesLockedEm = 0, amount = 0, lockedPM = 0, amountPM = 0;
            var emailParam = "toai.nguyencong@ncc.asia";
            var userId = 4;
            var expectedDate = (await _infoAppService.getStartDateToCheckUnlockTS()).ToString("dd/MM/yyyy");

            await WithUnitOfWorkAsync(async () =>
            {

                var result = await _infoAppService.GetAllTimesheetLocked1(emailParam);

                listLockedDate = await _infoAppService.getMyTimesheetLockedAsync(userId);
                lockedPM = await _infoAppService.getTimesheetLockedOfPMAsync(userId);

                timesLockedEm = listLockedDate == null ? 0 : listLockedDate.Count();
                amount = timesLockedEm >= 4 ? 100000 : timesLockedEm * 20000;
                amountPM = 50000 * lockedPM;

                Assert.Equal(timesLockedEm, result.LockedEmployee.Count);
                Assert.True(result.IsPM);
                Assert.False(result.IsUnlockLog);
                Assert.False(result.IsUnlockApprove);
                Assert.Equal(amount, result.Amount);
                Assert.Equal(amountPM, result.AmountPM);
                Assert.Equal(expectedDate, result.FirstDateCanLogIfUnlock);
            });
        }

        [Fact]
        public async void GetAllTimeSheetLockById()
        {
            var idParam = 17;
            var expectedAmount = 0;
            var expectedAmountPM = 0;
            var currentTime = DateTime.Now;
            var expectedDate = (await _infoAppService.getStartDateToCheckUnlockTS()).ToString("dd/MM/yyyy");
            //var expectedDate = string.Format("01/{0}/{1}", currentTime.Month < 10 ? "0" + currentTime.Month.ToString() : currentTime.Month.ToString(), currentTime.Year);

            // Action
            await WithUnitOfWorkAsync(async () =>
            {

                var result = await _infoAppService.GetAllTimesheetLocked(idParam);

                Assert.True(result.IsPM);
                Assert.True(result.IsUnlockLog);
                Assert.False(result.IsUnlockApprove);
                Assert.Equal(expectedAmount, result.Amount);
                Assert.Equal(expectedAmountPM, result.AmountPM);
                Assert.Equal(expectedDate, result.FirstDateCanLogIfUnlock);
            });
        }

        [Fact]
        public async void TopUserUnlock()
        {
            var workScope = Resolve<IWorkScope>();
            var expectedNumberOfUserUnlockIms = 10;
            var expectedFullName = "Phạm Thị Hà";

            await WithUnitOfWorkAsync(async () =>
            {
                // Get expected Total amount
                var expectedAmount = await workScope.GetAll<Fund>().Where(s => s.Status == FundStatus.Proceeds).Select(s => s.Amount).FirstOrDefaultAsync();

                // Action
                var result = _infoAppService.TopUserUnlock();

                // Get the first 3 userUnlockIms
                var userUnlockTop1 = result.Result.ListUnlock.First();
                var userUnlockTop2 = result.Result.ListUnlock[1];
                var userUnlockTop3 = result.Result.ListUnlock[2];

                // Check result get 10 top userUnlockIms
                Assert.Equal(expectedNumberOfUserUnlockIms, result.Result.ListUnlock.Count);

                // Compare actual and expected total amount
                Assert.Equal(expectedAmount, result.Result.TotalAmount);

                result.Result.ListUnlock.ShouldContain(userUnlock => userUnlock.FullName == expectedFullName);

                // Check rank of the first 3 userUnlockIms
                Assert.Equal(1, userUnlockTop1.Rank);
                Assert.Equal(2, userUnlockTop2.Rank);
                Assert.Equal(3, userUnlockTop3.Rank);

                // Check sorting is correct
                userUnlockTop1.Amount.ShouldBeGreaterThan(userUnlockTop2.Amount);
                userUnlockTop1.Amount.ShouldBeGreaterThan(userUnlockTop3.Amount);
                userUnlockTop2.Amount.ShouldBeGreaterThan(userUnlockTop3.Amount);
            });
        }

        [Fact]
        public async void GetAllHistory()
        {
            var workScope = Resolve<IWorkScope>();
            var expectedNumberOfUserUnlockIms = 10;
            var expectContent = "đã đóng góp ngân khố";

            await WithUnitOfWorkAsync(async () =>
            {
                // Action
                var result = _infoAppService.GetAllHistory();

                // Get the first userUnlockIms
                var theFirstUserUnlockIms = workScope.GetAll<UserUnlockIms>().OrderByDescending(userUnlockIms => userUnlockIms.CreationTime).First();
                var expectedDay = theFirstUserUnlockIms.CreationTime.ToString("dd/MM/yyy HH:mm");

                // Check result get 10 top userUnlockIms 
                Assert.Equal(expectedNumberOfUserUnlockIms, result.Result.Count);

                // Check content of HistoryUnlock
                result.Result.ShouldContain(unlockIms => unlockIms.Content.Contains(expectContent));

                // Check unlockDay
                result.Result.ShouldContain(unlockIms => unlockIms.Day == expectedDay);
            });
        }
    }
}
