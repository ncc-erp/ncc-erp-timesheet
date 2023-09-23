
using Ncc.IoC;
using Shouldly;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Timesheet.APIs.HRMv2.Dto;
using Timesheet.APIs.OverTimeHours;
using Timesheet.Entities;
using Timesheet.Paging;
using Xunit;
using Ncc.Entities.Enum;
using SortDirection = Timesheet.Paging.SortDirection;
using static Ncc.Entities.Enum.StatusEnum;

namespace Timesheet.Application.Tests.API.OverTimeHour
{
    /// <summary>
    /// 6/6 function
    /// 11/11 test cases passed
    /// update day 10/01/2023
    /// </summary>
    public class OverTimeHourAppService_Tests : TimesheetApplicationTestBase
    {
        private List<MyTimesheet> listMyTimesheet = new List<MyTimesheet>
        {
            new MyTimesheet
            {
                Id=100,
                ProjectTaskId = 14,
                Note="",
                WorkingTime=480,
                TargetUserWorkingTime=0,
                TypeOfWork=TypeOfWork.OverTime,
                IsCharged=false,
                DateAt=new DateTime(2022,12,27),
                Status=TimesheetStatus.Approve,
                ProjectTargetUserId=1,
                IsTemp=false,
                UserId=6,
            },
            new MyTimesheet
            {
                Id=101,
                ProjectTaskId = 14,
                Note="",
                WorkingTime=480,
                TargetUserWorkingTime=0,
                TypeOfWork=TypeOfWork.OverTime,
                IsCharged=false,
                DateAt=new DateTime(2022,12,26),
                Status=TimesheetStatus.Approve,
                ProjectTargetUserId=1,
                IsTemp=false,
                UserId=9,
            },
            new MyTimesheet
            {
                Id=102,
                ProjectTaskId = 14,
                Note="",
                WorkingTime=480,
                TargetUserWorkingTime=0,
                TypeOfWork=TypeOfWork.OverTime,
                IsCharged=false,
                DateAt=new DateTime(2022,12,28),
                Status=TimesheetStatus.Approve,
                ProjectTargetUserId=1,
                IsTemp=false,
                UserId=9,
            },
            new MyTimesheet
            {
                Id=103,
                ProjectTaskId = 14,
                Note="",
                WorkingTime=480,
                TargetUserWorkingTime=0,
                TypeOfWork=TypeOfWork.OverTime,
                IsCharged=false,
                DateAt=new DateTime(2022,12,29),
                Status=TimesheetStatus.Approve,
                ProjectTargetUserId=1,
                IsTemp=false,
                UserId=13,
            },
            new MyTimesheet
            {
                Id=104,
                ProjectTaskId = 14,
                Note="",
                WorkingTime=480,
                TargetUserWorkingTime=0,
                TypeOfWork=TypeOfWork.OverTime,
                IsCharged=false,
                DateAt=new DateTime(2022,12,30),
                Status=TimesheetStatus.Approve,
                ProjectTargetUserId=1,
                IsTemp=false,
                UserId=13,
            },
            new MyTimesheet
            {
                Id=105,
                ProjectTaskId = 14,
                Note="",
                WorkingTime=240,
                TargetUserWorkingTime=0,
                TypeOfWork=TypeOfWork.OverTime,
                IsCharged=false,
                DateAt=new DateTime(2022,12,31),
                Status=TimesheetStatus.Approve,
                ProjectTargetUserId=1,
                IsTemp=false,
                UserId=1,
            },
            new MyTimesheet
            {
                Id=106,
                ProjectTaskId = 14,
                Note="",
                WorkingTime=240,
                TargetUserWorkingTime=0,
                TypeOfWork=TypeOfWork.OverTime,
                IsCharged=false,
                DateAt=new DateTime(2022,12,24),
                Status=TimesheetStatus.Approve,
                ProjectTargetUserId=1,
                IsTemp=false,
                UserId=1,
            },
            new MyTimesheet
            {
                Id=107,
                ProjectTaskId = 14,
                Note="",
                WorkingTime=480,
                TargetUserWorkingTime=0,
                TypeOfWork=TypeOfWork.OverTime,
                IsCharged=false,
                DateAt=new DateTime(2022,12,19),
                Status=TimesheetStatus.Approve,
                ProjectTargetUserId=1,
                IsTemp=false,
                UserId=1,
            },
            new MyTimesheet
            {
                Id=108,
                ProjectTaskId = 1,
                Note="",
                WorkingTime=240,
                TargetUserWorkingTime=0,
                TypeOfWork=TypeOfWork.OverTime,
                IsCharged=false,
                DateAt=new DateTime(2022,12,01),
                Status=TimesheetStatus.Approve,
                ProjectTargetUserId=1,
                IsTemp=false,
                UserId=10,
            },
        };

