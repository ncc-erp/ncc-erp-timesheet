using Abp.Application.Services.Dto;
using Abp.Domain.Entities;
using Abp.ObjectMapping;
using Abp.UI;
using DocumentFormat.OpenXml.Spreadsheet;
using MassTransit.NewIdFormatters;
using Ncc.IoC;
using Shouldly;
using System;
using System.Collections.Generic;
using System.Linq.Dynamic.Core;
using System.Text;
using System.Threading.Tasks;
using Timesheet.APIs.Branchs;
using Timesheet.APIs.Branchs.Dto;
using Timesheet.Entities;
using Timesheet.Paging;
using Xunit;
using SortDirection = Timesheet.Paging.SortDirection;

namespace Timesheet.Application.Tests.API.Branchs
{
    /// <summary>
    /// 5/5 Function
    /// 16/16 test cases passed
    /// update day 10/01/2023
    /// </summary>
    public class BranchAppService_Tests : TimesheetApplicationTestBase
    {
        private readonly BranchAppService _branch;
        private readonly IWorkScope _work;
        public BranchAppService_Tests()
        {
            _work = Resolve<IWorkScope>();
            _branch = new BranchAppService(_work);
            _branch.ObjectMapper = Resolve<IObjectMapper>();
        }

        //Test Function GetAllPagging
        [Fact]
        public async Task Should_Get_All_Paging()
        {
            var inputGridParam = new GridParam { };
            await WithUnitOfWorkAsync(async () =>
            {
                var result = await _branch.GetAllPagging(inputGridParam);
                Assert.Equal(8, result.TotalCount);
                result.Items.ShouldContain(x => x.Id == 1);
                result.Items.ShouldContain(x => x.Code == "HN1");
                result.Items.ShouldContain(x => x.DisplayName == "HN1");
                result.Items.ShouldContain(x => x.Name =="HN1");
                result.Items.ShouldContain(x => x.MorningWorking==3.5);
                result.Items.ShouldContain(x => x.AfternoonWorking==4.5);
                result.Items.ShouldContain(x => x.MorningStartAt=="08:30");
                result.Items.ShouldContain(x => x.MorningEndAt=="12:00");
                result.Items.ShouldContain(x => x.AfternoonStartAt=="13:00");
                result.Items.ShouldContain(x => x.AfternoonEndAt=="17:30");
            });
        }

        [Fact]
        public async Task Should_Get_All_Paging_With_Search_Text()
        {
            var inputGridParam = new GridParam
            {
                SearchText = "HN"
            };
            await WithUnitOfWorkAsync(async () =>
            {
                var result = await _branch.GetAllPagging(inputGridParam);
                Assert.Equal(3, result.TotalCount);
                result.Items.ShouldContain(x => x.Id == 1);
                result.Items.ShouldContain(x => x.Id == 2);
                result.Items.ShouldContain(x => x.Id == 3);
                result.Items.ShouldContain(x => x.Code == "HN1");
                result.Items.ShouldContain(x => x.Code == "HN2");
                result.Items.ShouldContain(x => x.Code == "HN3");
                result.Items.ShouldContain(x => x.MorningWorking == 3.5);
                result.Items.ShouldContain(x => x.AfternoonWorking == 4.5);
                result.Items.ShouldContain(x => x.MorningStartAt == "08:30");
                result.Items.ShouldContain(x => x.MorningEndAt == "12:00");
                result.Items.ShouldContain(x => x.AfternoonStartAt == "13:00");
                result.Items.ShouldContain(x => x.AfternoonEndAt == "17:30");
            });
        }

