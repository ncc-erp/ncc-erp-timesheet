using Abp.Configuration;
using Abp.UI;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Ncc.Authorization.Users;
using Ncc.IoC;
using Shouldly;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Timesheet.APIs.OverTimeHours;
using Timesheet.APIs.Public;
using Timesheet.APIs.Public.Dto;
using Timesheet.Entities;
using Timesheet.Uitls;
using Xunit;
using static Ncc.Entities.Enum.StatusEnum;

namespace Timesheet.Application.Tests.API.Public
{
    /// <summary>
    /// 23/24 functions
    /// 34/35 test cases passed
    /// update day 17/01/2023
    /// </summary>
    
    public class PublicAppService_Tests : TimesheetApplicationTestBase
    {
        private readonly PublicAppService _publicAppService;
        private readonly IWorkScope _workScope;

        public PublicAppService_Tests()
        {
            _workScope = Resolve<IWorkScope>();
            var overTimeHourAppService = NSubstitute.Substitute.For<OverTimeHourAppService>(_workScope);
            var httpContextAccessor = Resolve<IHttpContextAccessor>();

            _publicAppService = new PublicAppService(overTimeHourAppService, httpContextAccessor, _workScope);
            _publicAppService.SettingManager = Resolve<ISettingManager>();

            long requestId = _workScope.InsertAndGetId<AbsenceDayRequest>(new AbsenceDayRequest
            {
                UserId = 6,
                Type = RequestType.Remote,
                Status = RequestStatus.Approved,
            });
            _workScope.Insert<AbsenceDayDetail>(new AbsenceDayDetail
            {
                RequestId = requestId,
                Hour = 3.5,
                DateType = DayType.Morning,
                DateAt = DateTime.Parse("2022-12-05")
            });
        }

        [Fact]
        public async Task GetUserWorkFromHome_Test()
        {
            var expectTotalCount = 1;
            var date = new DateTime(2022,12,5);

            await WithUnitOfWorkAsync(async () =>
            {
                var result = await _publicAppService.GetUserWorkFromHome(date);
                Assert.Equal(expectTotalCount, result.Count);
                result.First().UserId.ShouldBe(6);
                result.First().EmailAddress.ShouldBe("testemail6@gmail.com");
                result.First().Status.ShouldBe("Approved");
                result.First().DateTypeName.ShouldBe("0");
                result.First().DateAt.Value.Date.ShouldBe(new System.DateTime(2022, 12, 5).Date);
            });
        }

        [Fact]
        public async Task GetAllUserLeaveDay_Test()
        {
            var expectTotalCount = 1;
            var date = DateTime.Parse("2022/12/20");

            await WithUnitOfWorkAsync(async () =>
            {
                var result = await _publicAppService.GetAllUserLeaveDay(date);
                Assert.Equal(expectTotalCount, result.Count);

                result.Last().EmailAddress.ShouldBe("testemail6@gmail.com");
                result.Last().Message.ShouldBe("[Approved] Off VeSom: 1.5h");
                result.Last().DateAt.Date.ShouldBe(new System.DateTime(2022, 12, 20).Date);
                result.Last().DayType.ShouldBe(DayType.Custom);
                result.Last().Hour.ShouldBe(1.5);
                result.Last().OnDayType.ShouldBe(OnDayType.VeSom);
                result.Last().RequestType.ShouldBe(RequestType.Off);
                result.Last().Status.ShouldBe(RequestStatus.Approved);
            });
        }

        [Fact]
        public async Task GetAllUserByEmail_Test()
        {
            var expectTotalCount = 4;
            var listEmail = new List<String> { "admin@aspnetboilerplate.com", "testemail4@gmail.com", "testemail5@gmail.com", "testemail16@gmail.com" };

            WithUnitOfWork(() =>
            {
                var result = _publicAppService.GetAllUserByEmail(listEmail);
                Assert.Equal(expectTotalCount, result.Count);

                result.Last().UserId.ShouldBe(16);
                result.Last().FullName.ShouldBe("email16 test");
                result.Last().EmailAddress.ShouldBe("testemail16@gmail.com");
                result.Last().UserType.ShouldBe(Usertype.Staff);
                result.Last().Type.ShouldBe("Staff");
                result.Last().FullName.ShouldBe("email16 test");
            });
        }

