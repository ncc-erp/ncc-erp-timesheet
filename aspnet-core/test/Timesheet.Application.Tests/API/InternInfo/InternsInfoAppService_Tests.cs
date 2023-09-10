
using Ncc.IoC;
using Shouldly;
using System;
using System.Threading.Tasks;
using Timesheet.APIs.InternInfo;
using Timesheet.APIs.InternInfo.Dto;
using Xunit;
using Timesheet.Entities;
using Ncc.Authorization.Users;

namespace Timesheet.Application.Tests.API.InternsInfo
{
    public class InternsInfoAppService_Test : TimesheetApplicationTestBase
    {
        /// <summary>
        /// 7/7 funtions
        /// 8/8 test cases passed
        /// update day 11/01/2023
        /// </summary>
        private readonly InternsInfoAppService _internsInfoAppService;
        private readonly IWorkScope _workScope;

        public InternsInfoAppService_Test()
        {
            _workScope = Resolve<IWorkScope>();

            _internsInfoAppService = new InternsInfoAppService(_workScope);
        }

        [Fact]
        public async Task Should_Get_All_Interns_Infor_By_OnBoardDate()
        {
            await WithUnitOfWorkAsync(async () =>
            {
                // Arrange
                var input = new InputInternInfoDto
                {
                    DateFilterType = DateFilterType.OnBoardDate,
                    StartDate = new DateTime(2022, 1, 1),
                    EndDate = new DateTime(2022, 12, 30),
                };

                // Act
                var result = _internsInfoAppService.GetAll(input);

                // Assert
                Assert.NotNull(result);
                result.ListMonth.Count.ShouldBeGreaterThanOrEqualTo(4);
                result.ListMonth.ShouldContain(item => item == "06-2022");
                result.ListMonth.ShouldContain(item => item == "09-2022");
                result.ListInternInfo.TotalCount.ShouldBeGreaterThanOrEqualTo(9);
                result.ListInternInfo.Items.Count.ShouldBeGreaterThanOrEqualTo(9);
                result.ListInternInfo.Items.ShouldContain(item => item.BeginLevel == Ncc.Entities.Enum.StatusEnum.UserLevel.Intern_3);
                result.ListInternInfo.Items.ShouldNotContain(item => item.BeginLevel == Ncc.Entities.Enum.StatusEnum.UserLevel.FresherMinus);
            });
        }

        [Fact]
        public async Task Should_Get_All_Interns_Infor_By_OutDate()
        {
            await WithUnitOfWorkAsync(async () =>
            {
                // Arrange
                var input = new InputInternInfoDto
                {
                    DateFilterType = DateFilterType.OutDate,
                    StartDate = new DateTime(2022, 1, 1),
                    EndDate = new DateTime(2022, 12, 30),
                };

                // Act
                var result = _internsInfoAppService.GetAll(input);

                // Assert
                Assert.Null(result);
            });
        }

        [Fact]
        public async Task Should_Get_All_Interns_Infor_By_BeStaffDate()
        {
            await WithUnitOfWorkAsync(async () =>
            {
                // Arrange
                var input = new InputInternInfoDto
                {
                    DateFilterType = DateFilterType.BeStaffDate,
                    StartDate = new DateTime(2022, 1, 1),
                    EndDate = new DateTime(2022, 12, 30),
                };

                // Act
                var result = _internsInfoAppService.GetAll(input);

                // Assert
                Assert.NotNull(result);
                result.ListMonth.Count.ShouldBeGreaterThanOrEqualTo(4);
                result.ListMonth.ShouldContain(item => item == "06-2022");
                result.ListMonth.ShouldContain(item => item == "09-2022");
                result.ListInternInfo.TotalCount.ShouldBeGreaterThanOrEqualTo(4);
                result.ListInternInfo.Items.Count.ShouldBeGreaterThanOrEqualTo(4);
                foreach (var item in result.ListInternInfo.Items)
                {
                    item.ReviewDetails.ShouldContain(item => item.NewLevel == Ncc.Entities.Enum.StatusEnum.UserLevel.FresherMinus);
                };
                foreach (var item in result.ListInternInfo.Items)
                {
                    item.ReviewDetails.ShouldNotContain(item => item.NewLevel == Ncc.Entities.Enum.StatusEnum.UserLevel.Intern_3);
                };
            });
        }