        [Fact]
        public async Task Should_Get_All_Paging_With_Sort_Default_ASC()
        {
            var inputGridParam = new GridParam
            {
                Sort = "Code"
            };
            await WithUnitOfWorkAsync(async () =>
            {
                var result = await _branch.GetAllPagging(inputGridParam);
                Assert.Equal(8, result.TotalCount);
                Assert.Equal(6, result.Items[0].Id);
                Assert.Equal("ĐN", result.Items[0].Code);
                Assert.Equal("ĐN", result.Items[0].DisplayName);
                Assert.Equal(7, result.Items[7].Id);
                Assert.Equal("Vinh", result.Items[7].Code);
                Assert.Equal("Vinh", result.Items[7].DisplayName);
                result.Items.ShouldContain(x => x.MorningWorking == 3.5);
                result.Items.ShouldContain(x => x.AfternoonWorking == 4.5);
                result.Items.ShouldContain(x => x.MorningStartAt == "08:30");
                result.Items.ShouldContain(x => x.MorningEndAt == "12:00");
                result.Items.ShouldContain(x => x.AfternoonStartAt == "13:00");
                result.Items.ShouldContain(x => x.AfternoonEndAt == "17:30");
            });
        }

        [Fact]
        public async Task Should_Get_All_Paging_With_Sort_And_Sort_Direction_DESC()
        {
            var inputGridParam = new GridParam
            {
                Sort = "Code",
                SortDirection = SortDirection.DESC
            };
            await WithUnitOfWorkAsync(async () =>
            {
                var result = await _branch.GetAllPagging(inputGridParam);
                Assert.Equal(8, result.TotalCount);
                Assert.Equal(7, result.Items[0].Id);
                Assert.Equal("Vinh", result.Items[0].Code);
                Assert.Equal("Vinh", result.Items[0].DisplayName);
                Assert.Equal(6, result.Items[7].Id);
                Assert.Equal("ĐN", result.Items[7].Code);
                Assert.Equal("ĐN", result.Items[7].DisplayName);
                result.Items.ShouldContain(x => x.MorningWorking == 3.5);
                result.Items.ShouldContain(x => x.AfternoonWorking == 4.5);
                result.Items.ShouldContain(x => x.MorningStartAt == "08:30");
                result.Items.ShouldContain(x => x.MorningEndAt == "12:00");
                result.Items.ShouldContain(x => x.AfternoonStartAt == "13:00");
                result.Items.ShouldContain(x => x.AfternoonEndAt == "17:30");
            });
        }

        [Fact]
        public async Task Should_Get_All_Paging_With_Skip_Count()
        {
            var inputGridParam = new GridParam
            {
                SkipCount = 1,
            };
            await WithUnitOfWorkAsync(async () =>
            {
                var result = await _branch.GetAllPagging(inputGridParam);
                Assert.Equal(7, result.Items.Count);
                result.Items.ShouldNotContain(x => x.Id == 1);
                result.Items.ShouldNotContain(x => x.Code == "HN1");
                result.Items.ShouldNotContain(x => x.DisplayName == "HN1");
                result.Items.ShouldContain(x => x.Id == 2);
                result.Items.ShouldContain(x => x.Code == "HN2");
                result.Items.ShouldContain(x => x.DisplayName == "HN2");
                result.Items.ShouldContain(x => x.MorningWorking == 3.5);
                result.Items.ShouldContain(x => x.AfternoonWorking == 4.5);
                result.Items.ShouldContain(x => x.MorningStartAt == "08:30");
                result.Items.ShouldContain(x => x.MorningEndAt == "12:00");
                result.Items.ShouldContain(x => x.AfternoonStartAt == "13:00");
                result.Items.ShouldContain(x => x.AfternoonEndAt == "17:30");
            });
        }

        [Fact]
        public async Task Should_Get_All_Paging_With_Max_Result_Count()
        {
            var inputGridParam = new GridParam
            {
                MaxResultCount = 5,
            };
            await WithUnitOfWorkAsync(async () =>
            {
                var result = await _branch.GetAllPagging(inputGridParam);
                Assert.Equal(5, result.Items.Count);
                result.Items.ShouldNotContain(x => x.Id == 6);
                result.Items.ShouldNotContain(x => x.Code == "ĐN");
                result.Items.ShouldNotContain(x => x.DisplayName == "ĐN");
                result.Items.ShouldContain(x => x.Id == 1);
                result.Items.ShouldContain(x => x.Code == "HN1");
                result.Items.ShouldContain(x => x.DisplayName == "HN1");
                result.Items.ShouldContain(x => x.MorningWorking == 3.5);
                result.Items.ShouldContain(x => x.AfternoonWorking == 4.5);
                result.Items.ShouldContain(x => x.MorningStartAt == "08:30");
                result.Items.ShouldContain(x => x.MorningEndAt == "12:00");
                result.Items.ShouldContain(x => x.AfternoonStartAt == "13:00");
                result.Items.ShouldContain(x => x.AfternoonEndAt == "17:30");
            });
        }