        [Fact]
        public async Task GetWorkingStatusByUser_Test()
        {
            var email = "testemail6@gmail.com";
            var date = DateTime.Parse("2022/12/05");

            await WithUnitOfWorkAsync(async () =>
            {
                var result = await _publicAppService.GetWorkingStatusByUser(email, date);

                result.EmailAddress.ShouldBe("testemail6@gmail.com");
                result.DateAt.Date.ShouldBe(new DateTime(2022,12,5).Date);
                result.DayType.ShouldBe(DayType.Morning);
                result.Hour.ShouldBe(3.5);
                result.Message.ShouldBe("[Approved] Off Morning");
                result.RequestType.ShouldBe(RequestType.Off);
                result.Status.ShouldBe(RequestStatus.Approved);
            });
        }

        [Fact]
        public async Task GetAllUser_Test()
        {
            var expectTotalCount = 21;
            var expectLastUser = new
            {
                EmailAddress = "testemail22@gmail.com",
                FullName = "email22 test",
                Sex = Sex.Female,
                BranchId = 1,
            };

            await WithUnitOfWorkAsync(async () =>
           {
               var result = await _publicAppService.GetAllUser();
               var resultUsers = ((IEnumerable)result).Cast<object>().ToList();
               Assert.Equal(expectTotalCount, resultUsers.Count());
               Assert.Equal(expectLastUser.ToString(), resultUsers.Last().ToString());
           });
        }

        [Fact]
        public async Task KmGetAllUsers_Test()
        {
            var expectTotalCount = 21;

            await WithUnitOfWorkAsync(async () =>
            {
                var result = await _publicAppService.KmGetAllUsers();

                Assert.Equal(expectTotalCount, result.Count());
                result.Last().EmailAddress.ShouldBe("testemail22@gmail.com");
                result.Last().UserName.ShouldBe("testemail22@gmail.com");
                result.Last().FullName.ShouldBe("email22 test");
                result.Last().Name.ShouldBe("email22");
                result.Last().Surname.ShouldBe("test");
            });
        }

        [Fact]
        public async Task GetAllProject_Test()
        {
            var expectTotalCount = 7;

            await WithUnitOfWorkAsync(async () =>
            {
                var result = await _publicAppService.GetAllProject();

                Assert.Equal(expectTotalCount, result.Count());
                result.Last().Id.ShouldBe(7);
                result.Last().Code.ShouldBe("Company activity1");
                result.Last().Name.ShouldBe("Company activity1");
            });
        }


