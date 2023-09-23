
using Abp.BackgroundJobs;
using Abp.Configuration;
using Abp.Domain.Entities;
using Abp.ObjectMapping;
using Abp.Runtime.Session;
using Abp.UI;
using DocumentFormat.OpenXml.Office2010.Excel;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Ncc.Authorization.Users;
using Ncc.Configuration;
using Ncc.IoC;
using NSubstitute;
using Shouldly;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Net.Http;
using System.Threading.Tasks;
using Timesheet.APIs.Timekeepings.Dto;
using Timesheet.APIs.Timesheets.MyTimesheets.Dto;
using Timesheet.APIs.Timesheets.Timesheets.Dto;
using Timesheet.DomainServices;
using Timesheet.DomainServices.Dto;
using Timesheet.Entities;
using Timesheet.Services.Komu;
using Timesheet.Timesheets.MyTimesheets;
using Timesheet.Timesheets.MyTimesheets.Dto;
using Timesheet.Timesheets.Timesheets.Dto;
using Timesheet.Uitls;
using Xunit;
using static Ncc.Entities.Enum.StatusEnum;

namespace Timesheet.Application.Tests.API.MyTimesheets
{
    /// <summary>
    /// 18/18 Function 
    /// 48/51 test cases passed 3 cases ignore (comment) 
    /// update day 16/01/2023
    /// </summary>
    public class MyTimesheetsAppService_Tests : TimesheetApplicationTestBase
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
            new MyTimesheet
            {
                Id=109,
                ProjectTaskId = 14,
                Note="",
                WorkingTime=480,
                TargetUserWorkingTime=0,
                TypeOfWork=TypeOfWork.NormalWorkingHours,
                IsCharged=false,
                DateAt=new DateTime(2022,12,30),
                Status=TimesheetStatus.Pending,
                ProjectTargetUserId=1,
                IsTemp=false,
                UserId=1,
            },
            new MyTimesheet
            {
                Id=110,
                ProjectTaskId = 14,
                Note="",
                WorkingTime=240,
                TargetUserWorkingTime=0,
                TypeOfWork=TypeOfWork.NormalWorkingHours,
                IsCharged=false,
                DateAt=new DateTime(2022,12,14),
                Status=TimesheetStatus.None,
                ProjectTargetUserId=1,
                IsTemp=false,
                UserId=1,
            },
            new MyTimesheet
            {
                Id=111,
                ProjectTaskId = 14,
                Note="",
                WorkingTime=300,
                TargetUserWorkingTime=0,
                TypeOfWork=TypeOfWork.NormalWorkingHours,
                IsCharged=false,
                DateAt=new DateTime(2022,12,26),
                Status=TimesheetStatus.None,
                ProjectTargetUserId=1,
                IsTemp=false,
                UserId=1,
            },
            new MyTimesheet
            {
                Id=112,
                ProjectTaskId = 14,
                Note="",
                WorkingTime=480,
                TargetUserWorkingTime=0,
                TypeOfWork=TypeOfWork.NormalWorkingHours,
                IsCharged=false,
                DateAt=new DateTime(2022,12,15),
                Status=TimesheetStatus.Reject,
                ProjectTargetUserId=1,
                IsTemp=false,
                UserId=1,
            },
        };
        private readonly MyTimesheetsAppService _myTimesheet;
        private readonly IWorkScope _work;
        public MyTimesheetsAppService_Tests()
        {
            var _httpClient = Resolve<HttpClient>();
            var _logger = Resolve<ILogger<KomuService>>();
            var _config = Substitute.For<IConfiguration>();
            var _settingManager = Substitute.For<ISettingManager>();
            _config.GetValue<string>("KomuService:DevModeChannelId").Returns("DevModeChannelId");
            _config.GetValue<string>("KomuService:EnableKomuNotification").Returns("true");
            _config.GetValue<string>("KomuService:BaseAddress").Returns("http://example.com");
            _config.GetValue<string>("KomuService:SecurityCode").Returns("SecurityCode");

            var _backgroundJobManager = Substitute.For<IBackgroundJobManager>();
            var _commonServices = Substitute.For<ICommonServices>();
            var _komuService = Substitute.For<KomuService>(_httpClient, _logger, _config, _settingManager);
            var _abpSession = Substitute.For<IAbpSession>();
            _work = Resolve<IWorkScope>();
            _myTimesheet = new MyTimesheetsAppService(_backgroundJobManager, _komuService, _commonServices, _work);
            _myTimesheet.AbpSession = Resolve<IAbpSession>();

            _myTimesheet.SettingManager = Resolve<ISettingManager>();
            _myTimesheet.ObjectMapper = Resolve<IObjectMapper>();
            foreach (var ts in listMyTimesheet)
            {
                _work.InsertAsync(ts);
            }
        }

        //Test Function GetTimesheetStatisticMembers
        [Fact]
        public async Task Should_Get_Timesheet_Statistic_Members()
        {
            var inputStartDate = new DateTime(2022, 12, 1);
            var inputEndDate = new DateTime(2022, 12, 31);
            await WithUnitOfWorkAsync(async () =>
            {
                var result = await _myTimesheet.GetTimesheetStatisticMembers(inputStartDate, inputEndDate);
                Assert.Equal(6, result.Count);
                result.ShouldContain(x => x.UserID == 17);
                result.ShouldContain(x => x.UserName == "email17 test");
                result.ShouldContain(x => x.TotalWorkingTime == 6240);
                result.ShouldContain(x => x.BillableWorkingTime == 6240);
            });
        }

        //Test Function GetTimesheetStatisticTasks
        [Fact]
        public async Task Should_Get_Timesheet_Statistic_Task()
        {
            var inputStartDate = new DateTime(2022, 12, 1);
            var inputEndDate = new DateTime(2022, 12, 31);
            await WithUnitOfWorkAsync(async () =>
            {
                var result = await _myTimesheet.GetTimesheetStatisticTasks(inputStartDate, inputEndDate);
                Assert.Equal(2, result.Count);
                result.ShouldContain(x => x.TaskId == 1);
                result.ShouldContain(x => x.TaskName == "Coding");
                result.ShouldContain(x => x.TotalWorkingTime == 7410);
                result.ShouldContain(x => x.BillableWorkingTime == 7170);
            });
        }

        //Test Function GetTimesheetStatisticProjects
        [Fact]
        public async Task Should_Get_Timesheet_Statistic_Projects()
        {
            var inputStartDate = new DateTime(2022, 12, 1);
            var inputEndDate = new DateTime(2022, 12, 31);
            await WithUnitOfWorkAsync(async () =>
            {
                var result = await _myTimesheet.GetTimesheetStatisticProjects(inputStartDate, inputEndDate);
                Assert.Equal(2, result.Count);
                result.ShouldContain(x => x.ProjectId == 5);
                result.ShouldContain(x => x.ProjectName == "Project 3");
                result.ShouldContain(x => x.TotalWorkingTime == 12090);
                result.ShouldContain(x => x.BillableWorkingTime == 8730);
            });
        }

        //Test Function GetTimesheetStatisticClients
        [Fact]
        public async Task Should_Get_Timesheet_Statistic_Clients()
        {
            var inputStartDate = new DateTime(2022, 12, 1);
            var inputEndDate = new DateTime(2022, 12, 31);
            await WithUnitOfWorkAsync(async () =>
            {
                var result = await _myTimesheet.GetTimesheetStatisticClients(inputStartDate, inputEndDate);
                Assert.Equal(2, result.Count);
                result.ShouldContain(x => x.CustomerId == 3);
                result.ShouldContain(x => x.CustomerName == "Client 3");
                result.ShouldContain(x => x.TotalWorkingTime == 12090);
                result.ShouldContain(x => x.BillableWorkingTime == 8730);
            });
        }

        //Test Function GetAllTimesheetOfUser
        [Fact]
        public async Task Should_Get_All_Timesheet_Of_User()
        {
            var inputStartDate = new DateTime(2022, 12, 1);
            var inputEndDate = new DateTime(2022, 12, 31);
            await WithUnitOfWorkAsync(async () =>
            {
                var result = await _myTimesheet.GetAllTimesheetOfUser(inputStartDate, inputEndDate);
                Assert.Equal(7, result.Count);
                result.ShouldContain(x => x.Id == 105);
                result.ShouldContain(x => x.CustomerName == "Client 3");
                result.ShouldContain(x => x.DateAt == new DateTime(2022, 12, 31));
                result.ShouldContain(x => x.ProjectCode == "Project 3");
                result.ShouldContain(x => x.ProjectName == "Project 3");
                result.ShouldContain(x => x.Status == Ncc.Entities.Enum.StatusEnum.TimesheetStatus.Approve);
                result.ShouldContain(x => x.TaskName == "Testing");
                result.ShouldContain(x => x.WorkingTime == 240);
                result.ShouldContain(x => x.ProjectTaskId == 14);
                result.ShouldContain(x => x.Note == "");
                result.ShouldContain(x => x.TypeOfWork == Ncc.Entities.Enum.StatusEnum.TypeOfWork.OverTime);
                result.ShouldContain(x => x.IsCharged == false);
                result.ShouldContain(x => x.Billable == true);
                result.ShouldContain(x => x.IsTemp == false);
            });
        }

        //Test Function GetTimesheetReportHours
        [Fact]
        public async Task Should_Get_Timesheet_Report_Hours()
        {
            var inputStartDate = new DateTime(2022, 12, 1);
            var inputEndDate = new DateTime(2022, 12, 31);
            await WithUnitOfWorkAsync(async () =>
            {
                var result = await _myTimesheet.GetTimesheetReportHours(inputStartDate, inputEndDate);
                Assert.Equal(11010, result.Billable);
                Assert.Equal(3600, result.NonBillable);
                Assert.Equal(11010, result.NormalWorkingHours);
                Assert.Equal(0, result.OvertimeBillable);
                Assert.Equal(3600, result.OvertimeNonBillable);
                Assert.Equal(14610, result.HoursTracked);
            });
        }

        //Test Function Create
        [Fact]
        public async Task Should_Not_Allow_Create_With_Working_Time_Invalid()
        {
            var input = new MyTimesheetDto
            {
                ProjectTaskId = 1,
                Note = "",
                WorkingTime = -1,
                TargetUserWorkingTime = 1,
                TypeOfWork = Ncc.Entities.Enum.StatusEnum.TypeOfWork.NormalWorkingHours,
                IsCharged = true,
                DateAt = new DateTime(2022, 12, 1),
                Status = Ncc.Entities.Enum.StatusEnum.TimesheetStatus.All,
                ProjectTargetUserId = 1,
                IsTemp = true,
            };

            await WithUnitOfWorkAsync(async () =>
            {
                var expectedMsg = "You can't log this time sheet with working time < 0";
                var exception = await Assert.ThrowsAsync<UserFriendlyException>(async () =>
                {
                    await _myTimesheet.Create(input);
                });
                Assert.Equal(expectedMsg, exception.Message);
            });
        }

        [Fact]
        public async Task Should_Not_Allow_Create_With_Timesheet_Locked()
        {
            var input = new MyTimesheetDto
            {
                ProjectTaskId = 1,
                Note = "",
                WorkingTime = 240,
                TargetUserWorkingTime = 1,
                TypeOfWork = Ncc.Entities.Enum.StatusEnum.TypeOfWork.NormalWorkingHours,
                IsCharged = true,
                DateAt = new DateTime(2022, 12, 1),
                Status = Ncc.Entities.Enum.StatusEnum.TimesheetStatus.All,
                ProjectTargetUserId = 1,
                IsTemp = true,
            };

            await WithUnitOfWorkAsync(async () =>
            {
                var expectedMsg = "Timesheet was locked! You can log timesheet begin :";
                var exception = await Assert.ThrowsAsync<UserFriendlyException>(async () =>
                {
                    await _myTimesheet.Create(input);
                });
                Assert.Contains(expectedMsg, exception.Message);
            });
        }

        [Fact]
        public async Task Should_Not_Allow_Create_With_Log_Timesheet_In_Future()
        {
            var canLogTimesheetInFuture = await _myTimesheet.SettingManager.GetSettingValueAsync(AppSettingNames.LogTimesheetInFuture);
            var now = DateTimeUtils.GetNow();
            var input = new MyTimesheetDto
            {
                ProjectTaskId = 1,
                Note = "",
                WorkingTime = 240,
                TargetUserWorkingTime = 1,
                TypeOfWork = Ncc.Entities.Enum.StatusEnum.TypeOfWork.NormalWorkingHours,
                IsCharged = true,
                DateAt = new DateTime(now.Year, now.Month, now.Day).AddMonths(5),
                Status = Ncc.Entities.Enum.StatusEnum.TimesheetStatus.All,
                ProjectTargetUserId = 1,
                IsTemp = true,
            };

            if (canLogTimesheetInFuture.Equals("false"))
            {
                await WithUnitOfWorkAsync(async () =>
                {
                    var expectedMsg = "You can't log time sheet for the future";
                    var exception = await Assert.ThrowsAsync<UserFriendlyException>(async () =>
                    {
                        await _myTimesheet.Create(input);
                    });
                    Assert.Contains(expectedMsg,exception.Message);
                });
            }
            else
            {
                await WithUnitOfWorkAsync(async () =>
                {
                    var expectedMsg = "You can't log time sheet with date > ";
                    var exception = await Assert.ThrowsAsync<UserFriendlyException>(async () =>
                    {
                        await _myTimesheet.Create(input);
                    });
                    Assert.Contains(expectedMsg,exception.Message);
                });
            }
        }

        [Fact]
        public async Task Should_Not_Allow_Create_With_Total_Working_Time_Invalid()
        {
            var now = DateTimeUtils.GetNow();
            var input = new MyTimesheetDto
            {
                ProjectTaskId = 1,
                Note = "",
                WorkingTime = 10000,
                TargetUserWorkingTime = 1,
                TypeOfWork = Ncc.Entities.Enum.StatusEnum.TypeOfWork.NormalWorkingHours,
                IsCharged = true,
                DateAt = new DateTime(now.Year, now.Month, now.Day),
                Status = Ncc.Entities.Enum.StatusEnum.TimesheetStatus.All,
                ProjectTargetUserId = 1,
                IsTemp = true,
            };

            await WithUnitOfWorkAsync(async () =>
            {
                var expectedMsg = string.Format($"total working time on {input.DateAt.ToString("yyyy-MM-dd")} can't  >");
                var exception = await Assert.ThrowsAsync<UserFriendlyException>(async () =>
                {
                    await _myTimesheet.Create(input);
                });
                Assert.Contains(expectedMsg, exception.Message);
            });
        }

        //TODO: Không cho log timesheet vào các ngày khác ngoài ngày hôm nay nên k có data test
        //[Fact]
        //public async Task Should_Not_Allow_Create_With_Over_Time_Invalid()
        //{
        //    var now = DateTimeUtils.GetNow();
        //    var input = new MyTimesheetDto
        //    {
        //        ProjectTaskId = 14,
        //        Note = "",
        //        WorkingTime = 240,
        //        TargetUserWorkingTime = 1,
        //        TypeOfWork = Ncc.Entities.Enum.StatusEnum.TypeOfWork.OverTime,
        //        IsCharged = false,
        //        DateAt = new DateTime(now.Year, now.Month, now.Day).AddDays((double)(6 - now.DayOfWeek)),
        //        Status = Ncc.Entities.Enum.StatusEnum.TimesheetStatus.All,
        //        ProjectTargetUserId = 1,
        //        IsTemp = false,
        //    };
        //    await WithUnitOfWorkAsync(async () =>
        //    {
        //        var expectedMsg = "Saturday morning is NORMAL WORKING. You have to log 4h NORMAL WORKING first. So, the rest";
        //        var exception = await Assert.ThrowsAsync<UserFriendlyException>(async () =>
        //        {
        //            await _myTimesheet.Create(input);
        //        });
        //        Assert.Contains(exception.Message, expectedMsg);
        //    });
        //}

        [Fact]
        public async Task Should_Not_Allow_Create_With_Project_Task_Not_Found()
        {
            var now = DateTimeUtils.GetNow();
            var input = new MyTimesheetDto
            {
                ProjectTaskId = 100,
                Note = "",
                WorkingTime = 480,
                TargetUserWorkingTime = 1,
                TypeOfWork = Ncc.Entities.Enum.StatusEnum.TypeOfWork.NormalWorkingHours,
                IsCharged = true,
                DateAt = new DateTime(now.Year, now.Month, now.Day),
                Status = Ncc.Entities.Enum.StatusEnum.TimesheetStatus.All,
                ProjectTargetUserId = 1,
                IsTemp = true,
            };

            await WithUnitOfWorkAsync(async () =>
            {
                var expectedMsg = $"Not found ProjectTask by Id {input.ProjectTaskId}";
                var exception = await Assert.ThrowsAsync<UserFriendlyException>(async () =>
                {
                    await _myTimesheet.Create(input);
                });
                Assert.Equal(expectedMsg, exception.Message);
            });
        }

        [Fact]
        public async Task Should_Allow_Create_With_MyTimesheet_Valid()
        {
            var now = DateTimeUtils.GetNow();
            var input = new MyTimesheetDto
            {
                ProjectTaskId = 14,
                Note = "",
                WorkingTime = 480,
                TargetUserWorkingTime = 1,
                TypeOfWork = Ncc.Entities.Enum.StatusEnum.TypeOfWork.NormalWorkingHours,
                IsCharged = false,
                DateAt = new DateTime(now.Year, now.Month, now.Day),
                Status = Ncc.Entities.Enum.StatusEnum.TimesheetStatus.All,
                ProjectTargetUserId = 1,
                IsTemp = false,
            };
            var result = new MyTimesheetDto { };
            await WithUnitOfWorkAsync(async () =>
            {
                result = await _myTimesheet.Create(input);
                Assert.Equal(input.ProjectTaskId, result.ProjectTaskId);
                Assert.Equal(input.Note, result.Note);
                Assert.Equal(input.WorkingTime, result.WorkingTime);
                Assert.Equal(input.TargetUserWorkingTime, result.TargetUserWorkingTime);
                Assert.Equal(input.TypeOfWork, result.TypeOfWork);
                Assert.Equal(input.DateAt, result.DateAt);
                Assert.Equal(input.ProjectTargetUserId, result.ProjectTargetUserId);
                Assert.Equal(input.IsTemp, result.IsTemp);
            });

            await WithUnitOfWorkAsync(async () =>
            {
                var myTimesheets = _work.GetAll<MyTimesheet>();
                Assert.Equal(99, myTimesheets.Count());
                var myTimesheet = await _work.GetAsync<MyTimesheet>(result.Id);
                Assert.Equal(input.ProjectTaskId, myTimesheet.ProjectTaskId);
                Assert.Equal(input.Note, myTimesheet.Note);
                Assert.Equal(input.WorkingTime, myTimesheet.WorkingTime);
                Assert.Equal(input.TargetUserWorkingTime, myTimesheet.TargetUserWorkingTime);
                Assert.Equal(input.TypeOfWork, myTimesheet.TypeOfWork);
                Assert.Equal(input.DateAt, myTimesheet.DateAt);
                Assert.Equal(input.ProjectTargetUserId, myTimesheet.ProjectTargetUserId);
                Assert.Equal(input.IsTemp, myTimesheet.IsTemp);
                Assert.Equal(1, myTimesheet.UserId);
            });
        }

        //Test Function Update
        [Fact]
        public async Task Should_Not_Allow_Update_With_Working_Time_Invalid()
        {
            var now = DateTimeUtils.GetNow();
            var input = new MyTimesheetDto
            {
                Id = 58,
                ProjectTaskId = 14,
                Note = "",
                WorkingTime = -1,
                TargetUserWorkingTime = 1,
                TypeOfWork = Ncc.Entities.Enum.StatusEnum.TypeOfWork.NormalWorkingHours,
                IsCharged = false,
                DateAt = new DateTime(now.Year, now.Month, now.Day),
                Status = Ncc.Entities.Enum.StatusEnum.TimesheetStatus.All,
                ProjectTargetUserId = 1,
                IsTemp = false,
            };
            await WithUnitOfWorkAsync(async () =>
            {
                await WithUnitOfWorkAsync(async () =>
                {
                    var expectedMsg = "You can't log this time sheet";
                    var exception = await Assert.ThrowsAsync<UserFriendlyException>(async () =>
                    {
                        await _myTimesheet.Update(input);
                    });
                    Assert.Contains(exception.Message, expectedMsg);
                });
            });
        }

        [Fact]
        public async Task Should_Not_Allow_Update_With_Timesheet_Locked()
        {
            var now = DateTimeUtils.GetNow();
            var input = new MyTimesheetDto
            {
                Id = 62,
                ProjectTaskId = 14,
                Note = "",
                WorkingTime = 480,
                TargetUserWorkingTime = 1,
                TypeOfWork = Ncc.Entities.Enum.StatusEnum.TypeOfWork.NormalWorkingHours,
                IsCharged = false,
                DateAt = new DateTime(now.Year, now.Month, now.Day).AddMonths(-1),
                Status = Ncc.Entities.Enum.StatusEnum.TimesheetStatus.All,
                ProjectTargetUserId = 1,
                IsTemp = false,
            };
            await WithUnitOfWorkAsync(async () =>
            {
                await WithUnitOfWorkAsync(async () =>
                {
                    var expectedMsg = "Timesheet was locked! You can log timesheet begin :";
                    var exception = await Assert.ThrowsAsync<UserFriendlyException>(async () =>
                    {
                        await _myTimesheet.Update(input);
                    });
                    Assert.Contains(expectedMsg, exception.Message);
                });
            });
        }

        [Fact]
        public async Task Should_Not_Allow_Update_With_Timesheet_Not_Existed()
        {
            var now = DateTimeUtils.GetNow();
            var input = new MyTimesheetDto
            {
                Id = 1000,
                ProjectTaskId = 14,
                Note = "",
                WorkingTime = 480,
                TargetUserWorkingTime = 1,
                TypeOfWork = Ncc.Entities.Enum.StatusEnum.TypeOfWork.NormalWorkingHours,
                IsCharged = false,
                DateAt = new DateTime(now.Year, now.Month, now.Day),
                Status = Ncc.Entities.Enum.StatusEnum.TimesheetStatus.All,
                ProjectTargetUserId = 1,
                IsTemp = false,
            };
            await WithUnitOfWorkAsync(async () =>
            {
                await WithUnitOfWorkAsync(async () =>
                {
                    var exception = await Assert.ThrowsAsync<EntityNotFoundException>(async () =>
                    {
                        await _myTimesheet.Update(input);
                    });
                });
            });
        }

        [Fact]
        public async Task Should_Not_Allow_Update_With_Timesheet_Other_People()
        {
            var now = DateTimeUtils.GetNow();
            var input = new MyTimesheetDto
            {
                Id = 50,
                ProjectTaskId = 14,
                Note = "",
                WorkingTime = 480,
                TargetUserWorkingTime = 1,
                TypeOfWork = Ncc.Entities.Enum.StatusEnum.TypeOfWork.NormalWorkingHours,
                IsCharged = false,
                DateAt = new DateTime(now.Year, now.Month, now.Day),
                Status = Ncc.Entities.Enum.StatusEnum.TimesheetStatus.All,
                ProjectTargetUserId = 1,
                IsTemp = false,
            };
            await WithUnitOfWorkAsync(async () =>
            {
                await WithUnitOfWorkAsync(async () =>
                {
                    var expectedMsg = string.Format("You can't update other people's timesheet");
                    var exception = await Assert.ThrowsAsync<UserFriendlyException>(async () =>
                    {
                        await _myTimesheet.Update(input);
                    });
                    Assert.Equal(exception.Message, expectedMsg);
                });
            });
        }

        [Fact]
        public async Task Should_Not_Allow_Update_With_Timesheet_Status_Approved()
        {
            var now = DateTimeUtils.GetNow();
            var input = new MyTimesheetDto
            {
                Id = 106,
                ProjectTaskId = 14,
                Note = "",
                WorkingTime = 480,
                TargetUserWorkingTime = 1,
                TypeOfWork = Ncc.Entities.Enum.StatusEnum.TypeOfWork.NormalWorkingHours,
                IsCharged = false,
                DateAt = new DateTime(now.Year, now.Month, now.Day),
                Status = Ncc.Entities.Enum.StatusEnum.TimesheetStatus.All,
                ProjectTargetUserId = 1,
                IsTemp = false,
            };
            await WithUnitOfWorkAsync(async () =>
            {
                await WithUnitOfWorkAsync(async () =>
                {
                    var expectedMsg = string.Format("You can't update approved Timesheet");
                    var exception = await Assert.ThrowsAsync<UserFriendlyException>(async () =>
                    {
                        await _myTimesheet.Update(input);
                    });
                    Assert.Equal(exception.Message, expectedMsg);
                });
            });
        }

        [Fact]
        public async Task Should_Not_Allow_Update_With_Log_Timesheet_Saturday()
        {
            var now = DateTimeUtils.GetNow();
            var input = new MyTimesheetDto
            {
                Id = 109,
                ProjectTaskId = 14,
                Note = "",
                WorkingTime = 240,
                TargetUserWorkingTime = 1,
                TypeOfWork = Ncc.Entities.Enum.StatusEnum.TypeOfWork.OverTime,
                IsCharged = false,
                DateAt = new DateTime(now.Year, now.Month, now.Day).AddDays((double)(6 - now.DayOfWeek)),
                Status = Ncc.Entities.Enum.StatusEnum.TimesheetStatus.All,
                ProjectTargetUserId = 1,
                IsTemp = false,
            };
            await WithUnitOfWorkAsync(async () =>
            {
                await WithUnitOfWorkAsync(async () =>
                {
                    var expectedMsg = $"Saturday morning is NORMAL WORKING. You have to log 4h NORMAL WORKING first.";
                    var exception = await Assert.ThrowsAsync<UserFriendlyException>(async () =>
                    {
                        await _myTimesheet.Update(input);
                    });
                    Assert.Contains(exception.Message, expectedMsg);
                });
            });
        }

        [Fact]
        public async Task Should_Not_Allow_Update_With_Total_Log_Timesheet_Invalid()
        {
            var now = DateTimeUtils.GetNow();
            var input = new MyTimesheetDto
            {
                Id = 109,
                ProjectTaskId = 14,
                Note = "",
                WorkingTime = 100000,
                TargetUserWorkingTime = 1,
                TypeOfWork = Ncc.Entities.Enum.StatusEnum.TypeOfWork.NormalWorkingHours,
                IsCharged = false,
                DateAt = new DateTime(now.Year, now.Month, now.Day),
                Status = Ncc.Entities.Enum.StatusEnum.TimesheetStatus.All,
                ProjectTargetUserId = 1,
                IsTemp = false,
            };
            await WithUnitOfWorkAsync(async () =>
            {
                await WithUnitOfWorkAsync(async () =>
                {
                    var expectedMsg = string.Format($"total working time on {input.DateAt.ToString("yyyy-MM-dd")}");
                    var exception = await Assert.ThrowsAsync<UserFriendlyException>(async () =>
                    {
                        await _myTimesheet.Update(input);
                    });
                    Assert.Contains(expectedMsg, exception.Message);
                });
            });
        }

        [Fact]
        public async Task Should_Allow_Update_With_MyTimesheet_Valid()
        {
            var now = DateTimeUtils.GetNow();
            var input = new MyTimesheetDto
            {
                Id = 109,
                ProjectTaskId = 14,
                Note = "",
                WorkingTime = 480,
                TargetUserWorkingTime = 1,
                TypeOfWork = Ncc.Entities.Enum.StatusEnum.TypeOfWork.NormalWorkingHours,
                IsCharged = false,
                DateAt = new DateTime(now.Year, now.Month, now.Day),
                Status = Ncc.Entities.Enum.StatusEnum.TimesheetStatus.All,
                ProjectTargetUserId = 1,
                IsTemp = false,
            };
            var result = new MyTimesheetDto { };
            await WithUnitOfWorkAsync(async () =>
            {
                result = await _myTimesheet.Update(input);
                Assert.Equal(input.Id, result.Id);
                Assert.Equal(input.ProjectTaskId, result.ProjectTaskId);
                Assert.Equal(input.Note, result.Note);
                Assert.Equal(input.WorkingTime, result.WorkingTime);
                Assert.Equal(input.TargetUserWorkingTime, result.TargetUserWorkingTime);
                Assert.Equal(input.TypeOfWork, result.TypeOfWork);
                Assert.Equal(input.DateAt, result.DateAt);
                Assert.Equal(input.ProjectTargetUserId, result.ProjectTargetUserId);
                Assert.Equal(input.IsTemp, result.IsTemp);
            });

            await WithUnitOfWorkAsync(async () =>
            {
                var myTimesheets = _work.GetAll<MyTimesheet>();
                Assert.Equal(98, myTimesheets.Count());
                var myTimesheet = await _work.GetAsync<MyTimesheet>(input.Id);
                Assert.Equal(input.ProjectTaskId, myTimesheet.ProjectTaskId);
                Assert.Equal(input.Note, myTimesheet.Note);
                Assert.Equal(input.WorkingTime, myTimesheet.WorkingTime);
                Assert.Equal(input.TargetUserWorkingTime, myTimesheet.TargetUserWorkingTime);
                Assert.Equal(input.TypeOfWork, myTimesheet.TypeOfWork);
                Assert.Equal(input.DateAt, myTimesheet.DateAt);
                Assert.Equal(input.ProjectTargetUserId, myTimesheet.ProjectTargetUserId);
                Assert.Equal(input.IsTemp, myTimesheet.IsTemp);
            });
        }

        //Test Function Get
        [Fact]
        public async Task Should_Get_By_Id()
        {
            var input = 1;
            await WithUnitOfWorkAsync(async () =>
            {
                var result = await _myTimesheet.Get(input);
                Assert.Equal(input, result.Id);
                Assert.Equal(14, result.ProjectTaskId);
                Assert.Equal(new DateTime(2022, 12, 27), result.DateAt);
                Assert.Null(result.Note);
                Assert.Equal(Ncc.Entities.Enum.StatusEnum.TimesheetStatus.Pending, result.Status);
                Assert.Equal(480, result.WorkingTime);
                Assert.Equal(Ncc.Entities.Enum.StatusEnum.TypeOfWork.NormalWorkingHours, result.TypeOfWork);
                Assert.False(result.IsCharged);
                Assert.False(result.IsTemp);
            });
        }

        //Test function SaveList
        [Fact]
        public async Task Should_Allow_Save_List()
        {
            var now = DateTimeUtils.GetNow();
            var input = new List<MyTimesheetDto>{
                new MyTimesheetDto
                {
                    ProjectTaskId = 14,
                    Note = "",
                    WorkingTime = 500,
                    TargetUserWorkingTime = 1,
                    TypeOfWork = Ncc.Entities.Enum.StatusEnum.TypeOfWork.NormalWorkingHours,
                    IsCharged = false,
                    DateAt = new DateTime(now.Year, now.Month, now.Day),
                    Status = Ncc.Entities.Enum.StatusEnum.TimesheetStatus.All,
                    ProjectTargetUserId = 1,
                    IsTemp = false,
                },
                new MyTimesheetDto
                {
                    ProjectTaskId = 13,
                    Note = "",
                    WorkingTime = 480,
                    TargetUserWorkingTime = 1,
                    TypeOfWork = Ncc.Entities.Enum.StatusEnum.TypeOfWork.NormalWorkingHours,
                    IsCharged = false,
                    DateAt = new DateTime(now.Year, now.Month, now.Day),
                    Status = Ncc.Entities.Enum.StatusEnum.TimesheetStatus.All,
                    ProjectTargetUserId = 1,
                    IsTemp = false,
                }
            };
            await WithUnitOfWorkAsync(async () =>
            {
                var result = await _myTimesheet.SaveList(input);
                Assert.Equal(2, result.Count);
            });

            await WithUnitOfWorkAsync(async () =>
            {
                var myTimesheets = _work.GetAll<MyTimesheet>();
                Assert.Equal(100, myTimesheets.Count());
                var myTimesheet = await _work.GetAsync<MyTimesheet>(input[0].Id);
                Assert.Equal(input[0].ProjectTaskId, myTimesheet.ProjectTaskId);
                Assert.Equal(input[0].Note, myTimesheet.Note);
                Assert.Equal(input[0].WorkingTime, myTimesheet.WorkingTime);
                Assert.Equal(input[0].TargetUserWorkingTime, myTimesheet.TargetUserWorkingTime);
                Assert.Equal(input[0].TypeOfWork, myTimesheet.TypeOfWork);
                Assert.Equal(input[0].DateAt, myTimesheet.DateAt);
                Assert.Equal(input[0].ProjectTargetUserId, myTimesheet.ProjectTargetUserId);
                Assert.Equal(input[0].IsTemp, myTimesheet.IsTemp);

            });
        }

        //Test Function Delete
        [Fact]
        public async Task Should_Not_Allow_Delete_With_MyTimesheet_Not_Exist()
        {
            await WithUnitOfWorkAsync(async () =>
            {
                var input = 1000;
                var exception = await Assert.ThrowsAsync<EntityNotFoundException>(async () =>
                {
                    await _myTimesheet.Delete(input);
                });
            });
        }

        [Fact]
        public async Task Should_Not_Allow_Delete_With_MyTimesheet_Status_Approved()
        {
            await WithUnitOfWorkAsync(async () =>
            {
                var input = 100;
                var expectedMsg = String.Format("MyTimeSheet Id {0} is approved", input);
                var exception = await Assert.ThrowsAsync<UserFriendlyException>(async () =>
                {
                    await _myTimesheet.Delete(input);
                });
                Assert.Equal(expectedMsg, exception.Message);
            });
        }

        [Fact]
        public async Task Should_Allow_Delete()
        {
            await WithUnitOfWorkAsync(async () =>
            {
                var input = 109;
                await _myTimesheet.Delete(input);
            });
            await WithUnitOfWorkAsync(async () =>
            {
                var myTimesheets = _work.GetAll<MyTimesheet>();
                Assert.Equal(97, myTimesheets.Count());
            });
        }

        //Test Function SubmitToPending
        //TODO: Loi funtion get LockDate in Function SubmitToPending
        //[Fact]
        //public async Task Should_Not_Allow_Submit_To_Pending_When_Not_Unlock_Timesheet()
        //{
        //    var input = new StartEndDateDto
        //    {
        //        StartDate = new DateTime(2022, 12, 1),
        //        EndDate = new DateTime(2022, 12, 20)
        //    };
        //    await WithUnitOfWorkAsync(async () =>
        //    {
        //        var expectedMsg = "Go to ims.nccsoft.vn > Unlock timesheet";

        //        var exception = await Assert.ThrowsAsync<UserFriendlyException>(async () =>
        //        {
        //            await _myTimesheet.SubmitToPending(input);
        //        });
        //        Assert.Equal(expectedMsg, exception.Message);
        //    });
        //}

        [Fact]
        public async Task Should_Allow_Submit_To_Pending()
        {
            var input = new StartEndDateDto
            {
                StartDate = new DateTime(2022, 12, 1),
                EndDate = new DateTime(2022, 12, 20)
            };
            var unlockTimesheet = new UnlockTimesheet
            {
                Id = 10,
                UserId = 1,
                Type = LockUnlockTimesheetType.MyTimesheet
            };
            await WithUnitOfWorkAsync(async () =>
            {
                await _work.InsertAsync(unlockTimesheet);
            });
            await WithUnitOfWorkAsync(async () =>
            {
                var unLockTimesheetExists = _work.GetAll<UnlockTimesheet>()
                .Where(x => x.UserId == 1)
                .ToList();
                Assert.Single(unLockTimesheetExists);
                var mytimesheets = await _work.GetAll<MyTimesheet>()
                .Where(s => s.UserId == AbpSession.UserId.Value)
                .Where(s => s.DateAt >= input.StartDate.Date && s.DateAt.Date <= input.EndDate)
                .Where(s => s.Status == TimesheetStatus.None)
                .ToListAsync();
                var expectedResult = "Submit success " + mytimesheets.Count + " timesheets";
                var result = await _myTimesheet.SubmitToPending(input);
                Assert.Equal(expectedResult, result);
            });
            await WithUnitOfWorkAsync(async () =>
            {
                var myTimesheet = await _work.GetAsync<MyTimesheet>(110);
                Assert.Equal(TimesheetStatus.Pending, myTimesheet.Status);
            });
        }

        //Test function getReceiverList
        [Fact]
        public async Task Should_Get_Receiver_List()
        {
            var input = new List<MyTimesheet>
            {
                new MyTimesheet
                {
                    Id=104,
                },
                new MyTimesheet
                {
                    Id=108,
                },
                new MyTimesheet
                {
                    Id=109,
                }
            };

            await WithUnitOfWorkAsync(async () =>
            {
                var result = await _myTimesheet.getReceiverList(input);
                Assert.Equal(2, result.Count());
                result.ShouldContain(x => x.IsNotifyKomu == false);
                result.ShouldContain(x => x.ProjectId == 5);
                result.ShouldContain(x => x.ProjectCode == "Project 3");
                result.ShouldContain(x => x.ProjectName == "Project 3");
                result.ShouldContain(x => x.Emails.Count == 2);
                result.ShouldContain(x => x.PMs.Count == 2);
            });
        }

        //Test function SaveAndReset
        [Fact]
        public async Task Should_Not_Allow_Save_And_Reset_With_Working_Time_Invalid()
        {
            var now = DateTimeUtils.GetNow();
            var input = new MyTimesheetDto
            {
                Id = 60,
                ProjectTaskId = 14,
                Note = "",
                WorkingTime = -1,
                TargetUserWorkingTime = 1,
                TypeOfWork = Ncc.Entities.Enum.StatusEnum.TypeOfWork.NormalWorkingHours,
                IsCharged = false,
                DateAt = new DateTime(now.Year, now.Month, now.Day),
                Status = Ncc.Entities.Enum.StatusEnum.TimesheetStatus.All,
                ProjectTargetUserId = 1,
                IsTemp = false,
            };
            await WithUnitOfWorkAsync(async () =>
            {
                var expectedMsg = "You can't log this time sheet";
                var exception = await Assert.ThrowsAsync<UserFriendlyException>(async () =>
                {
                    await _myTimesheet.SaveAndReset(input);
                });
                Assert.Equal(expectedMsg, exception.Message);
            });
        }

        [Fact]
        public async Task Should_Not_Allow_Save_And_Reset_With_Timesheet_Not_Exist()
        {
            var now = DateTimeUtils.GetNow();
            var input = new MyTimesheetDto
            {
                Id = 1000,
                ProjectTaskId = 14,
                Note = "",
                WorkingTime = 480,
                TargetUserWorkingTime = 1,
                TypeOfWork = Ncc.Entities.Enum.StatusEnum.TypeOfWork.NormalWorkingHours,
                IsCharged = false,
                DateAt = new DateTime(now.Year, now.Month, now.Day),
                Status = Ncc.Entities.Enum.StatusEnum.TimesheetStatus.All,
                ProjectTargetUserId = 1,
                IsTemp = false,
            };
            await WithUnitOfWorkAsync(async () =>
            {
                var exception = await Assert.ThrowsAsync<EntityNotFoundException>(async () =>
                {
                    await _myTimesheet.SaveAndReset(input);
                });
            });
        }

        [Fact]
        public async Task Should_Not_Allow_Save_And_Reset_With_Status_Not_Reject()
        {
            var now = DateTimeUtils.GetNow();
            var input = new MyTimesheetDto
            {
                Id = 105,
                ProjectTaskId = 14,
                Note = "",
                WorkingTime = 480,
                TargetUserWorkingTime = 1,
                TypeOfWork = Ncc.Entities.Enum.StatusEnum.TypeOfWork.NormalWorkingHours,
                IsCharged = false,
                DateAt = new DateTime(now.Year, now.Month, now.Day),
                Status = Ncc.Entities.Enum.StatusEnum.TimesheetStatus.All,
                ProjectTargetUserId = 1,
                IsTemp = false,
            };
            await WithUnitOfWorkAsync(async () =>
            {
                var expectedMsg = string.Format("MyTimesheet Id {0} is not exist or status is not rejected.", input.Id);
                var exception = await Assert.ThrowsAsync<UserFriendlyException>(async () =>
                {
                    await _myTimesheet.SaveAndReset(input);
                });
                Assert.Equal(expectedMsg, exception.Message);
            });
        }

        [Fact]
        public async Task Should_Not_Allow_Save_And_Reset_With_Log_Timesheet_Saturday()
        {
            var now = DateTimeUtils.GetNow();
            var input = new MyTimesheetDto
            {
                Id = 112,
                ProjectTaskId = 14,
                Note = "",
                WorkingTime = 240,
                TargetUserWorkingTime = 1,
                TypeOfWork = Ncc.Entities.Enum.StatusEnum.TypeOfWork.OverTime,
                IsCharged = false,
                DateAt = new DateTime(now.Year, now.Month, now.Day).AddDays((double)(6 - now.DayOfWeek)),
                Status = Ncc.Entities.Enum.StatusEnum.TimesheetStatus.All,
                ProjectTargetUserId = 1,
                IsTemp = false,
            };
            await WithUnitOfWorkAsync(async () =>
            {
                var expectedMsg = $"Saturday morning is NORMAL WORKING. You have to log 4h NORMAL WORKING first.";
                var exception = await Assert.ThrowsAsync<UserFriendlyException>(async () =>
                {
                    await _myTimesheet.SaveAndReset(input);
                });
                Assert.Equal(expectedMsg, exception.Message);
            });
        }

        [Fact]
        public async Task Should_Not_Allow_Save_And_Reset_With_Total_Log_Timesheet_Invalid()
        {
            var now = DateTimeUtils.GetNow();
            var input = new MyTimesheetDto
            {
                Id = 112,
                ProjectTaskId = 14,
                Note = "",
                WorkingTime = 100000,
                TargetUserWorkingTime = 1,
                TypeOfWork = Ncc.Entities.Enum.StatusEnum.TypeOfWork.NormalWorkingHours,
                IsCharged = false,
                DateAt = new DateTime(now.Year, now.Month, now.Day),
                Status = Ncc.Entities.Enum.StatusEnum.TimesheetStatus.All,
                ProjectTargetUserId = 1,
                IsTemp = false,
            };
            await WithUnitOfWorkAsync(async () =>
            {
                var expectedMsg = $"total working time on {input.DateAt.ToString("yyyy-MM-dd")} can't";
                var exception = await Assert.ThrowsAsync<UserFriendlyException>(async () =>
                {
                    await _myTimesheet.SaveAndReset(input);
                });
                Assert.Contains(expectedMsg, exception.Message);
            });
        }

        [Fact]
        public async Task Should_Allow_Save_And_Reset_With_Timesheet_Valid()
        {
            var now = DateTimeUtils.GetNow();
            var input = new MyTimesheetDto
            {
                Id = 112,
                ProjectTaskId = 14,
                Note = "",
                WorkingTime = 240,
                TargetUserWorkingTime = 0,
                TypeOfWork = Ncc.Entities.Enum.StatusEnum.TypeOfWork.NormalWorkingHours,
                IsCharged = false,
                DateAt = new DateTime(now.Year, now.Month, now.Day),
                ProjectTargetUserId = 1,
                IsTemp = false,
            };
            await WithUnitOfWorkAsync(async () =>
            {
                var result = await _myTimesheet.SaveAndReset(input);
                Assert.Equal(input.Id, result.Id);
                Assert.Equal(input.ProjectTaskId, result.ProjectTaskId);
                Assert.Equal(input.Note, result.Note);
                Assert.Equal(input.WorkingTime, result.WorkingTime);
                Assert.Equal(input.TypeOfWork, result.TypeOfWork);
                Assert.Equal(input.IsCharged, result.IsCharged);
                Assert.Equal(input.DateAt, result.DateAt);
                Assert.Equal(input.ProjectTargetUserId, result.ProjectTargetUserId);
                Assert.Equal(input.IsTemp, result.IsTemp);
            });

            await WithUnitOfWorkAsync(async () =>
            {
                var myTimesheet = await _work.GetAsync<MyTimesheet>(112);
                Assert.Equal(input.Id, myTimesheet.Id);
                Assert.Equal(input.ProjectTaskId, myTimesheet.ProjectTaskId);
                Assert.Equal(input.Note, myTimesheet.Note);
                Assert.Equal(input.WorkingTime, myTimesheet.WorkingTime);
                Assert.Equal(input.TypeOfWork, myTimesheet.TypeOfWork);
                Assert.Equal(input.IsCharged, myTimesheet.IsCharged);
                Assert.Equal(input.DateAt, myTimesheet.DateAt);
                Assert.Equal(input.ProjectTargetUserId, myTimesheet.ProjectTargetUserId);
                Assert.Equal(input.IsTemp, myTimesheet.IsTemp);
                Assert.Equal(TimesheetStatus.Pending, myTimesheet.Status);
            });
        }

        //Test function CreateByKumo
        [Fact]
        public async Task Should_Not_Allow_Create_By_Komu_With_User_Not_Exist()
        {
            var input = new MyTimesheetByKomuDto
            {
                EmailAddress = "usernotexist@gmail.com",
                Note = "Nothing",
                Hour = 10
            };
            await WithUnitOfWorkAsync(async () =>
            {
                var expected = "Failed! Not found user with emailAddress = " + input.EmailAddress;
                var result = await _myTimesheet.CreateByKomu(input);
                Assert.Equal(expected, result);
            });
        }

        [Fact]
        public async Task Should_Not_Allow_Create_By_Komu_With_User_Inactive()
        {          
            var input = new MyTimesheetByKomuDto
            {
                EmailAddress = "testemail23@gmail.com",
                Note = "Nothing",
                Hour = 10
            };
            var user = new User
            {
                Id = 100,
                EmailAddress = "testemail23@gmail.com",
                IsActive = false
            };
            await WithUnitOfWorkAsync(async () =>
            {
                await _work.InsertAsync(user);
            });

            await WithUnitOfWorkAsync(async () =>
            {
                var expected = "Failed! User is inactive with emailAddress = " + input.EmailAddress;
                var result = await _myTimesheet.CreateByKomu(input);
                Assert.Equal(expected, result);
            });
        }


        [Fact]
        public async Task Should_Not_Allow_Create_By_Komu_With_Not_Set_Project()
        {
            var input = new MyTimesheetByKomuDto
            {
                EmailAddress = "testemail22@gmail.com",
                Note = "Nothing",
                Hour = 10
            };
            await WithUnitOfWorkAsync(async () =>
            {
                var expected = "Failed! Not set Project Task Default yet. Go to timesheet.nccsoft.vn > My timesheet > Add new timesheet to set it.";
                var result = await _myTimesheet.CreateByKomu(input);
                Assert.Equal(expected, result);
            });
        }

        [Fact]
        public async Task Should_Not_Allow_Create_By_Komu_With_Project_Task_Not_Exist()
        {
            var input = new MyTimesheetByKomuDto
            {
                EmailAddress = "testemail23@gmail.com",
                Note = "Nothing",
                Hour = 10
            };
            var user = new User
            {
                Id = 100,
                EmailAddress = "testemail23@gmail.com",
                IsActive = true,
                DefaultProjectTaskId= 100,
            };
            await WithUnitOfWorkAsync(async () =>
            {
                await _work.InsertAsync(user);
            });
            await WithUnitOfWorkAsync(async () =>
            {
                var expected = "Fail! Not found projectTask in DB with Id " + user.DefaultProjectTaskId;
                var result = await _myTimesheet.CreateByKomu(input);
                Assert.Equal(expected, result);
            });
        }

        [Fact]
        public async Task Should_Not_Allow_Create_By_Komu_With_Project_Is_Closed()
        {
            var input = new MyTimesheetByKomuDto
            {
                EmailAddress = "testemail23@gmail.com",
                Note = "Nothing",
                Hour = 10
            };
            var user = new User
            {
                Id = 100,
                EmailAddress = "testemail23@gmail.com",
                IsActive = true,
                DefaultProjectTaskId = 14,
            };
            await WithUnitOfWorkAsync(async () =>
            {
                await _work.InsertAsync(user);
            });
            await WithUnitOfWorkAsync(async () =>
            {
                var expected = "Fail! User is not in this project or project is closed";
                var result = await _myTimesheet.CreateByKomu(input);
                Assert.Equal(expected, result);
            });
        }

        [Fact]
        public async Task Should_Not_Allow_Create_By_Komu_With_Timesheet_Approved()
        {
            var input = new MyTimesheetByKomuDto
            {
                EmailAddress = "testemail17@gmail.com",
                Note = "Nothing",
                DateAt = new DateTime(2022, 12, 26),
                Hour = 10
            };
            await WithUnitOfWorkAsync(async () =>
            {
                var expected = "Fail! Timesheet của bạn đã được Approve";
                var result = await _myTimesheet.CreateByKomu(input);
                Assert.Equal(expected, result);
            });
        }

        [Fact]
        public async Task Should_Allow_Create_By_Komu()
        {
            var input = new MyTimesheetByKomuDto
            {
                EmailAddress = "testemail17@gmail.com",
                Note = "Nothing",
                DateAt = new DateTime(2023,1,1 ),
                Hour = 10
            };
            await WithUnitOfWorkAsync(async () =>
            {
                var expected = "Success! Bạn đã log: **10h** task: **Testing** project: **Project 3** ngày **2023-01-01** với note là : **Nothing**";
                var result = await _myTimesheet.CreateByKomu(input);
                Assert.Contains(expected, result);
            });

            await WithUnitOfWorkAsync(async () =>
            {
                var myTimesheets = _work.GetAll<MyTimesheet>();
                Assert.Equal(99, myTimesheets.Count());
            });
        }

        [Fact]
        public async Task Should_Allow_Update_By_Komu()
        {
            var input = new MyTimesheetByKomuDto
            {
                EmailAddress = "testemail17@gmail.com",
                Note = "Nothing",
                DateAt = new DateTime(2022, 12,27),
                Hour = 10
            };
            await WithUnitOfWorkAsync(async () =>
            {
                var expected = "Success! Bạn đã log: **10h** task: **Testing** project: **Project 3** ngày **2022-12-27** với note là : **Nothing**";
                var result = await _myTimesheet.CreateByKomu(input);
                Assert.Contains(expected, result);
            });

            await WithUnitOfWorkAsync(async () =>
            {
                var myTimesheets = _work.GetAll<MyTimesheet>();
                Assert.Equal(98, myTimesheets.Count());
            });
        }

        //Test Function CreateFullByKomu
        [Fact]
        public async Task Should_Not_Allow_Create_Full_By_Komu_With_User_Not_Exist()
        {
            var input = new MyFullTimesheetByKomuDto
            {
                EmailAddress = "usernotexist@gmail.com",
                ProjectCode = "Project 3",
                TaskName = "Testing",
                Hour = 10,
                Note = "Nothing"
            };
            await WithUnitOfWorkAsync(async () =>
            {
                var expected = "Fail! Not found user with emailAddress = " + input.EmailAddress;
                var result = await _myTimesheet.CreateFullByKomu(input);
                Assert.Equal(expected, result);
            });
        }

        [Fact]
        public async Task Should_Not_Allow_Create_Full_By_Komu_With_User_Inactive()
        {
            var input = new MyFullTimesheetByKomuDto
            {
                EmailAddress = "testemail23@gmail.com",
                ProjectCode = "Project 3",
                TaskName = "Testing",
                Hour = 10,
                Note = "Nothing"
            };
            var user = new User
            {
                Id = 100,
                EmailAddress = "testemail23@gmail.com",
                IsActive = false
            };
            await WithUnitOfWorkAsync(async () =>
            {
                await _work.InsertAsync(user);
            });
            await WithUnitOfWorkAsync(async () =>
            {
                var expected = "Fail! User with emailAddress " + input.EmailAddress + " inactive";
                var result = await _myTimesheet.CreateFullByKomu(input);
                Assert.Equal(expected, result);
            });
        }

        [Fact]
        public async Task Should_Not_Allow_Create_Full_By_Komu_With_Project_Not_Found()
        {
            var input = new MyFullTimesheetByKomuDto
            {
                EmailAddress = "testemail17@gmail.com",
                ProjectCode = "Project 3 Not Exist",
                TaskName = "Testing",
                Hour = 10,
                Note = "Nothing"
            };
            await WithUnitOfWorkAsync(async () =>
            {
                var expected = "Fail! Not found project with code " + input.ProjectCode + " or User is not in this project or project is closed";
                var result = await _myTimesheet.CreateFullByKomu(input);
                Assert.Equal(expected, result);
            });
        }

        [Fact]
        public async Task Should_Not_Allow_Create_Full_By_Komu_With_User_Not_In_Project()
        {
            var input = new MyFullTimesheetByKomuDto
            {
                EmailAddress = "testemail22@gmail.com",
                ProjectCode = "Project 3",
                TaskName = "Testing",
                Hour = 10,
                Note = "Nothing"
            };
            await WithUnitOfWorkAsync(async () =>
            {
                var expected = "Fail! Not found project with code " + input.ProjectCode + " or User is not in this project or project is closed";
                var result = await _myTimesheet.CreateFullByKomu(input);
                Assert.Equal(expected, result);
            });
        }

        [Fact]
        public async Task Should_Not_Allow_Create_Full_By_Komu_With_Task_Not_Found()
        {
            var input = new MyFullTimesheetByKomuDto
            {
                EmailAddress = "testemail17@gmail.com",
                ProjectCode = "Project 3",
                TaskName = "Testing Not Exist",
                Hour = 10,
                Note = "Nothing"
            };
            await WithUnitOfWorkAsync(async () =>
            {
                var expected = "Fail! Not found task with name " + input.TaskName + " or the task is not in this project";
                var result = await _myTimesheet.CreateFullByKomu(input);
                Assert.Equal(expected, result);
            });
        }

        //TODO: Sử dụng GetNow Date không có data để test
        //[Fact]
        //public async Task Should_Not_Allow_Create_Full_By_Komu_With_Working_Time_Invalid()
        //{
        //    var input = new MyFullTimesheetByKomuDto
        //    {
        //        EmailAddress = "testemail17@gmail.com",
        //        ProjectCode = "Project 3",
        //        TaskName = "Testing",
        //        Hour = 10,
        //        Note = "Nothing",             
        //    };
        //    await WithUnitOfWorkAsync(async () =>
        //    {
        //        var expected = "is illegal because this value must be than 0h";
        //        var result = await _myTimesheet.CreateFullByKomu(input);
        //        Assert.Contains(expected, result);
        //    });
        //}

        [Fact]
        public async Task Should_Allow_Create_Full_By_Komu()
        {
            var input = new MyFullTimesheetByKomuDto
            {
                EmailAddress = "testemail17@gmail.com",
                ProjectCode = "Project 3",
                TaskName = "Testing",
                Hour = 10,
                Note = "Nothing",
            };
            await WithUnitOfWorkAsync(async () =>
            {
                var expected = $"Success! Bạn đã log ```10h``` cho task ```Testing``` của dự án ```Project 3``` ngày ```{DateTime.Today.ToString("yyyy-MM-dd")}``` dạng (official) với note là : ```Nothing```";
                var result = await _myTimesheet.CreateFullByKomu(input);
                Assert.Equal(expected, result);
            });

            await WithUnitOfWorkAsync(async () =>
            {
                var myTimesheets = _work.GetAll<MyTimesheet>();
                Assert.Equal(99, myTimesheets.Count());
            });
        }

        //Test Function WarningMyTimesheet
        [Fact]
        public async Task Should_Warning_My_Timesheet()
        {
            var inputDate = new DateTime(2022, 12, 26);
            var inputWorkingTime = 480;
            var inputTimesheetId = 110;
            await WithUnitOfWorkAsync(async () =>
            {
                var result = _myTimesheet.WarningMyTimesheet(inputDate, inputWorkingTime, inputTimesheetId);
                Assert.Equal(1, result.UserId);
                Assert.Equal(new DateTime(2022,12,26), result.DateAt);
                Assert.Equal(0, result.HourOff);
                Assert.Equal(0, result.HourDiMuon);
                Assert.Equal(0, result.HourVeSom);
                Assert.False(result.IsOffHalfDay);
                Assert.Equal(480, result.WorkingTime);
                Assert.Equal(300, result.WorkingTimeLogged);
                Assert.Equal("09:12", result.CheckIn);
                Assert.Equal("18:26", result.CheckOut);
                Assert.False(result.IsOffDay);

            });
        }
    }
}