        //Test Function GetAllNotPagging
        [Fact]
        public async Task Should_Get_All_Not_Paging()
        {
            await WithUnitOfWorkAsync(async () =>
            {
                var result = await _branch.GetAllNotPagging();
                Assert.Equal(8, result.Count);
                result.ShouldContain(x => x.Id == 1);
                result.ShouldContain(x => x.Id == 2);
                result.ShouldContain(x => x.Id == 3);
                result.ShouldContain(x => x.Name == "HN1");
                result.ShouldContain(x => x.Name == "HN2");
                result.ShouldContain(x => x.Name == "HN3");
                result.ShouldContain(x => x.MorningWorking == 3.5);
                result.ShouldContain(x => x.AfternoonWorking == 4.5);
                result.ShouldContain(x => x.MorningStartAt == "08:30");
                result.ShouldContain(x => x.MorningEndAt == "12:00");
                result.ShouldContain(x => x.AfternoonStartAt == "13:00");
                result.ShouldContain(x => x.AfternoonEndAt == "17:30");
            });
        }

        //Test Function Save
        [Fact]
        public async Task Should_Not_Allow_Save_With_Branch_Name_Already_Existed()
        {
            await WithUnitOfWorkAsync(async () =>
            {
                var input = new BranchCreateEditDto
                {
                    Id = 0,
                    Code = "HN",
                    Name = "HN1",
                    DisplayName = "HN",
                    Color = "#f44336",
                    MorningWorking = 3.5,
                    AfternoonWorking = 4.5,
                    MorningStartAt = "08:30",
                    MorningEndAt = "12:00",
                    AfternoonStartAt = "13:00",
                    AfternoonEndAt = "17:30"
                };

                var expectedMsg = string.Format("Branch name {0} or display name {1} or code {2} already existed ", input.Name, input.DisplayName, input.Code);
                var exception = await Assert.ThrowsAsync<UserFriendlyException>(async () =>
                {
                    await _branch.Save(input);
                });

                Assert.Equal(expectedMsg, exception.Message);
            });
        }

        [Fact]
        public async Task Should_Not_Allow_Save_With_Display_Name_Already_Existed()
        {
            await WithUnitOfWorkAsync(async () =>
            {
                var input = new BranchCreateEditDto
                {
                    Id = 0,
                    Code = "HN",
                    Name = "HN",
                    DisplayName = "HN1",
                    Color = "#f44336",
                    MorningWorking = 3.5,
                    AfternoonWorking = 4.5,
                    MorningStartAt = "08:30",
                    MorningEndAt = "12:00",
                    AfternoonStartAt = "13:00",
                    AfternoonEndAt = "17:30"
                };

                var expectedMsg = string.Format("Branch name {0} or display name {1} or code {2} already existed ", input.Name, input.DisplayName, input.Code);
                var exception = await Assert.ThrowsAsync<UserFriendlyException>(async () =>
                {
                    await _branch.Save(input);
                });

                Assert.Equal(expectedMsg, exception.Message);
            });
        }