        [Fact]
        public async Task CheckUserLogEnoughTimesheetThisWeek_Test1()
        {
            var emailAddress = "testemail6@gmail.com";

            await WithUnitOfWorkAsync(async () =>
            {
                var result = await _publicAppService.CheckUserLogEnoughTimesheetThisWeek(emailAddress);
                var user = await _workScope.GetAll<User>().Where(s => s.EmailAddress.ToLower().Trim() == emailAddress.ToLower().Trim())
               .Select(s => new { s.Id, s.IsActive }).FirstOrDefaultAsync();

                var userId = user.Id;

                DateTime today = DateTimeUtils.GetNow().Date;

                DateTime firstDayOfWeek = DateTimeUtils.FirstDayOfWeek(today);
                DateTime lastDayOfWeek = DateTimeUtils.LastDayOfWeek(today);

                var totalLoggedWeekMinute = _workScope.GetAll<MyTimesheet>()
                   .Where(s => s.UserId == userId)
                   .Where(s => s.DateAt >= firstDayOfWeek && s.DateAt.Date <= lastDayOfWeek)
                   .Where(s => s.Status == TimesheetStatus.Pending || s.Status == TimesheetStatus.Approve)
                   .Where(s => s.TypeOfWork == TypeOfWork.NormalWorkingHours)
                   .Sum(s => s.WorkingTime);

                var totalOffWeekHour = await _workScope.GetAll<AbsenceDayDetail>()
                    .Where(s => s.Request.UserId == userId)
                       .Where(s => s.DateAt >= firstDayOfWeek && s.DateAt.Date <= lastDayOfWeek)
                    .Where(s => s.Request.Type == RequestType.Off)
                    .Where(s => s.Request.Status == RequestStatus.Pending || s.Request.Status == RequestStatus.Approved)
                    .SumAsync(s => s.Hour);

                var missMinute = 40 * 60 - totalOffWeekHour * 60 - totalLoggedWeekMinute;
                Assert.Equal(string.Format("This week is NOT OK! Off:```{0}```Logged normal working time:```{1}```Miss hour:```40h```",
                  CommonUtils.ConvertHourToHHmm((int)(totalOffWeekHour * 60)),
                  CommonUtils.ConvertHourToHHmm(totalLoggedWeekMinute),
                  CommonUtils.ConvertHourToHHmm((int)missMinute)
              ), result);
            });
        }

        [Fact]
        public async Task CheckUserLogEnoughTimesheetThisWeek_Test2()
        {
            var emailAddress = "aaaatestemail@gmail.com";

            await WithUnitOfWorkAsync(async () =>
            {
                var result = await _publicAppService.CheckUserLogEnoughTimesheetThisWeek(emailAddress);

                Assert.Equal("Fail! Not found user with emailAddress = " + emailAddress, result);
            });
        }

        [Fact]
        public async Task GetListUserLogTimesheetThisWeekNotOk_Test1()
        {
            var expectTotalCount = 21;

            await WithUnitOfWorkAsync(async () =>
            {
                var result = await _publicAppService.GetListUserLogTimesheetThisWeekNotOk();

                Assert.Equal(expectTotalCount, result.Count);
                result.Last().EmailAddress.ShouldBe("testemail22@gmail.com");
                result.Last().FullName.ShouldBe("email22 test");
                result.Last().Result.ShouldBe("This week is NOT OK! Off:```00h```PENDING, APPROVED normal working timesheet:```00h```Miss hour:```40h```");
            });
        }

        [Fact]
        public async Task GetTimesheetOfUserInProject_Test()
        {
            await WithUnitOfWorkAsync(async () =>
            {
                var result = await _publicAppService.GetTimesheetOfUserInProject("Project 1", "testemail6@gmail.com", DateTime.Parse("2022/12/01"), DateTime.Parse("2022/12/31"));

                result.ProjectId.ShouldBe(1);
                result.ProjectCode.ShouldBe("Project 1");
                result.NormalWorkingMinute.ShouldBe(2280);
                result.NormalWorkingTime.ShouldBe(38);
                result.NormalWorkingTimeAll.ShouldBe(0);
                result.NormalWorkingTimeStandard.ShouldBe(0);
                result.OTMinute.ShouldBe(0);
                result.OTNoChargeMinute.ShouldBe(0);
                result.OverTime.ShouldBe(0);
                result.OverTimeNoCharge.ShouldBe(0);
            });
        }