        private readonly OverTimeHourAppService _overTimeHour;
        private readonly IWorkScope _work;
        public OverTimeHourAppService_Tests()
        {
            _work = Resolve<WorkScope>();
            _overTimeHour = new OverTimeHourAppService(_work);
            foreach (var ts in listMyTimesheet)
            {
                _work.InsertAsync(ts);
            }
        }

        //Test Function GetAllPagging
        [Fact]
        public async Task Should_Get_all_Paging()
        {
            await WithUnitOfWorkAsync(async () =>
            {
                var inputGridParam = new GridParam { };
                var inputYear = 2022;
                var inputMonth = 12;
                var projectId = 5;

                var result = await _overTimeHour.GetAllPagging(inputGridParam, inputYear, inputMonth, projectId);

                Assert.Equal(4, result.TotalCount);
                result.Items.ShouldContain(x => x.UserId == 1);
                result.Items.ShouldContain(x => x.EmailAddress == "admin@aspnetboilerplate.com");
                result.Items.ShouldContain(x => x.EmailAddress == "testemail6@gmail.com");
                result.Items.ShouldContain(x => x.BranchId==1);
                result.Items.ShouldContain(x => x.Branch=="HN1");
            });
        }

        [Fact]
        public async Task Should_Get_all_Paging_With_Search_Text()
        {
            await WithUnitOfWorkAsync(async () =>
            {
                var inputGridParam = new GridParam
                {
                    SearchText = "gmail"
                };
                var inputYear = 2022;
                var inputMonth = 12;
                var projectId = 5;

                var result = await _overTimeHour.GetAllPagging(inputGridParam, inputYear, inputMonth, projectId);

                Assert.Equal(3, result.TotalCount);
                result.Items.ShouldContain(x => x.UserId == 9);
                result.Items.ShouldContain(x => x.UserId == 6);
                result.Items.ShouldContain(x => x.BranchId == 3);
                result.Items.ShouldContain(x => x.Branch == "HN3");
            });
        }

        [Fact]
        public async Task Should_Get_all_Paging_With_Sort_Default_ASC()
        {
            await WithUnitOfWorkAsync(async () =>
            {
                var inputGridParam = new GridParam
                {
                    Sort = "UserId"
                };
                var inputYear = 2022;
                var inputMonth = 12;
                var projectId = 5;

                var result = await _overTimeHour.GetAllPagging(inputGridParam, inputYear, inputMonth, projectId);

                Assert.Equal(4, result.TotalCount);
                Assert.Equal(1, result.Items[0].UserId);
                Assert.Equal(13, result.Items[3].UserId);
                result.Items.ShouldContain(x => x.UserId == 1);
                result.Items.ShouldContain(x => x.EmailAddress == "testemail6@gmail.com");
                result.Items.ShouldContain(x => x.BranchId == 1);
                result.Items.ShouldContain(x => x.Branch == "HN1");
            });
        }

        [Fact]
        public async Task Should_Get_all_Paging_With_Sort_And_Sort_Direction()
        {
            await WithUnitOfWorkAsync(async () =>
            {
                var inputGridParam = new GridParam
                {
                    Sort = "UserId",
                    SortDirection = SortDirection.DESC
                };
                var inputYear = 2022;
                var inputMonth = 12;
                var projectId = 5;

                var result = await _overTimeHour.GetAllPagging(inputGridParam, inputYear, inputMonth, projectId);

                Assert.Equal(4, result.TotalCount);
                Assert.Equal(13, result.Items[0].UserId);
                Assert.Equal(1, result.Items[3].UserId);
                result.Items.ShouldContain(x => x.UserId == 1);
                result.Items.ShouldContain(x => x.EmailAddress == "testemail6@gmail.com");
                result.Items.ShouldContain(x => x.BranchId == 1);
                result.Items.ShouldContain(x => x.Branch == "HN1");
            });
        }

        [Fact]
        public async Task Should_Get_all_Paging_With_Skip_Count()
        {
            await WithUnitOfWorkAsync(async () =>
            {
                var inputGridParam = new GridParam
                {
                    SkipCount = 2
                };
                var inputYear = 2022;
                var inputMonth = 12;
                var projectId = 5;

                var result = await _overTimeHour.GetAllPagging(inputGridParam, inputYear, inputMonth, projectId);

                Assert.Equal(4, result.TotalCount);
                Assert.Equal(2, result.Items.Count);
                result.Items.ShouldNotContain(x => x.UserId == 6);
                result.Items.ShouldNotContain(x => x.UserId == 9);
                result.Items.ShouldContain(x => x.UserId == 1);
                result.Items.ShouldContain(x => x.UserId == 13);
                result.Items.ShouldContain(x => x.EmailAddress == "admin@aspnetboilerplate.com");
                result.Items.ShouldContain(x => x.BranchId == 1);
                result.Items.ShouldContain(x => x.Branch == "HN1");
            });
        }