        [Fact]
        public async Task Should_Not_Allow_Save_With_Code_Already_Existed()
        {
            await WithUnitOfWorkAsync(async () =>
            {
                var input = new BranchCreateEditDto
                {
                    Id = 0,
                    Code = "HN1",
                    Name = "HN",
                    DisplayName = "HN",
                    Color = "#f44336",
                    MorningWorking = 3.5,
                    AfternoonWorking = 4.5,
                    MorningStartAt = "08:30",
                    MorningEndAt = "12:00",
                    AfternoonStartAt = "13:00",
                    AfternoonEndAt = "17:30"
                };

                var expectedMsg = string.Format("Branch name {0} or display name {1} or code {2} already existed ", input.Name, input.DisplayName, input.Code);
                var exception = await Assert.ThrowsAsync<UserFriendlyException>(async () =>
                {
                    await _branch.Save(input);
                });

                Assert.Equal(expectedMsg, exception.Message);
            });
        }

        [Fact]
        public async Task Should_Allow_Create_With_New_Branch()
        {
            var input = new BranchCreateEditDto
            {
                Id = -1,
                Code = "HN4",
                Name = "HN4",
                DisplayName = "HN4",
                Color = "#f44336",
                MorningWorking = 3.5,
                AfternoonWorking = 4.5,
                MorningStartAt = "08:30",
                MorningEndAt = "12:00",
                AfternoonStartAt = "13:00",
                AfternoonEndAt = "17:30"
            };
            await WithUnitOfWorkAsync(async () =>
            {
                var result = await _branch.Save(input);
                Assert.Equal(input.Code, result.Code);
                Assert.Equal(input.Name, result.Name);
                Assert.Equal(input.DisplayName, result.DisplayName);
                Assert.Equal(input.Color, result.Color);
                Assert.Equal(input.MorningWorking, result.MorningWorking);
                Assert.Equal(input.AfternoonWorking, result.AfternoonWorking);
                Assert.Equal(input.MorningStartAt, result.MorningStartAt);
                Assert.Equal(input.MorningEndAt, result.MorningEndAt);
                Assert.Equal(input.AfternoonStartAt, result.AfternoonStartAt);
                Assert.Equal(input.AfternoonEndAt, result.AfternoonEndAt);
            });

            await WithUnitOfWorkAsync(async () =>
            {
                var branchs = _work.GetAll<Branch>();
                Assert.Equal(9, branchs.Count());
                branchs.ShouldContain(x => x.Code == input.Code);
                branchs.ShouldContain(x => x.Name == input.Name);
                branchs.ShouldContain(x => x.DisplayName == input.DisplayName);
                branchs.ShouldContain(x => x.MorningWorking == 3.5);
                branchs.ShouldContain(x => x.AfternoonWorking == 4.5);
                branchs.ShouldContain(x => x.MorningStartAt == "08:30");
                branchs.ShouldContain(x => x.MorningEndAt == "12:00");
                branchs.ShouldContain(x => x.AfternoonStartAt == "13:00");
                branchs.ShouldContain(x => x.AfternoonEndAt == "17:30");
            });
        }

        [Fact]
        public async Task Should_Allow_Update_With_Branch_Already_Existed()
        {

            var input = new BranchCreateEditDto
            {
                Id = 1,
                Code = "HN4",
                Name = "HN4",
                DisplayName = "HN4",
                Color = "#f44336",
                MorningWorking = 3.5,
                AfternoonWorking = 4.5,
                MorningStartAt = "09:00",
                MorningEndAt = "12:00",
                AfternoonStartAt = "13:00",
                AfternoonEndAt = "18:00"
            };
            await WithUnitOfWorkAsync(async () =>
            {
                var result = await _branch.Save(input);
                Assert.Equal(input.Code, result.Code);
                Assert.Equal(input.Name, result.Name);
                Assert.Equal(input.DisplayName, result.DisplayName);
                Assert.Equal(input.Color, result.Color);
                Assert.Equal(input.MorningWorking, result.MorningWorking);
                Assert.Equal(input.AfternoonWorking, result.AfternoonWorking);
                Assert.Equal(input.MorningStartAt, result.MorningStartAt);
                Assert.Equal(input.MorningEndAt, result.MorningEndAt);
                Assert.Equal(input.AfternoonStartAt, result.AfternoonStartAt);
                Assert.Equal(input.AfternoonEndAt, result.AfternoonEndAt);
            });

            await WithUnitOfWorkAsync(async () =>
            {
                var branchs = _work.GetAll<Branch>();
                Assert.Equal(8, branchs.Count());
                branchs.ShouldNotContain(x => x.Name == "HN1");
                branchs.ShouldNotContain(x => x.Code == "HN1");
                branchs.ShouldNotContain(x => x.DisplayName == "HN1");
                var branchExisted = await _work.GetAsync<Branch>(input.Id);
                Assert.Equal(input.Code, branchExisted.Code);
                Assert.Equal(input.Name, branchExisted.Name);
                Assert.Equal(input.DisplayName, branchExisted.DisplayName);
                Assert.Equal(input.Color, branchExisted.Color);
                Assert.Equal(input.MorningWorking, branchExisted.MorningWorking);
                Assert.Equal(input.AfternoonWorking, branchExisted.AfternoonWorking);
                Assert.Equal(input.MorningStartAt, branchExisted.MorningStartAt);
                Assert.Equal(input.MorningEndAt, branchExisted.MorningEndAt);
                Assert.Equal(input.AfternoonStartAt, branchExisted.AfternoonStartAt);
                Assert.Equal(input.AfternoonEndAt, branchExisted.AfternoonEndAt);
            });
        }