        [Fact]
        public async Task GetTimesheetOfUserInProjectNew_Test()
        {
            WithUnitOfWork(() =>
            {
                var result = _publicAppService.GetTimesheetOfUserInProjectNew("Project 1", "testemail6@gmail.com", DateTime.Parse("2022/12/01"), DateTime.Parse("2022/12/31"));

                result.ProjectId.ShouldBe(1);
                result.ProjectCode.ShouldBe("Project 1");
                result.NormalWorkingMinute.ShouldBe(2280);
                result.NormalWorkingTime.ShouldBe(38);
                result.NormalWorkingTimeAll.ShouldBe(79.5);
                result.NormalWorkingTimeStandard.ShouldBe(153.5);
                result.OTMinute.ShouldBe(0);
                result.OTNoChargeMinute.ShouldBe(0);
                result.OverTime.ShouldBe(0);
                result.OverTimeNoCharge.ShouldBe(0);
            });
        }

        [Fact]
        public async Task GetTimesheetOfUserInProjectNew_Should_Not_Get_Because_Project_Is_Null()
        {
            await WithUnitOfWorkAsync(async () =>
            {
                var exception = await Assert.ThrowsAsync<UserFriendlyException>(async () =>
                {
                    _publicAppService.GetTimesheetOfUserInProjectNew("", "test@gmail.com", DateTime.Parse("2022/12/01"), DateTime.Parse("2022/12/30"));
                });
                Assert.Equal("project code is null or empty", exception.Message);
            });
        }

        [Fact]
        public async Task GetUserInProjectFromTimesheet_Test()
        {
            WithUnitOfWork(() =>
           {
               var result = _publicAppService.GetUserInProjectFromTimesheet("Project 1", new List<String> { "admin@aspnetboilerplate.com", "testemail22@gmail.com" }, DateTime.Parse("2022/12/01"), DateTime.Parse("2022/12/31"));

               result.Last().UserId.ShouldBe(6);
               result.Last().Branch.ShouldBe(3);
               result.Last().BranchColor.ShouldBe("blue");
               result.Last().BranchDisplayName.ShouldBe("HN3");
               result.Last().FullName.ShouldBe("email6 test");
               result.Last().EmailAddress.ShouldBe("testemail6@gmail.com");
               result.Last().UserTypeName.ShouldBe("CTV");
               result.Last().UserType.ShouldBe(Usertype.Collaborators);
               result.Last().UserLevel.ShouldBe(UserLevel.FresherMinus);
               result.Last().IsActive.ShouldBe(true);
           });
        }

        /*[Fact]
        public async Task GetUserInProjectFromTimesheet_Should_Not_Get_Because_Project_Is_Null()
        {
            await WithUnitOfWorkAsync(async () =>
            {
                var exception = await Assert.ThrowsAsync<UserFriendlyException>(async () =>
                {
                    var result = _publicAppService.GetUserInProjectFromTimesheet("", new List<String> { "test@gmail.com", "test1@gmail.com" }, DateTime.Parse("2022/12/01"), DateTime.Parse("2022/12/30"));
                });
                Assert.Equal("project code is null or empty", exception.Message);
            });
        }*/

        [Fact]
        public async Task GetTimesheetWeeklyChartOfUserInProject_Should_Get()
        {
            await WithUnitOfWorkAsync(async () =>
            {
                var result = await _publicAppService.GetTimesheetWeeklyChartOfUserInProject("Project UCG", "admin@aspnetboilerplate.com", DateTime.Parse("2022/12/01"), DateTime.Parse("2022/12/30"));

                result.Labels.Count.ShouldBe(5);
                result.NormalWoringHours.Count.ShouldBe(5);
                result.OTNoChargeHours.Count.ShouldBe(5);
                result.OverTimeHours.Count.ShouldBe(5);
                result.Labels.First().ShouldBe("28/11-4/12");
                result.NormalWoringHours.First().ShouldBe(0);
                result.OTNoChargeHours.First().ShouldBe(0);
                result.OverTimeHours.First().ShouldBe(0);
            });
        }

