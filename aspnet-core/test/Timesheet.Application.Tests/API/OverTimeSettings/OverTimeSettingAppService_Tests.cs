using Abp.Application.Services.Dto;
using Abp.Configuration;
using Abp.Domain.Entities;
using Abp.ObjectMapping;
using Abp.UI;
using Microsoft.EntityFrameworkCore;
using Ncc.IoC;
using NPOI.SS.Formula.Functions;
using OfficeOpenXml.FormulaParsing.Excel.Functions.DateTime;
using Shouldly;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Timesheet.APIs.OverTimeSettings;
using Timesheet.APIs.OverTimeSettings.Dto;
using Timesheet.Entities;
using Timesheet.Paging;
using Timesheet.Uitls;
using Xunit;
using SortDirection = Timesheet.Paging.SortDirection;

namespace Timesheet.Application.Tests.API.OverTimeSettings
{   
    /// <summary>
    /// 3/3 Function
    /// 11/11 test cases passed
    /// update day 10/01/2023
    /// </summary>
    public class OverTimeSettingAppService_Tests : TimesheetApplicationTestBase
    {
        private readonly IWorkScope _work;
        private readonly OverTimeSettingAppService _overTimeSetting;
        public OverTimeSettingAppService_Tests()
        {
            _work = Resolve<IWorkScope>();
            _overTimeSetting = new OverTimeSettingAppService(_work);
            _overTimeSetting.SettingManager = Resolve<ISettingManager>();
            _overTimeSetting.ObjectMapper = Resolve<IObjectMapper>();
        }

        // Test Function GetAllPaging
        [Fact]
        public async Task Should_Get_All_Paging()
        {
            var inputGridParam = new GridParam { };
            var inputDateAt = new DateTime(2022, 12, 27);
            var inputProjectId = 1;
            await WithUnitOfWorkAsync(async () =>
            {
                var result = await _overTimeSetting.GetAllPagging(inputGridParam, inputDateAt, inputProjectId);
                Assert.Equal(4, result.TotalCount);
                result.Items.ShouldContain(x => x.Id == 1);
                result.Items.ShouldContain(x => x.Id == 3);
                result.Items.ShouldContain(x => x.ProjectId == inputProjectId);
                result.Items.ShouldContain(x => x.ProjectName == "Project UCG");
                result.Items.ShouldContain(x => x.DateAt == inputDateAt);
            });

        }

        [Fact]
        public async Task Should_Get_All_Paging_With_Search_Text()
        {
            await WithUnitOfWorkAsync(async () =>
            {
                var inputGridParam = new GridParam
                {
                    SearchText = "Project"
                };
                var inputDateAt = new DateTime(2022, 12, 27);
                var inputProjectId = 1;
                var result = await _overTimeSetting.GetAllPagging(inputGridParam, inputDateAt, inputProjectId);
                Assert.Equal(4, result.TotalCount);
                result.Items.ShouldContain(x => x.ProjectId == 1);
            });
        }

        [Fact]
        public async Task Should_Get_All_Paging_With_Sort_Default_ASC()
        {
            await WithUnitOfWorkAsync(async () =>
            {
                var inputGridParam = new GridParam
                {
                    Sort = "Id"
                };
                var inputDateAt = new DateTime(2022, 12, 27);
                var inputProjectId = 1;
                var result = await _overTimeSetting.GetAllPagging(inputGridParam, inputDateAt, inputProjectId);
                Assert.Equal(4, result.TotalCount);
                Assert.Equal(1, result.Items[0].Id);
                Assert.Equal(4, result.Items[3].Id);
                result.Items.ShouldContain(x => x.ProjectId == inputProjectId);
                result.Items.ShouldContain(x => x.ProjectName == "Project UCG");
                result.Items.ShouldContain(x => x.DateAt == inputDateAt);
            });
        }

        [Fact]
        public async Task Should_Get_All_Paging_With_Sort_And_Sort_Direction_DESC()
        {
            await WithUnitOfWorkAsync(async () =>
            {
                var inputGridParam = new GridParam
                {
                    Sort = "Id",
                    SortDirection = SortDirection.DESC
                };
                var inputDateAt = new DateTime(2022, 12, 27);
                var inputProjectId = 1;
                var result = await _overTimeSetting.GetAllPagging(inputGridParam, inputDateAt, inputProjectId);
                Assert.Equal(4, result.TotalCount);
                Assert.Equal(4, result.Items[0].Id);
                Assert.Equal(1, result.Items[3].Id);
                result.Items.ShouldContain(x => x.ProjectId == inputProjectId);
                result.Items.ShouldContain(x => x.ProjectName == "Project UCG");
                result.Items.ShouldContain(x => x.DateAt == inputDateAt);
            });
        }