        [Fact]
        public async Task Should_Get_all_Paging_With_Max_Result_Count()
        {
            await WithUnitOfWorkAsync(async () =>
            {
                var inputGridParam = new GridParam
                {
                    MaxResultCount = 3
                };
                var inputYear = 2022;
                var inputMonth = 12;
                var projectId = 5;

                var result = await _overTimeHour.GetAllPagging(inputGridParam, inputYear, inputMonth, projectId);
                Assert.Equal(4, result.TotalCount);
                Assert.Equal(3, result.Items.Count);
                result.Items.ShouldNotContain(x => x.UserId == 1);
                result.Items.ShouldContain(x => x.UserId == 6);
                result.Items.ShouldContain(x => x.EmailAddress == "testemail6@gmail.com");
                result.Items.ShouldContain(x => x.BranchId == 3);
                result.Items.ShouldContain(x => x.Branch == "HN3");
            });
        }

        //Test Function GetAllOverTimeForHRM
        [Fact]
        public async Task Should_Get_Over_Time_For_HRM()
        {
            await WithUnitOfWorkAsync(async () =>
            {
                var inputYear = 2022;
                var inputMonth = 12;

                var result = await _overTimeHour.GetAllOverTimeForHRM(inputYear, inputMonth);

                Assert.Equal(5, result.Count);
                result.ShouldContain(x => x.NormalizedEmailAddress == "TESTEMAIL6@GMAIL.COM");
                result.ShouldContain(x => x.NormalizedEmailAddress == "TESTEMAIL13@GMAIL.COM");
                result.ShouldContain(x => x.NormalizedEmailAddress == "TESTEMAIL10@GMAIL.COM");
                result.ShouldContain(x => x.UserId == 1);
                result.ShouldContain(x => x.UserId == 10);
            });
        }

        //Test Function GetAllOverTimeForHRMv2
        [Fact]
        public async Task Should_Get_Over_Time_For_HRMv2()
        {
            await WithUnitOfWorkAsync(async () =>
            {
                var input = new InputCollectDataForPayslipDto
                {
                    Year = 2022,
                    Month = 12,
                    UpperEmails = new List<string>
                    {
                        "TESTEMAIL6@GMAIL.COM",
                        "TESTEMAIL13@GMAIL.COM",
                        "TESTEMAIL10@GMAIL.COM",
                        "TESTEMAIL17@GMAIL.COM"
                    }
                };

                var result = await _overTimeHour.GetAllOverTimeForHRMv2(input);

                Assert.Equal(3, result.Count);
                result.ShouldContain(x => x.NormalizedEmailAddress == "TESTEMAIL10@GMAIL.COM" && x.ListOverTimeHour.Count == 1);
                result.ShouldContain(x => x.NormalizedEmailAddress == "TESTEMAIL13@GMAIL.COM" && x.ListOverTimeHour.Count == 2);
            });
        }

        //Test Function GetListOverTimeForChart
        [Fact]
        public async Task Should_Get_List_Over_Time_For_Chart()
        {
            await WithUnitOfWorkAsync(async () =>
            {
                var inputStartDate = new DateTime(2022, 12, 1);
                var inputEndDate = new DateTime(2022, 12, 31);
                var inputProjectCode = "Project 3";
                var inputEmails = new List<string>
                {
                    "TESTEMAIL6@GMAIL.COM",
                    "TESTEMAIL13@GMAIL.COM",
                    "TESTEMAIL17@GMAIL.COM",
                    "ADMIN@ASPNETBOILERPLATE.COM"
                };

                var result = await _overTimeHour.GetListOverTimeForChart(inputStartDate, inputEndDate, inputProjectCode, inputEmails);

                Assert.Equal(3, result.Count);
                result.ShouldContain(x => x.UserId == 6);
                result.ShouldContain(x => x.EmailAddress == "testemail6@gmail.com");
                result.ShouldContain(x => x.NormalizedEmailAddress == inputEmails[0]);
                result.ShouldNotContain(x => x.NormalizedEmailAddress == inputEmails[2]);
            });
        }

        //Test Function GetProjectIdByCode
        [Fact]
        public async Task Should_Get_ProjectId_By_Code()
        {
            await WithUnitOfWorkAsync(async () =>
            {
                var inputProjectCode = "Project 3";
                var result = await _overTimeHour.GetProjectIdByCode(inputProjectCode);
                Assert.Equal(5, result);
            });
        }

        //Test Function GetProjectCodeById
        [Fact]
        public async Task Should_Get_ProjectCode_By_Id()
        {
            await WithUnitOfWorkAsync(async () =>
            {
                var inputProjectId = 5;
                var result = await _overTimeHour.GetProjectCodeById(inputProjectId);
                Assert.Equal("Project 3", result);
            });
        }
    }
}