        [Fact]
        public async Task GetTimesheetWeeklyChartOfUserInProject_Should_Not_Get_Because_Project_Code_Is_Null()
        {
            await WithUnitOfWorkAsync(async () =>
            {
                var exception = await Assert.ThrowsAsync<UserFriendlyException>(async () =>
                {
                    await _publicAppService.GetTimesheetWeeklyChartOfUserInProject("", "test@gmail.com", DateTime.Parse("2022/12/01"), DateTime.Parse("2022/12/30"));
                });
                Assert.Equal("project code is null or empty", exception.Message);
            });
        }

        [Fact]
        public async Task GetTimesheetWeeklyChartOfUserInProject_Should_Not_Get_Because_Email_Is_Null()
        {
            await WithUnitOfWorkAsync(async () =>
            {
                var exception = await Assert.ThrowsAsync<UserFriendlyException>(async () =>
                {
                    await _publicAppService.GetTimesheetWeeklyChartOfUserInProject("Project 1", "", DateTime.Parse("2022/12/01"), DateTime.Parse("2022/12/30"));
                });
                Assert.Equal("emailAddress is null or empty", exception.Message);
            });
        }

        [Fact]
        public async Task GetTimesheetWeeklyChartOfProject_Should_Get()
        {
            await WithUnitOfWorkAsync(async () =>
            {
                var result = await _publicAppService.GetTimesheetWeeklyChartOfProject("Project 1", DateTime.Parse("2022/12/01"), DateTime.Parse("2022/12/30"));
                result.Labels.Count.ShouldBe(5);
                result.NormalWoringHours.Count.ShouldBe(5);
                result.OTNoChargeHours.Count.ShouldBe(5);
                result.OverTimeHours.Count.ShouldBe(5);
                result.Labels.First().ShouldBe("28/11-4/12");
                result.NormalWoringHours.First().ShouldBe(4);
                result.OverTimeHours.First().ShouldBe(0);
                result.OTNoChargeHours.First().ShouldBe(0);
            });
        }

        [Fact]
        public async Task GetTimesheetWeeklyChartOfProject_Should_Not_Get_Because_Project_Code_Is_Null()
        {
            await WithUnitOfWorkAsync(async () =>
            {
                var exception = await Assert.ThrowsAsync<UserFriendlyException>(async () =>
                {
                    await _publicAppService.GetTimesheetWeeklyChartOfProject("", DateTime.Parse("2022/12/01"), DateTime.Parse("2022/12/30"));
                });
                Assert.Equal("project code is null or empty", exception.Message);
            });
        }

        [Fact]
        public async Task GetTimesheetWeeklyChartOfUser_Should_Get()
        {
            await WithUnitOfWorkAsync(async () =>
            {
                var result = await _publicAppService.GetTimesheetWeeklyChartOfUser("admin@aspnetboilerplate.com", DateTime.Parse("2022/12/01"), DateTime.Parse("2022/12/30"));
                result.Labels.Count.ShouldBe(5);
                result.NormalWoringHours.Count.ShouldBe(5);
                result.OTNoChargeHours.Count.ShouldBe(5);
                result.OverTimeHours.Count.ShouldBe(5);
                result.Labels.First().ShouldBe("28/11-4/12");
                result.NormalWoringHours.First().ShouldBe(0);
                result.OTNoChargeHours.First().ShouldBe(0);
                result.OverTimeHours.First().ShouldBe(0);
            });
        }

        [Fact]
        public async Task GetTimesheetWeeklyChartOfUser_Should_Not_Get_Because_Project_Email_Is_Null()
        {
            await WithUnitOfWorkAsync(async () =>
            {
                var exception = await Assert.ThrowsAsync<UserFriendlyException>(async () =>
                {
                    await _publicAppService.GetTimesheetWeeklyChartOfUser("", DateTime.Parse("2022/12/01"), DateTime.Parse("2022/12/30"));
                });
                Assert.Equal("emailAddress is null or empty", exception.Message);
            });
        }