        [Fact]
        public async Task Should_Get_All_Paging_With_Skip_Count()
        {
            await WithUnitOfWorkAsync(async () =>
            {
                var inputGridParam = new GridParam
                {
                    SkipCount = 2
                };
                var inputDateAt = new DateTime(2022, 12, 27);
                var inputProjectId = 1;
                var result = await _overTimeSetting.GetAllPagging(inputGridParam, inputDateAt, inputProjectId);
                Assert.Equal(2, result.Items.Count);
                result.Items.ShouldContain(x => x.Id == 3);
                result.Items.ShouldNotContain(x => x.Id == 1);
                result.Items.ShouldContain(x => x.ProjectId == inputProjectId);
                result.Items.ShouldContain(x => x.ProjectName == "Project UCG");
                result.Items.ShouldContain(x => x.DateAt == inputDateAt);
            });
        }

        [Fact]
        public async Task Should_Get_All_Paging_With_Max_Result_Count()
        {
            await WithUnitOfWorkAsync(async () =>
            {
                var inputGridParam = new GridParam
                {
                    MaxResultCount = 3
                };
                var inputDateAt = new DateTime(2022, 12, 27);
                var inputProjectId = 1;
                var result = await _overTimeSetting.GetAllPagging(inputGridParam, inputDateAt, inputProjectId);
                Assert.Equal(3, result.Items.Count);
                result.Items.ShouldContain(x => x.Id == 1);
                result.Items.ShouldNotContain(x => x.Id == 4);
                result.Items.ShouldContain(x => x.ProjectId == inputProjectId);
                result.Items.ShouldContain(x => x.ProjectName == "Project UCG");
                result.Items.ShouldContain(x => x.DateAt == inputDateAt);
            });
        }

        //Test Function Save
        // do sử dụng now.Day nên có 2 case now.Day < 5 và now.Day >=5 gộp chung vào 1 test
        // nếu tách ra sẽ có 1 case fail
        [Fact]
        public async Task Should_Not_Allow_Save_With_Lock_Timesheet_Late()
        {
            var now = DateTimeUtils.GetNow();
            if (now.Day < 5)
                await WithUnitOfWorkAsync(async () =>
                {
                    var input = new OverTimeSettingDto
                    {
                        Id = 1,
                        ProjectId = 1,
                        DateAt = new DateTime(now.Year, now.Month, 1).AddMonths(-2),
                        Coefficient = 1,
                        Note = "Nothing"
                    };
                    var exception = await Assert.ThrowsAsync<UserFriendlyException>(async () =>
                    {
                        await _overTimeSetting.Save(input);
                    });
                    var expectedMsg = string.Format("Cannot setting OT for day {0}", input.DateAt.ToString("MM/dd/yyyy"));
                    Assert.Equal(expectedMsg, exception.Message);
                });
            else
            {
                await WithUnitOfWorkAsync(async () =>
                {
                    var input = new OverTimeSettingDto
                    {
                        Id = 1,
                        ProjectId = 1,
                        DateAt = new DateTime(now.Year, now.Month, 1).AddMonths(-1),
                        Coefficient = 1,
                        Note = ""
                    };
                    var exception = await Assert.ThrowsAsync<UserFriendlyException>(async () =>
                    {
                        await _overTimeSetting.Save(input);
                    });
                    var expectedMsg = string.Format("Cannot setting OT for day {0}", input.DateAt.ToString("MM/dd/yyyy"));
                    Assert.Equal(expectedMsg, exception.Message);
                });
            }
        }

        [Fact]
        public async Task Should_Not_Allow_Save_With_Project_Not_Found()
        {
            await WithUnitOfWorkAsync(async () =>
            {
                var now = DateTimeUtils.GetNow();
                var input = new OverTimeSettingDto
                {
                    Id = 1,
                    ProjectId = 10,
                    DateAt = new DateTime(now.Year, now.Month, 1),
                    Coefficient = 1,
                    Note = ""
                };
                var exception = await Assert.ThrowsAsync<UserFriendlyException>(async () =>
                {
                    await _overTimeSetting.Save(input);
                });
                var expectedMsg = string.Format("Not found project");
                Assert.Equal(expectedMsg, exception.Message);
            });
        }

