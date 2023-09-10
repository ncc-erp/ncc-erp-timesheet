using Castle.Facilities.TypedFactory.Internal;
using Ncc.Authorization.Users;
using Ncc.IoC;
using NPOI.SS.UserModel;
using Shouldly;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Timesheet.APIs.KomuTrackers;
using Timesheet.APIs.KomuTrackers.Dto;
using Timesheet.Entities;
using Xunit;

namespace Timesheet.Application.Tests.API.KomuTrackers
{
    /*<summary>
    2/2 functions
    8 passed test cases
    Updated date: 16/01//2023
    </summary>*/

    public class KomuTrackerAppServiceTest : KomuTrackerAppServiceTestBase
    {
        private readonly KomuTrackerAppService _komuTrackerAppService;

        public KomuTrackerAppServiceTest()
        {
            _komuTrackerAppService = InstanceKomuTrackerAppService();
        }

        [Fact]
        public async void GetAllPagingByGridParam()
        {
            var workScope = Resolve<IWorkScope>();
            var gridParam = GridParam();
            gridParam.MaxResultCount = 20;

            await WithUnitOfWorkAsync(async () =>
            {
                // Action
                var result = await _komuTrackerAppService.GetAllPagging(gridParam, null, null, null);

                var allKomuTracker = workScope.GetAll<KomuTracker>().ToList();

                Assert.Equal(allKomuTracker.Count, result.Items.Count);
                result.Items.Count.ShouldBeLessThanOrEqualTo(gridParam.MaxResultCount);
            });
        }

        [Fact]
        public async void GetAllPagingByDateAt()
        {
            var gridParam = GridParam();
            var dateAtParam = new DateTime(2022, 1, 1);

            await WithUnitOfWorkAsync(async () =>
            {
                var result = await _komuTrackerAppService.GetAllPagging(gridParam, dateAtParam, null, null);

                result.Items.Count.ShouldBeLessThanOrEqualTo(gridParam.MaxResultCount);
                Assert.True(result.Items.All(komuTracker => komuTracker.DateAt == dateAtParam));
            });
        }

        [Fact]
        public async void GetAllPagingByBranchId()
        {
            var workScope = Resolve<IWorkScope>();
            var gridParam = GridParam();
            var branchIdParam = 2;

            await WithUnitOfWorkAsync(async () =>
            {
                var result = await _komuTrackerAppService.GetAllPagging(gridParam, null, branchIdParam, null);

                var emailFromResult = result.Items.First().EmailAddress;
                var expectedBranchId = workScope.GetAll<User>()
                .Where(user => user.EmailAddress == emailFromResult)
                .First().BranchId;

                result.Items.Count.ShouldBeLessThanOrEqualTo(gridParam.MaxResultCount);
                Assert.Equal(branchIdParam, expectedBranchId);
            });
        }

        [Fact]
        public async void GetAllPagingByEmail()
        {
            var gridParam = GridParam();
            var emailParam = "toai.nguyencong@ncc.asia";

            await WithUnitOfWorkAsync(async () =>
            {
                var result = await _komuTrackerAppService.GetAllPagging(gridParam, null, null, emailParam);

                result.Items.Count.ShouldBeLessThanOrEqualTo(gridParam.MaxResultCount);
                Assert.True(result.Items.All(komuTracker => komuTracker.EmailAddress == emailParam));
            });
        }

        [Fact]
        public async void GetAllPaging()
        {
            var workScope = Resolve<IWorkScope>();
            var gridParam = GridParam();
            var emailParam = "toai.nguyencong@ncc.asia";
            var branchId = 2;
            var dateAt = new DateTime(2022, 1, 1);

            await WithUnitOfWorkAsync(async () =>
            {
                var result = await _komuTrackerAppService.GetAllPagging(gridParam, null, null, emailParam);

                var emailFromResult = result.Items.First().EmailAddress;
                var expectedBranchId = workScope.GetAll<User>()
                .Where(user => user.EmailAddress == emailFromResult)
                .First().BranchId;

                result.Items.Count.ShouldBeLessThanOrEqualTo(gridParam.MaxResultCount);
                Assert.True(result.Items.All(komuTracker => komuTracker.EmailAddress == emailParam));
                Assert.Equal(branchId, expectedBranchId);
                Assert.Contains(result.Items, komuTracker => komuTracker.DateAt == dateAt);
            });
        }

        [Fact]
        public async void CanNotSaveWithoutKomuTracker()
        {
            var komuTrackerDto = SaveKomuTrackerDto();
            komuTrackerDto.ListKomuTracker = new List<KomuTrackerDto>();
            var expectedMessage = "ListKomuTracker empty";

            await WithUnitOfWorkAsync(async () =>
            {
                var result = await _komuTrackerAppService.Save(komuTrackerDto);

                Assert.Equal(expectedMessage, result);
            });
        }

        [Fact]
        public async void Insert()
        {
            var workScope = Resolve<IWorkScope>();
            var listKomuTrackersBeforeAction = new List<KomuTracker>();
            var komuTrackerDto = SaveKomuTrackerDto();

            await WithUnitOfWorkAsync(async () =>
            {
                // Get list komutracker before action
                listKomuTrackersBeforeAction = workScope.GetAll<KomuTracker>().ToList();

                // Action
                var result = await _komuTrackerAppService.Save(komuTrackerDto);
            });

            WithUnitOfWork(() =>
            {
                // Get list komutracker after action
                var listKomuTrackersAfterAction = workScope.GetAll<KomuTracker>().ToList();
                var theLastKomuTracker = listKomuTrackersAfterAction.Last();

                listKomuTrackersAfterAction.Count.ShouldBeGreaterThan(listKomuTrackersBeforeAction.Count);
                Assert.Equal(komuTrackerDto.DateAt, theLastKomuTracker.DateAt);
            });
        }

        [Fact]
        public async void Update()
        {
            var workScope = Resolve<IWorkScope>();
            var listKomuTrackersBeforeAction = new List<KomuTracker>();
            var komuTrackerUpdate = KomuTrackerDto1();
            var komuTrackerDto = KomuTrackerUpdateDto();


            await WithUnitOfWorkAsync(async () =>
            {
                // Get list komutracker before action
                listKomuTrackersBeforeAction = workScope.GetAll<KomuTracker>().ToList();

                // Action
                var result = await _komuTrackerAppService.Save(komuTrackerDto);
            });

            WithUnitOfWork(() =>
            {
                // Get list komutracker after action
                var listKomuTrackersAfterAction = workScope.GetAll<KomuTracker>().ToList();

                // Get komuTracker that has just updated
                var theLastKomuTracker = listKomuTrackersAfterAction
                .Where(komuTracker => komuTracker.EmailAddress == komuTrackerUpdate.EmailAddress).First();

                // Verify number of komuTracker not change after update
                Assert.Equal(listKomuTrackersAfterAction.Count, listKomuTrackersBeforeAction.Count);

                // Verify propertied of komutracker that has just updated
                Assert.Equal(komuTrackerUpdate.ComputerName, theLastKomuTracker.ComputerName);
                Assert.Equal(komuTrackerUpdate.WorkingMinute, theLastKomuTracker.WorkingMinute);
            });
        }
    }
}