        [Fact]
        public async Task GetTimesheetWeeklyChartOfUserGroupInProject_Should_Get()
        {
            var input = new InputGetTimesheetChartOfUserGroupDto
            {
                ProjectCode = "Project 1",
                Emails = new List<String> { "testemail22@gmail.com", "admin@aspnetboilerplate.com" },
                StartDate = DateTime.Parse("2022/12/01"),
                EndDate = DateTime.Parse("2022/12/30")
            };

            await WithUnitOfWorkAsync(async () =>
            {
                var result = await _publicAppService.GetTimesheetWeeklyChartOfUserGroupInProject(input);
                result.Labels.Count.ShouldBe(5);
                result.NormalWoringHours.Count.ShouldBe(5);
                result.OTNoChargeHours.Count.ShouldBe(5);
                result.OverTimeHours.Count.ShouldBe(5);
                result.Labels.First().ShouldBe("28/11-4/12");
                result.NormalWoringHours.First().ShouldBe(0);
                result.OTNoChargeHours.First().ShouldBe(0);
                result.OverTimeHours.First().ShouldBe(0);
            });
        }

        [Fact]
        public async Task GetTimesheetWeeklyChartOfUserGroupInProject_Should_Not_Get_Because_Project_Code_Is_Null()
        {
            var input = new InputGetTimesheetChartOfUserGroupDto
            {
                ProjectCode = "",
                Emails = new List<String> { "test@gmail.com" },
                StartDate = DateTime.Parse("2022/12/01"),
                EndDate = DateTime.Parse("2022/12/30")
            };

            await WithUnitOfWorkAsync(async () =>
            {
                var exception = await Assert.ThrowsAsync<UserFriendlyException>(async () =>
                {
                    await _publicAppService.GetTimesheetWeeklyChartOfUserGroupInProject(input);
                });
                Assert.Equal("project code is null or empty", exception.Message);
            });
        }

        [Fact]
        public async Task GetTimesheetWeeklyChartOfUserGroupInProject_Should_Not_Get_Because_Project_Email_Is_Null()
        {
            var input = new InputGetTimesheetChartOfUserGroupDto
            {
                ProjectCode = "Project UCG",
                Emails = new List<String> { },
                StartDate = DateTime.Parse("2022/12/01"),
                EndDate = DateTime.Parse("2022/12/30")
            };
            await WithUnitOfWorkAsync(async () =>
            {
                var exception = await Assert.ThrowsAsync<UserFriendlyException>(async () =>
                {
                    await _publicAppService.GetTimesheetWeeklyChartOfUserGroupInProject(input);
                });
                Assert.Equal("emails is null or empty", exception.Message);
            });
        }

        [Fact]
        public async Task GetEffortMonthlyChartOfUserGroupInProject_Should_Get()
        {
            var input = new InputGetTimesheetChartOfUserGroupDto
            {
                ProjectCode = "Project 1",
                Emails = new List<String> { "admin@aspnetboilerplate.com" },
                StartDate = DateTime.Parse("2022/12/01"),
                EndDate = DateTime.Parse("2022/12/30")
            };

            await WithUnitOfWorkAsync(async () =>
            {
                var result = await _publicAppService.GetEffortMonthlyChartOfUserGroupInProject(input);

                result.Labels.Count.ShouldBe(1);
                result.ManDays.Count.ShouldBe(1);
                result.NormalWorkingHours.Count.ShouldBe(1);
                result.OTxCofficientHours.Count.ShouldBe(1);
                result.Labels.First().ShouldBe("12-2022");
                result.ManDays.First().ShouldBe(4.75);
                result.NormalWorkingHours.First().ShouldBe(38);
                result.OTxCofficientHours.First().ShouldBe(0);
            });
        }