        // Test Function Delete
        [Fact]
        public async Task Should_Not_Allow_Delete_With_Branch_Has_User()
        {
            
            var input = new EntityDto<long>(1);
            await WithUnitOfWorkAsync(async () =>
            {
                var expectedMsg = String.Format("Branch Id {0} has user", input.Id);
                var exception = await Assert.ThrowsAsync<UserFriendlyException>(async () =>
                {
                    await _branch.Delete(input);
                });
                Assert.Equal(expectedMsg, exception.Message);
            });
        }

        [Fact]
        public async Task Should_Allow_Delete()
        {
            var newBranch = new BranchCreateEditDto
            {
                Id = -1,
                Code = "HN4",
                Name = "HN4",
                DisplayName = "HN4",
                Color = "#f44336",
                MorningWorking = 3.5,
                AfternoonWorking = 4.5,
                MorningStartAt = "09:00",
                MorningEndAt = "12:00",
                AfternoonStartAt = "13:00",
                AfternoonEndAt = "18:00"
            };
            await WithUnitOfWorkAsync(async () =>
            {
                var result = await _branch.Save(newBranch);
            });
            var input = new EntityDto<long>(-1);
            await WithUnitOfWorkAsync(async () =>
            {
                var branchs = _work.GetAll<Branch>();
                Assert.Equal(9, branchs.Count());
                await _branch.Delete(input);
            });
            await WithUnitOfWorkAsync(async () =>
            {
                var branchs = _work.GetAll<Branch>();
                Assert.Equal(8, branchs.Count());
                branchs.ShouldNotContain(x => x.Id == input.Id);
                branchs.ShouldNotContain(x => x.Name == "HN4");
            });
        }

        //Test Function GetAllBranchFilter
        [Fact]
        public async Task Should_Get_All_Branch_With_Input_Is_True()
        {
            await WithUnitOfWorkAsync(async () =>
            {
                var input = true;
                var result = await _branch.GetAllBranchFilter(input);
                Assert.Equal(9, result.Count);
                result.ShouldContain(x => x.Id == 1);
                result.ShouldContain(x => x.Name == "HN1");
                result.ShouldContain(x => x.DisplayName == "HN1");
                result.ShouldContain(x => x.Id == 0);
                result.ShouldContain(x => x.Name == "All");
                result.ShouldContain(x => x.DisplayName == "All");
            });
        }

        [Fact]
        public async Task Should_Get_All_Branch_With_Input_Is_False()
        {
            await WithUnitOfWorkAsync(async () =>
            {
                var result = await _branch.GetAllBranchFilter();
                Assert.Equal(8, result.Count);
                result.ShouldContain(x => x.Id == 1);
                result.ShouldContain(x => x.Name == "HN1");
                result.ShouldContain(x => x.DisplayName == "HN1");
            });
        }

    }
}