        // Gồm cả test case allow insert và exception OvertimeSetting for Project already Existed
        // do sử dụng DateTimeUtils.GetNow() và data không thay đổi nên khi test case exception Existed bi fail
        [Fact]
        public async Task Should_Allow_Create_With_New_Over_Time_Setting_Valid_And_Test_Exception_Existed()
        {
            var now = DateTimeUtils.GetNow();
            var input = new OverTimeSettingDto
            {
                Id = -1,
                ProjectId = 1,
                DateAt = new DateTime(now.Year, now.Month, 1),
                Coefficient = 1,
                Note = "Nothing"
            };
            var result = new OverTimeSettingDto { };
            await WithUnitOfWorkAsync(async () =>
            {
                result = await _overTimeSetting.Save(input);
                Assert.Equal(input.ProjectId, result.ProjectId);
                Assert.Equal(input.DateAt, result.DateAt);
                Assert.Equal(input.Coefficient, result.Coefficient);
                Assert.Equal(input.Note, result.Note);
            });
            await WithUnitOfWorkAsync(async () =>
            {
                var overTimeSettings = _work.GetAll<OverTimeSetting>();
                Assert.Equal(7, overTimeSettings.Count());
                Assert.Equal(-1, result.Id);
                var overTimeSetting = await _work.GetAsync<OverTimeSetting>(result.Id);
                Assert.Equal(input.ProjectId, overTimeSetting.ProjectId);
                Assert.Equal(input.DateAt, overTimeSetting.DateAt);
                Assert.Equal(input.Coefficient, overTimeSetting.Coefficient);
                Assert.Equal(input.Note, overTimeSetting.Note);
            });

            await WithUnitOfWorkAsync(async () =>
            {
                var inputException = new OverTimeSettingDto
                {
                    Id = -2,
                    ProjectId = 1,
                    DateAt = new DateTime(now.Year, now.Month, 1),
                    Coefficient = 1,
                    Note = "Nothing"
                };
                var exception = await Assert.ThrowsAsync<UserFriendlyException>(async () =>
                {
                    await _overTimeSetting.Save(inputException);
                });
                var projectName = await _work.GetAll<Ncc.Entities.Project>()
                    .Where(s => s.Id == input.ProjectId)
                    .Select(s => s.Name).FirstOrDefaultAsync();
                var expectedMsg = string.Format("OverTimeSetting at {0} of project {1} already existed ", input.DateAt.ToString("MM/dd/yyyy"), projectName); ;
                Assert.Equal(expectedMsg, exception.Message);
            });
        }

        [Fact]
        public async Task Should_Allow_Update_With_Over_Time_Setting_Valid()
        {
            var now = DateTimeUtils.GetNow();
            var input = new OverTimeSettingDto
            {
                Id = 1,
                ProjectId = 1,
                DateAt = new DateTime(now.Year, now.Month, 1),
                Coefficient = 3,
                Note = "Nothing"
            };
            var result = new OverTimeSettingDto { };
            await WithUnitOfWorkAsync(async () =>
            {
                result = await _overTimeSetting.Save(input);
                Assert.Equal(input.Id, result.Id);
                Assert.Equal(input.ProjectId, result.ProjectId);
                Assert.Equal(input.DateAt, result.DateAt);
                Assert.Equal(input.Coefficient, result.Coefficient);
                Assert.Equal(input.Note, result.Note);
            });
            await WithUnitOfWorkAsync(async () =>
            {
                var overTimeSetting = await _work.GetAsync<OverTimeSetting>(input.Id);
                Assert.Equal(input.ProjectId, overTimeSetting.ProjectId);
                Assert.Equal(input.DateAt, overTimeSetting.DateAt);
                Assert.Equal(input.Coefficient, overTimeSetting.Coefficient);
                Assert.Equal(input.Note, overTimeSetting.Note);
            });
        }

        //Test Function Delete
        [Fact]
        public async Task Should_Allow_Delete()
        {
            await WithUnitOfWorkAsync(async () =>
            {
                var inputId = new EntityDto<long>(1);
                _overTimeSetting.Delete(inputId);

            });
            await WithUnitOfWorkAsync(async () =>
            {
                var overTimeSettings = _work.GetAll<OverTimeSetting>();
                Assert.Equal(5, overTimeSettings.Count());
            });
        }
    }
}