        [Fact]
        public async Task GetEffortMonthlyChartOfUserGroupInProject_Should_Not_Get_Because_Project_Code_Is_Null()
        {
            var input = new InputGetTimesheetChartOfUserGroupDto
            {
                ProjectCode = "",
                Emails = new List<String> { "test@gmail.com" },
                StartDate = DateTime.Parse("2022/12/01"),
                EndDate = DateTime.Parse("2022/12/30")
            };

            await WithUnitOfWorkAsync(async () =>
            {
                var exception = await Assert.ThrowsAsync<UserFriendlyException>(async () =>
                {
                    await _publicAppService.GetEffortMonthlyChartOfUserGroupInProject(input);
                });
                Assert.Equal("project code is null or empty", exception.Message);
            });
        }

        [Fact]
        public async Task GetEffortMonthlyChartProject_Should_Get()
        {
            var input = new InputGetEffortMonthlyChartDto
            {
                ProjectCode = "Project 1",
                StartDate = DateTime.Parse("2022/12/01"),
                EndDate = DateTime.Parse("2022/12/30")
            };

            await WithUnitOfWorkAsync(async () =>
            {
                var result = await _publicAppService.GetEffortMonthlyChartProject(input);
                result.Labels.Count.ShouldBe(1);
                result.ManDays.Count.ShouldBe(1);
                result.NormalWorkingHours.Count.ShouldBe(1);
                result.OTxCofficientHours.Count.ShouldBe(1);
                result.Labels.First().ShouldBe("12-2022");
                result.ManDays.First().ShouldBe(4.75);
                result.NormalWorkingHours.First().ShouldBe(38);
                result.OTxCofficientHours.First().ShouldBe(0);
            });
        }

        [Fact]
        public async Task GetEffortMonthlyChartProject_Should_Not_Get_Because_Project_Code_Is_Null()
        {
            var input = new InputGetEffortMonthlyChartDto
            {
                ProjectCode = "",
                StartDate = DateTime.Parse("2022/12/01"),
                EndDate = DateTime.Parse("2022/12/30")
            };

            await WithUnitOfWorkAsync(async () =>
            {
                var exception = await Assert.ThrowsAsync<UserFriendlyException>(async () =>
                {
                    await _publicAppService.GetEffortMonthlyChartProject(input);
                });
                Assert.Equal("project code is null or empty", exception.Message);
            });
        }

        [Fact]
        public async Task GetTimesheetDetailForTax_Test()
        {
            var input = new InputTimesheetTaxDto
            {
                ProjectCodes = new List<string> { "Project 1" },
                Month = 12,
                Year = 2022
            };

            await WithUnitOfWorkAsync(async () =>
            {
                var result = await _publicAppService.GetTimesheetDetailForTax(input);

                result.ListTimesheet.Count().ShouldBe(15);
                result.ListWorkingDay.Count().ShouldBe(22);
                result.ListTimesheet.Last().ActualEmailAddress.ShouldBe("testemail6@gmail.com");
                result.ListTimesheet.Last().ActualWorkingMinute.ShouldBe(480);
                result.ListTimesheet.Last().DateAt.Date.ShouldBe(new DateTime(2022,12,23).Date);
                result.ListTimesheet.Last().EmailAddress.ShouldBe("testemail6@gmail.com");
                result.ListTimesheet.Last().ManDay.ShouldBe(1);
                result.ListTimesheet.Last().ProjectCode.ShouldBe("Project 1");
                result.ListTimesheet.Last().ProjectName.ShouldBe("Project 1");
                result.ListTimesheet.Last().ShadowWorkingMinute.ShouldBe(0);
                result.ListTimesheet.Last().TaskName.ShouldBe("Coding");
                result.ListTimesheet.Last().WorkType.ShouldBe(TypeOfWork.NormalWorkingHours);
                result.ListTimesheet.Last().WorkingMinute.ShouldBe(480);
            });
        }