        [Fact]
        public async Task Should_Get_All_Interns_Infor_By_TrainerIds()
        {
            await WithUnitOfWorkAsync(async () =>
            {
                // Default DateFilterType field is OnBoardDate
                // Arrange
                var input = new InputInternInfoDto
                {
                    BasicTrainerIds = new System.Collections.Generic.List<long> { 3 },
                    StartDate = new DateTime(2022, 1, 1),
                    EndDate = new DateTime(2022, 12, 30),
                };
                var managerId3 = await _workScope.GetAsync<User>(3);

                // Act
                var result = _internsInfoAppService.GetAll(input);

                // Assert
                Assert.NotNull(result);
                foreach (var item in result.ListInternInfo.Items)
                {
                    item.BasicTrannerFullName.ShouldBe(managerId3.FullName);
                };
            });
        }

        [Fact]
        public async Task Should_Get_All_Interns_Infor_By_SearchText()
        {
            await WithUnitOfWorkAsync(async () =>
            {
                // Default DateFilterType field is OnBoardDate
                // Arrange
                var input = new InputInternInfoDto
                {
                    SearchText = "thanh",
                    StartDate = new DateTime(2022, 1, 1),
                    EndDate = new DateTime(2022, 12, 30),
                };
                var managerId3 = await _workScope.GetAsync<User>(3);

                // Act
                var result = _internsInfoAppService.GetAll(input);

                // Assert
                Assert.NotNull(result);
                result.ListInternInfo.Items.ShouldContain(item => item.MyInfo.EmailAddress.Contains("thanh"));
            });
        }

        [Fact]
        public async Task Should_Get_All_Interns_Infor_By_BranchIds()
        {
            await WithUnitOfWorkAsync(async () =>
            {
                // Default DateFilterType field is OnBoardDate
                // Arrange
                var input = new InputInternInfoDto
                {
                    BranchIds = new System.Collections.Generic.List<long> { 1, 2 },
                    StartDate = new DateTime(2022, 1, 1),
                    EndDate = new DateTime(2022, 12, 30),
                };

                var branchId1 = await _workScope.GetAsync<Branch>(1);
                var branchId2 = await _workScope.GetAsync<Branch>(2);

                // Act
                var result = _internsInfoAppService.GetAll(input);


                // Assert
                Assert.NotNull(result);
                result.ListInternInfo.Items.ShouldContain(item => item.MyInfo.BranchDisplayName == branchId1.DisplayName);
                result.ListInternInfo.Items.ShouldContain(item => item.MyInfo.BranchDisplayName == branchId2.DisplayName);
            });
        }

        [Fact]
        public async Task Should_Get_All_Basic_Trainer()
        {
            await WithUnitOfWorkAsync(async () =>
            {
                // Act
                var result = _internsInfoAppService.GetAllBasicTraner();

                // Assert
                result.Count.ShouldBe(2);
                result.ShouldContain(item => item.Id == 3);
                result.ShouldContain(item => item.FullName == "Tiến Nguyễn Hữu");
                result.ShouldContain(item => item.EmailAddress == "tien.nguyenhuu@ncc.asia");
                result.ShouldContain(item => item.Id == 4);
                result.ShouldContain(item => item.FullName == "Toại Nguyễn Công");
                result.ShouldContain(item => item.EmailAddress == "toai.nguyencong@ncc.asia");
            });
        }

        [Fact]
        public async Task Should_Get_All_Branch()
        {
            await WithUnitOfWorkAsync(async () =>
            {
                // Act
                var result = _internsInfoAppService.GetAllBranch();

                // Assert
                result.Count.ShouldBeGreaterThanOrEqualTo(8);
                result.ShouldContain(item => item.BranchId == 1);
                result.ShouldContain(item => item.BranchDisplayName == "HN2");
            });
        }
    }
}