        [Fact]
        public async Task GetDataForCheckPoint_Test()
        {
            var expectTotalCount = 2;
            await WithUnitOfWorkAsync(async () =>
            {
                var result = await _publicAppService.GetDataForCheckPoint(DateTime.Parse("2022/12/01"), DateTime.Parse("2022/12/31"));

                Assert.Equal(expectTotalCount, result.Count);
                var r = result.Last().ToString();
                Assert.Equal("{ UserId = 6, FullName = email6 test, EmailAddress = testemail6@gmail.com, Branch = HN3, Type = Collaborators, TypeName = Collaborators, LevelName = FresherMinus, ReviewerId = 1, ReviewerName = admin admin, ReviewerEmail = admin@aspnetboilerplate.com }", result.Last().ToString());
            });
        }

        [Fact]
        public async Task GetAllUsers_Test()
        {
            var expectTotalCount = 21;
            WithUnitOfWork(() =>
           {
               var result = _publicAppService.GetAllUsers();
               Assert.Equal(expectTotalCount, result.Count);

               result.Last().BranchName.ShouldBe("HN1");
               result.Last().EmailAddress.ShouldBe("testemail22@gmail.com");
               result.Last().FullName.ShouldBe("email22 test");
               result.Last().UserType.ShouldBe(Usertype.Staff);
               result.Last().UserTypeName.ShouldBe("Staff");
           });
        }

        [Fact]
        public async Task GetPMsOfUser_Test()
        {
            var expectTotalCount = 2;
            WithUnitOfWork(() =>
           {
               var result = _publicAppService.GetPMsOfUser("testemail17@gmail.com");
               Assert.Equal(expectTotalCount, result.Count);

               result.First().PMs.Count.ShouldBe(2);
               result.First().PMs.Last().BranchName.ShouldBe("HN1");
               result.First().PMs.Last().EmailAddress.ShouldBe("testemail17@gmail.com");
               result.First().PMs.Last().FullName.ShouldBe("email17 test");
               result.First().PMs.Last().UserTypeName.ShouldBe("Staff");
               result.First().PMs.Last().UserType.ShouldBe(Usertype.Staff);
               result.First().ProjectCode.ShouldBe("Project 3");
               result.First().ProjectName.ShouldBe("Project 3");
           });
        }

        [Fact]
        public async Task GetTimesheetAndCheckInOutAllUser_Test()
        {
            var expectTotalCount = 2;
            WithUnitOfWork(() =>
            {
                var result = _publicAppService.GetTimesheetAndCheckInOutAllUser(DateTime.Parse("2022/12/01"), DateTime.Parse("2022/12/30"));
                Assert.Equal(expectTotalCount, result.Count);

                result.First().Branch.ShouldBe("HN1");
                result.First().EmailAddress.ShouldBe("testemail17@gmail.com");
                result.First().FullName.ShouldBe("email17 test");
                result.First().UserType.ShouldBe(Usertype.Staff);
                result.First().ListDate.Count().ShouldBe(18);
                result.First().ListDate.First().DateAt.Date.ShouldBe(new DateTime(2022,12,1).Date);
                result.First().ListDate.First().TimeSheetMinute.ShouldBe(480);

            });
        }

        //Can not test 

        /*[Fact]
        public async Task CheckConnect_Test()
        {
            WithUnitOfWork(() =>
            {
                var result = _publicAppService.CheckConnect();
                result.ShouldNotBeNull();
            });
        }*/

        [Fact]
        public async Task GetCurrentWorkingTimeAllUserWorking_Test()
        {
            var expectTotalCount = 21;
            await WithUnitOfWorkAsync(async () =>
            {
                var result = await _publicAppService.GetCurrentWorkingTimeAllUserWorking();
                Assert.Equal(expectTotalCount, result.Count);

                result.Last().UserEmail.ShouldBe("testemail22@gmail.com");
                result.Last().MorningStartTime.ShouldBe("08:30");
                result.Last().MorningEndTime.ShouldBe("12:00");
                result.Last().AfternoonStartTime.ShouldBe("13:00");
                result.Last().AfternoonEndTime.ShouldBe("17:30");
            });
        }
    }
}
