using Abp.Authorization.Roles;
using Abp.BackgroundJobs;
using Abp.Configuration;
using Abp.Domain.Entities;
using Abp.ObjectMapping;
using Abp.Runtime.Session;
using Abp.UI;
using Amazon.S3.Model;
using DocumentFormat.OpenXml.Office2010.Excel;
using Ncc.Authorization.Roles;
using Ncc.IoC;
using Shouldly;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Timesheet.APIs.Timesheets.Timesheets.Dto;
using Timesheet.DomainServices;
using Timesheet.Entities;
using Timesheet.Timesheets.Timesheets;
using Timesheet.Uitls;
using Xunit;
using static Ncc.Entities.Enum.StatusEnum;

namespace Timesheet.Application.Tests.API.Timesheets.Timesheets
{
    /// <summary>
    /// 8/8 function 
    /// 25/25 test cases passed
    /// update day 16/01/2023
    /// </summary>
    public class TimesheetAppSevice_Tests : TimesheetApplicationTestBase
    {
        private List<MyTimesheet> listMyTimesheet = new List<MyTimesheet>
        {
            new MyTimesheet
            {
                Id=105,
                ProjectTaskId = 14,
                Note="",
                WorkingTime=240,
                TargetUserWorkingTime=0,
                TypeOfWork=TypeOfWork.NormalWorkingHours,
                IsCharged=false,
                DateAt=new DateTime(2022,12,26),
                Status=TimesheetStatus.Approve,
                ProjectTargetUserId=1,
                IsTemp=false,
                UserId=9,
            },
            new MyTimesheet
            {
                Id=107,
                ProjectTaskId = 14,
                Note="",
                WorkingTime=480,
                TargetUserWorkingTime=0,
                TypeOfWork=TypeOfWork.NormalWorkingHours,
                IsCharged=false,
                DateAt=new DateTime(2022,12,26),
                Status=TimesheetStatus.Approve,
                ProjectTargetUserId=1,
                IsTemp=false,
                UserId=13,
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
                Status=TimesheetStatus.Approve,
                ProjectTargetUserId=1,
                IsTemp=false,
                UserId=1,
            },
            new MyTimesheet
            {
                Id=112,
                ProjectTaskId = 14,
                Note="",
                WorkingTime=300,
                TargetUserWorkingTime=0,
                TypeOfWork=TypeOfWork.NormalWorkingHours,
                IsCharged=false,
                DateAt=new DateTime(2022,12,11),
                Status=TimesheetStatus.Approve,
                ProjectTargetUserId=1,
                IsTemp=false,
                UserId=1,
            },
            new MyTimesheet
            {
                Id=113,
                ProjectTaskId = 14,
                Note="",
                WorkingTime=300,
                TargetUserWorkingTime=0,
                TypeOfWork=TypeOfWork.NormalWorkingHours,
                IsCharged=false,
                DateAt=new DateTime(2022,12,11),
                Status=TimesheetStatus.Approve,
                ProjectTargetUserId=1,
                IsTemp=false,
                UserId=13,
            },
            new MyTimesheet
            {
                Id=114,
                ProjectTaskId = 14,
                Note="",
                WorkingTime=300,
                TargetUserWorkingTime=0,
                TypeOfWork=TypeOfWork.NormalWorkingHours,
                IsCharged=false,
                DateAt=new DateTime(2022,12,18),
                Status=TimesheetStatus.Approve,
                ProjectTargetUserId=1,
                IsTemp=false,
                UserId=9,
            },
            new MyTimesheet
            {
                Id=115,
                ProjectTaskId = 14,
                Note="",
                WorkingTime=300,
                TargetUserWorkingTime=0,
                TypeOfWork=TypeOfWork.NormalWorkingHours,
                IsCharged=false,
                DateAt=DateTime.Today.AddDays(1),
                Status=TimesheetStatus.Pending,
                ProjectTargetUserId=1,
                IsTemp=false,
                UserId=1,
            },
        };
        private readonly TimesheetAppService _timesheet;
        private readonly IWorkScope _work;
        private DateTime lockDate;
        public TimesheetAppSevice_Tests()
        {
            var _backgroundJobManager = Resolve<IBackgroundJobManager>();
            var _commonService = Resolve<ICommonServices>();
            lockDate = _commonService.getlockDatePM();
            _work = Resolve<IWorkScope>();
            _timesheet = new TimesheetAppService(_backgroundJobManager, _commonService, _work);
            _timesheet.AbpSession = Resolve<IAbpSession>();
            _timesheet.SettingManager = Resolve<ISettingManager>();
            _timesheet.ObjectMapper = Resolve<IObjectMapper>();
            foreach (var ts in listMyTimesheet)
            {
                _work.InsertAsync(ts);
            }

        }

        //Test function GetAll
        [Fact]
        public async Task Should_Get_All_With_Filter_Null()
        {
            var inputStartDate = new DateTime(2022, 12, 1);
            var inputEndDate = new DateTime(2022, 12, 31);
            var inputStatus = TimesheetStatus.Approve;
            var inputProjectId = 5;
            HaveCheckInFilter? inputFilter = null;
            await WithUnitOfWorkAsync(async () =>
            {
                var result = await _timesheet.GetAll(inputStartDate, inputEndDate, inputStatus, inputProjectId, inputFilter);
                Assert.Equal(28, result.Count);
                result.ShouldContain(x => x.UserId == 17);
                result.ShouldContain(x => x.UserId == 6);
                result.ShouldContain(x => x.UserId == 1);
                result.ShouldContain(x => x.UserId == 13);
                result.First().Id.ShouldBe(111);
                result.First().BranchColor.ShouldBe("#f44336\t");
                result.First().BranchDisplayName.ShouldBe("HN1");
                result.First().CheckIn.ShouldBe("09:12");
                result.First().CheckOut.ShouldBe("18:26");
                result.First().CustomerName.ShouldBe("Client 3");
                result.First().DateAt.ShouldBe(new DateTime(2022, 12, 26));
                result.First().EmailAddress.ShouldBe("admin@aspnetboilerplate.com");
                result.First().ProjectCode.ShouldBe("Project 3");
                result.First().ProjectId.ShouldBe(5);
                result.First().ProjectName.ShouldBe("Project 3");
                result.First().Status.ShouldBe(TimesheetStatus.Approve);
                result.First().TaskId.ShouldBe(2);
                result.First().TaskName.ShouldBe("Testing");
                result.First().TypeOfWork.ShouldBe(TypeOfWork.NormalWorkingHours);
                result.First().User.ShouldBe("admin admin");
                result.First().UserId.ShouldBe(1);
                result.First().Type.ShouldBe(null);
                result.First().MytimesheetNote.ShouldBe("");
                result.First().IsCharged.ShouldBe(false);
                result.First().IsUserInProject.ShouldBe(true);
                result.First().IsTemp.ShouldBe(false);
                result.First().IsOffDay.ShouldBe(false);
                result.First().LastModificationTime.ShouldBe(null);
                result.First().OffHour.ShouldBe(0);
                result.First().AvatarPath.ShouldBe(null);
            });
        }

        [Fact]
        public async Task Should_Get_All_With_Filter_Have_Checkin()
        {
            var inputStartDate = new DateTime(2022, 12, 1);
            var inputEndDate = new DateTime(2022, 12, 31);
            var inputStatus = TimesheetStatus.Approve;
            var inputProjectId = 5;
            HaveCheckInFilter? inputFilter = HaveCheckInFilter.HaveCheckIn;
            await WithUnitOfWorkAsync(async () =>
            {
                var result = await _timesheet.GetAll(inputStartDate, inputEndDate, inputStatus, inputProjectId, inputFilter);
                Assert.Equal(3, result.Count);
                result.ShouldContain(x => x.UserId == 17);
                result.ShouldNotContain(x => x.UserId == 6);
                result.ShouldContain(x => x.UserId == 1);
                result.ShouldContain(x => x.UserId == 13);
                result[1].Id.ShouldBe(107);
                result[1].BranchColor.ShouldBe("purple\t");
                result[1].BranchDisplayName.ShouldBe("ĐN");
                result[1].CheckIn.ShouldBe("08:40");
                result[1].CheckOut.ShouldBe(null);
                result[1].CustomerName.ShouldBe("Client 3");
                result[1].DateAt.ShouldBe(new DateTime(2022, 12, 26));
                result[1].EmailAddress.ShouldBe("testemail13@gmail.com");
                result[1].ProjectCode.ShouldBe("Project 3");
                result[1].ProjectId.ShouldBe(5);
                result[1].ProjectName.ShouldBe("Project 3");
                result[1].Status.ShouldBe(TimesheetStatus.Approve);
                result[1].TaskId.ShouldBe(2);
                result[1].TaskName.ShouldBe("Testing");
                result[1].TypeOfWork.ShouldBe(TypeOfWork.NormalWorkingHours);
                result[1].User.ShouldBe("email13 test");
                result[1].UserId.ShouldBe(13);
                result[1].Type.ShouldBe(Usertype.Internship);
                result[1].MytimesheetNote.ShouldBe("");
                result[1].IsCharged.ShouldBe(false);
                result[1].IsUserInProject.ShouldBe(true);
                result[1].IsTemp.ShouldBe(false);
                result[1].IsOffDay.ShouldBe(false);
                result[1].LastModificationTime.ShouldBe(null);
                result[1].OffHour.ShouldBe(0);
                result[1].AvatarPath.ShouldBe(null);
            });
        }

        [Fact]
        public async Task Should_Get_All_With_Filter_Have_Checkout()
        {
            var inputStartDate = new DateTime(2022, 12, 1);
            var inputEndDate = new DateTime(2022, 12, 31);
            var inputStatus = TimesheetStatus.Approve;
            var inputProjectId = 5;
            HaveCheckInFilter? inputFilter = HaveCheckInFilter.HaveCheckOut;
            await WithUnitOfWorkAsync(async () =>
            {
                var result = await _timesheet.GetAll(inputStartDate, inputEndDate, inputStatus, inputProjectId, inputFilter);
                Assert.Equal(3, result.Count);
                result.ShouldContain(x => x.UserId == 17);
                result.ShouldContain(x => x.UserId == 6);
                result.ShouldContain(x => x.UserId == 1);
                result.ShouldNotContain(x => x.UserId == 13);
                result.First().Id.ShouldBe(111);
                result.First().BranchColor.ShouldBe("#f44336\t");
                result.First().BranchDisplayName.ShouldBe("HN1");
                result.First().CheckIn.ShouldBe("09:12");
                result.First().CheckOut.ShouldBe("18:26");
                result.First().CustomerName.ShouldBe("Client 3");
                result.First().DateAt.ShouldBe(new DateTime(2022, 12, 26));
                result.First().EmailAddress.ShouldBe("admin@aspnetboilerplate.com");
                result.First().ProjectCode.ShouldBe("Project 3");
                result.First().ProjectId.ShouldBe(5);
                result.First().ProjectName.ShouldBe("Project 3");
                result.First().Status.ShouldBe(TimesheetStatus.Approve);
                result.First().TaskId.ShouldBe(2);
                result.First().TaskName.ShouldBe("Testing");
                result.First().TypeOfWork.ShouldBe(TypeOfWork.NormalWorkingHours);
                result.First().User.ShouldBe("admin admin");
                result.First().UserId.ShouldBe(1);
                result.First().Type.ShouldBe(null);
                result.First().MytimesheetNote.ShouldBe("");
                result.First().IsCharged.ShouldBe(false);
                result.First().IsUserInProject.ShouldBe(true);
                result.First().IsTemp.ShouldBe(false);
                result.First().IsOffDay.ShouldBe(false);
                result.First().LastModificationTime.ShouldBe(null);
                result.First().OffHour.ShouldBe(0);
                result.First().AvatarPath.ShouldBe(null);
            });
        }

        [Fact]
        public async Task Should_Get_All_With_Filter_Have_Checkin_And_Have_Checkout()
        {
            var inputStartDate = new DateTime(2022, 12, 1);
            var inputEndDate = new DateTime(2022, 12, 31);
            var inputStatus = TimesheetStatus.Approve;
            var inputProjectId = 5;
            HaveCheckInFilter? inputFilter = HaveCheckInFilter.HaveCheckInAndHaveCheckOut;
            await WithUnitOfWorkAsync(async () =>
            {
                var result = await _timesheet.GetAll(inputStartDate, inputEndDate, inputStatus, inputProjectId, inputFilter);
                Assert.Equal(2, result.Count);
                result.ShouldContain(x => x.UserId == 17);
                result.ShouldNotContain(x => x.UserId == 6);
                result.ShouldContain(x => x.UserId == 1);
                result.ShouldNotContain(x => x.UserId == 13);
                result.First().Id.ShouldBe(111);
                result.First().BranchColor.ShouldBe("#f44336\t");
                result.First().BranchDisplayName.ShouldBe("HN1");
                result.First().CheckIn.ShouldBe("09:12");
                result.First().CheckOut.ShouldBe("18:26");
                result.First().CustomerName.ShouldBe("Client 3");
                result.First().DateAt.ShouldBe(new DateTime(2022, 12, 26));
                result.First().EmailAddress.ShouldBe("admin@aspnetboilerplate.com");
                result.First().ProjectCode.ShouldBe("Project 3");
                result.First().ProjectId.ShouldBe(5);
                result.First().ProjectName.ShouldBe("Project 3");
                result.First().Status.ShouldBe(TimesheetStatus.Approve);
                result.First().TaskId.ShouldBe(2);
                result.First().TaskName.ShouldBe("Testing");
                result.First().TypeOfWork.ShouldBe(TypeOfWork.NormalWorkingHours);
                result.First().User.ShouldBe("admin admin");
                result.First().UserId.ShouldBe(1);
                result.First().Type.ShouldBe(null);
                result.First().MytimesheetNote.ShouldBe("");
                result.First().IsCharged.ShouldBe(false);
                result.First().IsUserInProject.ShouldBe(true);
                result.First().IsTemp.ShouldBe(false);
                result.First().IsOffDay.ShouldBe(false);
                result.First().LastModificationTime.ShouldBe(null);
                result.First().OffHour.ShouldBe(0);
                result.First().AvatarPath.ShouldBe(null);
            });
        }

        [Fact]
        public async Task Should_Get_All_With_Filter_Have_Checkin_Or_Have_Checkout()
        {
            var inputStartDate = new DateTime(2022, 12, 1);
            var inputEndDate = new DateTime(2022, 12, 31);
            var inputStatus = TimesheetStatus.Approve;
            var inputProjectId = 5;
            HaveCheckInFilter? inputFilter = HaveCheckInFilter.HaveCheckInOrHaveCheckOut;
            await WithUnitOfWorkAsync(async () =>
            {
                var result = await _timesheet.GetAll(inputStartDate, inputEndDate, inputStatus, inputProjectId, inputFilter);
                Assert.Equal(4, result.Count);
                result.ShouldContain(x => x.UserId == 17);
                result.ShouldContain(x => x.UserId == 6);
                result.ShouldContain(x => x.UserId == 1);
                result.ShouldContain(x => x.UserId == 13);
                result.First().Id.ShouldBe(111);
                result.First().BranchColor.ShouldBe("#f44336\t");
                result.First().BranchDisplayName.ShouldBe("HN1");
                result.First().CheckIn.ShouldBe("09:12");
                result.First().CheckOut.ShouldBe("18:26");
                result.First().CustomerName.ShouldBe("Client 3");
                result.First().DateAt.ShouldBe(new DateTime(2022, 12, 26));
                result.First().EmailAddress.ShouldBe("admin@aspnetboilerplate.com");
                result.First().ProjectCode.ShouldBe("Project 3");
                result.First().ProjectId.ShouldBe(5);
                result.First().ProjectName.ShouldBe("Project 3");
                result.First().Status.ShouldBe(TimesheetStatus.Approve);
                result.First().TaskId.ShouldBe(2);
                result.First().TaskName.ShouldBe("Testing");
                result.First().TypeOfWork.ShouldBe(TypeOfWork.NormalWorkingHours);
                result.First().User.ShouldBe("admin admin");
                result.First().UserId.ShouldBe(1);
                result.First().Type.ShouldBe(null);
                result.First().MytimesheetNote.ShouldBe("");
                result.First().IsCharged.ShouldBe(false);
                result.First().IsUserInProject.ShouldBe(true);
                result.First().IsTemp.ShouldBe(false);
                result.First().IsOffDay.ShouldBe(false);
                result.First().LastModificationTime.ShouldBe(null);
                result.First().OffHour.ShouldBe(0);
                result.First().AvatarPath.ShouldBe(null);
            });
        }

        [Fact]
        public async Task Should_Get_All_With_Filter_No_Checkin_And_No_Checkout()
        {
            var inputStartDate = new DateTime(2022, 12, 1);
            var inputEndDate = new DateTime(2022, 12, 31);
            var inputStatus = TimesheetStatus.Approve;
            var inputProjectId = 5;
            HaveCheckInFilter? inputFilter = HaveCheckInFilter.NoCheckInAndNoCheckOut;
            await WithUnitOfWorkAsync(async () =>
            {
                var result = await _timesheet.GetAll(inputStartDate, inputEndDate, inputStatus, inputProjectId, inputFilter);
                Assert.Equal(24, result.Count);
                result.ShouldContain(x => x.UserId == 17);
                result.ShouldContain(x => x.UserId == 6);
                result.ShouldContain(x => x.UserId == 1);
                result.ShouldContain(x => x.UserId == 13);
                result.First().Id.ShouldBe(112);
                result.First().BranchColor.ShouldBe("#f44336\t");
                result.First().BranchDisplayName.ShouldBe("HN1");
                result.First().CheckIn.ShouldBe(null);
                result.First().CheckOut.ShouldBe(null);
                result.First().CustomerName.ShouldBe("Client 3");
                result.First().DateAt.ShouldBe(new DateTime(2022, 12, 11));
                result.First().EmailAddress.ShouldBe("admin@aspnetboilerplate.com");
                result.First().ProjectCode.ShouldBe("Project 3");
                result.First().ProjectId.ShouldBe(5);
                result.First().ProjectName.ShouldBe("Project 3");
                result.First().Status.ShouldBe(TimesheetStatus.Approve);
                result.First().TaskId.ShouldBe(2);
                result.First().TaskName.ShouldBe("Testing");
                result.First().TypeOfWork.ShouldBe(TypeOfWork.NormalWorkingHours);
                result.First().User.ShouldBe("admin admin");
                result.First().UserId.ShouldBe(1);
                result.First().Type.ShouldBe(null);
                result.First().MytimesheetNote.ShouldBe("");
                result.First().IsCharged.ShouldBe(false);
                result.First().IsUserInProject.ShouldBe(true);
                result.First().IsTemp.ShouldBe(false);
                result.First().IsOffDay.ShouldBe(true);
                result.First().LastModificationTime.ShouldBe(null);
                result.First().OffHour.ShouldBe(0);
                result.First().AvatarPath.ShouldBe(null);
            });
        }

        //Test function GetTimesheetWarning
        [Fact]
        public async Task Should_Get_Timesheet_Warning()
        {
            var input = new long[3] { 112, 113, 114 };
            await WithUnitOfWorkAsync(async () =>
            {
                var result = await _timesheet.GetTimesheetWarning(input);
                Assert.Equal(3, result.Count);
                result.First().DateAt.ShouldBe(new DateTime(2022, 12, 11));
                result.First().EmailAddress.ShouldBe("admin@aspnetboilerplate.com");
                result.First().HourOff.ShouldBe(0);
                result.First().Id.ShouldBe(112);
                result.First().IsThanDefaultWorkingHourPerDay.ShouldBe(false);
                result.First().MytimesheetNote.ShouldBe("");
                result.First().ProjectName.ShouldBe("Project 3");
                result.First().Status.ShouldBe(TimesheetStatus.Approve);
                result.First().TaskName.ShouldBe("Testing");
                result.First().TotalWorkingTimeDateAt.ShouldBe(0);
                result.First().TotalWorkingTimeHourDateAt.ShouldBe(0);
                result.First().UserId.ShouldBe(1);
                result.First().WorkingTime.ShouldBe(300);
                result.First().WorkingTimeHour.ShouldBe(5);
            });
        }

        //Test function ApproveTimesheets
        [Fact]
        public async Task Should_Allow_Approve_Timesheets()
        {
            var now = DateTimeUtils.GetNow();
            var input = new long[1] { 115 };
            await WithUnitOfWorkAsync(async () =>
            {
                var result = await _timesheet.ApproveTimesheets(input);
                result.GetType().GetProperty("Success").GetValue(result, null).ShouldBe(" - Success 1 timesheets.");
                result.GetType().GetProperty("SuccessCount").GetValue(result, null).ShouldBe(1);
                result.GetType().GetProperty("FailedCount").GetValue(result, null).ShouldBe(0);
                result.GetType().GetProperty("Fail").GetValue(result, null).ShouldBe(" - Fail 0 timesheets.");
                result.GetType().GetProperty("LockDate").GetValue(result, null).ShouldBe(string.Format(" - Locked date: {0}.", lockDate.ToString("dd'-'MM'-'yyyy")));
            });
            await WithUnitOfWorkAsync(async () =>
            {
                var myTimesheet = await _work.GetAsync<MyTimesheet>(115);
                Assert.Equal(TimesheetStatus.Approve, myTimesheet.Status);
            });

        }

        [Fact]
        public async Task Should_Not_Allow_Approve_Timesheets_With_Timesheet_Locked()
        {
            var now = DateTimeUtils.GetNow();
            var input = new long[1] { 114 };
            await WithUnitOfWorkAsync(async () =>
            {
                var expectedMsg = "PM hãy vào ims.nccsoft.vn để unlock timesheet!";
                var exception = await Assert.ThrowsAsync<UserFriendlyException>(async () =>
                {
                    await _timesheet.ApproveTimesheets(input);
                });
                Assert.Equal(expectedMsg, exception.Message);
            });
        }

        //Test function RejectTimesheets
        [Fact]
        public async Task Should_Allow_Reject_Timesheets()
        {
            var now = DateTimeUtils.GetNow();
            var input = new long[1] { 115 };
            await WithUnitOfWorkAsync(async () =>
            {
                var result = await _timesheet.RejectTimesheets(input);
                result.GetType().GetProperty("Success").GetValue(result, null).ShouldBe(" - Success 1 timesheets.");
                result.GetType().GetProperty("Fail").GetValue(result, null).ShouldBe(" - Fail 0 timesheets.");
                result.GetType().GetProperty("LockDate").GetValue(result, null).ShouldBe(string.Format(" - Locked date: {0}.", lockDate.ToString("dd'-'MM'-'yyyy")));
            });
            await WithUnitOfWorkAsync(async () =>
            {
                var myTimesheet = await _work.GetAsync<MyTimesheet>(115);
                Assert.Equal(TimesheetStatus.Reject, myTimesheet.Status);
            });

        }

        [Fact]
        public async Task Should_Not_Allow_Reject_Timesheets_With_Timesheet_Locked()
        {
            var now = DateTimeUtils.GetNow();
            var input = new long[1] { 114 };
            await WithUnitOfWorkAsync(async () =>
            {
                var expectedMsg = "PM hãy vào ims.nccsoft.vn để unlock timesheet";
                var exception = await Assert.ThrowsAsync<UserFriendlyException>(async () =>
                {
                    await _timesheet.RejectTimesheets(input);
                });
                Assert.Equal(expectedMsg, exception.Message);
            });
        }

        //Test function UnlockTimesheet
        [Fact]
        public async Task Should_Not_Allow_Unlock_Timesheet_With_Timesheet_Already_Unlocked_Case_1()
        {
            var input = new UnlockTimesheetDto
            {
                Id = 100,
                UserId = 17,
                Type = LockUnlockTimesheetType.MyTimesheet
            };
            await WithUnitOfWorkAsync(async () =>
            {
                var expectedMsg = string.Format("User Id - {0} already unlocked", input.UserId);
                var exception = await Assert.ThrowsAsync<UserFriendlyException>(async () =>
                {
                    await _timesheet.UnlockTimesheet(input);
                });
                Assert.Equal(expectedMsg, exception.Message);
            });
        }

        [Fact]
        public async Task Should_Not_Allow_Unlock_Timesheet_With_Timesheet_Already_Unlocked_Case_2()
        {
            var unlockTimesheet = new UnlockTimesheet
            {
                Id = 100,
                UserId = 1,
                Type = LockUnlockTimesheetType.ApproveRejectTimesheet
            };
            await WithUnitOfWorkAsync(async () =>
            {
                await _work.InsertAsync(unlockTimesheet);
            });
            var input = new UnlockTimesheetDto
            {
                Id = 100,
                UserId = 1,
                Type = LockUnlockTimesheetType.ApproveRejectTimesheet
            };
            await WithUnitOfWorkAsync(async () =>
            {
                var unlockTimesheets = _work.GetAll<UnlockTimesheet>();
                Assert.Equal(3, unlockTimesheets.Count());
                var expectedMsg = string.Format("User Id - {0} already unlocked", input.UserId);
                var exception = await Assert.ThrowsAsync<UserFriendlyException>(async () =>
                {
                    await _timesheet.UnlockTimesheet(input);
                });
                Assert.Equal(expectedMsg, exception.Message);
            });
        }

        [Fact]
        public async Task Should_Allow_Unlock_Timesheet()
        {
            var input = new UnlockTimesheetDto
            {
                Id = 100,
                UserId = 1,
                Type = LockUnlockTimesheetType.ApproveRejectTimesheet
            };
            var result = new UnlockTimesheetDto { };
            await WithUnitOfWorkAsync(async () =>
            {
                result = await _timesheet.UnlockTimesheet(input);
                Assert.Equal(input.UserId, result.UserId);
                Assert.Equal(input.Type, result.Type);
            });
            await WithUnitOfWorkAsync(async () =>
            {
                var unlockTimesheets = _work.GetAll<UnlockTimesheet>();
                Assert.Equal(3, unlockTimesheets.Count());
                var unlockTimehseet = await _work.GetAsync<UnlockTimesheet>(result.Id);
                Assert.Equal(input.Id, unlockTimehseet.Id);
                Assert.Equal(input.UserId, unlockTimehseet.UserId);
                Assert.Equal(input.Type, unlockTimehseet.Type);
            });
        }

        //Test function LockTimesheet
        [Fact]
        public async Task Should_Not_Allow_Lock_Timesheet_With_User_Not_Exist_Case_1()
        {
            var input = new LockTimesheetDto
            {
                UserId = 100,
                Type = LockUnlockTimesheetType.MyTimesheet
            };
            await WithUnitOfWorkAsync(async () =>
            {
                var expectedMsg = string.Format("UserId {0} is not exist", input.UserId);
                var exception = await Assert.ThrowsAsync<UserFriendlyException>(async () =>
                {
                    await _timesheet.LockTimesheet(input);
                });
                Assert.Equal(expectedMsg, exception.Message);
            });
        }

        [Fact]
        public async Task Should_Not_Allow_Lock_Timesheet_With_User_Not_Exist_Case_2()
        {
            var input = new LockTimesheetDto
            {
                UserId = 17,
                Type = LockUnlockTimesheetType.ApproveRejectTimesheet
            };
            await WithUnitOfWorkAsync(async () =>
            {
                var expectedMsg = string.Format("UserId {0} is not exist", input.UserId);
                var exception = await Assert.ThrowsAsync<UserFriendlyException>(async () =>
                {
                    await _timesheet.LockTimesheet(input);
                });
                Assert.Equal(expectedMsg, exception.Message);
            });
        }

        [Fact]
        public async Task Should_Allow_Lock_Timesheet()
        {
            var input = new LockTimesheetDto
            {
                UserId = 17,
                Type = LockUnlockTimesheetType.MyTimesheet
            };
            await WithUnitOfWorkAsync(async () =>
            {
                await _timesheet.LockTimesheet(input);
            });
            await WithUnitOfWorkAsync(async () =>
            {
                var unlockTimesheets = _work.GetAll<UnlockTimesheet>();
                Assert.Equal(1, unlockTimesheets.Count());
                await Assert.ThrowsAsync<EntityNotFoundException>(async () =>
                {
                    var unlockTimehseet = await _work.GetAsync<UnlockTimesheet>(input.UserId);
                });
            });
        }

        //Test function GetAllTimeSheetOrRemote
        [Fact]
        public async Task Should_Get_All_Timesheet_Or_Remote_With_Export_True()
        {
            var inputDate = new DateTime(2022, 12, 26);
            var inputExport = true;
            await WithUnitOfWorkAsync(async () =>
            {
                var result = await _timesheet.GetAllTimeSheetOrRemote(inputDate, inputExport);
                Assert.Equal(5, result.Count());
                result.First().Id.ShouldBe(2);
                result.First().Name.ShouldBe("email17");
                result.First().ProjectName.ShouldBe("Project 3");
                result.First().NormalWorkingHours.ShouldBe(8);
                result.First().OverTime.ShouldBe(0);
            });
        }

        [Fact]
        public async Task Should_Get_All_Timesheet_Or_Remote_With_Export_False()
        {

            await WithUnitOfWorkAsync(async () =>
            {
                var dayRequest = new AbsenceDayRequest
                {
                    UserId = 6,
                    DayOffTypeId = 1,
                    Status = RequestStatus.Approved,
                    Reason = "None",
                    Type = RequestType.Remote
                };
                var idDayRequest = await _work.InsertAndGetIdAsync(dayRequest);
                var dayDetail = new AbsenceDayDetail
                {
                    DateAt = new DateTime(2022, 12, 28),
                    DateType = DayType.Fullday,
                    RequestId = idDayRequest,
                    Hour = 8,
                };
                await _work.InsertAsync(dayDetail);
            });
            var inputDate = new DateTime(2022, 12, 28);
            var inputExport = false;
            await WithUnitOfWorkAsync(async () =>
            {
                var result = await _timesheet.GetAllTimeSheetOrRemote(inputDate, inputExport);
                Assert.Single(result);
                result.First().Name.ShouldBe("email6 test");
                result.First().DayOffType.ShouldBe(DayType.Fullday);
                Assert.Equal(0, (int)result.First().TimeCustom);
            });
        }

        //Test function GetQuantiyTimesheetStatus
        [Fact]
        public async Task Should_Get_Quantity_Timesheet_Status_With_Filter_Null()
        {
            var inputStartDate = new DateTime(2022, 12, 1);
            var inputEndDate = new DateTime(2022, 12, 31);
            var inputProjectId = 5;
            HaveCheckInFilter? inputFilter = null;
            var searchText = "";
            var branchId = 0;
            await WithUnitOfWorkAsync(async () =>
            {
                var result = await _timesheet.GetQuantiyTimesheetStatus(inputStartDate, inputEndDate, inputProjectId, inputFilter, searchText, branchId);
                var listStatus = ((IEnumerable)result).Cast<object>().ToList();
                Assert.Equal(5, listStatus.Count);
                listStatus[0].ToString().ShouldBe("{ Status = All, Quantity = 0 }");
                listStatus[1].ToString().ShouldBe("{ Status = Approve, Quantity = 28 }");
                listStatus[2].ToString().ShouldBe("{ Status = None, Quantity = 0 }");
                listStatus[3].ToString().ShouldBe("{ Status = Pending, Quantity = 9 }");
                listStatus[4].ToString().ShouldBe("{ Status = Reject, Quantity = 0 }");

            });
        }

        [Fact]
        public async Task Should_Get_Quantity_Timesheet_Status_With_Filter_Have_Checkin()
        {
            var inputStartDate = new DateTime(2022, 12, 1);
            var inputEndDate = new DateTime(2022, 12, 31);
            var inputProjectId = 5;
            var searchText = "";
            var branchId = 0;
            HaveCheckInFilter? inputFilter = HaveCheckInFilter.HaveCheckIn;
            await WithUnitOfWorkAsync(async () =>
            {
                var result = await _timesheet.GetQuantiyTimesheetStatus(inputStartDate, inputEndDate, inputProjectId, inputFilter, searchText, branchId);
                var listStatus = ((IEnumerable)result).Cast<object>().ToList();
                Assert.Equal(5, listStatus.Count);
                listStatus[0].ToString().ShouldBe("{ Status = All, Quantity = 0 }");
                listStatus[1].ToString().ShouldBe("{ Status = Approve, Quantity = 3 }");
                listStatus[2].ToString().ShouldBe("{ Status = None, Quantity = 0 }");
                listStatus[3].ToString().ShouldBe("{ Status = Pending, Quantity = 0 }");
                listStatus[4].ToString().ShouldBe("{ Status = Reject, Quantity = 0 }");

            });
        }

        [Fact]
        public async Task Should_Get_Quantity_Timesheet_Status_With_Filter_Have_Checkout()
        {
            var inputStartDate = new DateTime(2022, 12, 1);
            var inputEndDate = new DateTime(2022, 12, 31);
            var inputProjectId = 5;
            HaveCheckInFilter? inputFilter = HaveCheckInFilter.HaveCheckOut;
            var searchText = "";
            var branchId = 0;
            await WithUnitOfWorkAsync(async () =>
            {
                var result = await _timesheet.GetQuantiyTimesheetStatus(inputStartDate, inputEndDate, inputProjectId, inputFilter, searchText, branchId);
                var listStatus = ((IEnumerable)result).Cast<object>().ToList();
                Assert.Equal(5, listStatus.Count);
                listStatus[0].ToString().ShouldBe("{ Status = All, Quantity = 0 }");
                listStatus[1].ToString().ShouldBe("{ Status = Approve, Quantity = 3 }");
                listStatus[2].ToString().ShouldBe("{ Status = None, Quantity = 0 }");
                listStatus[3].ToString().ShouldBe("{ Status = Pending, Quantity = 0 }");
                listStatus[4].ToString().ShouldBe("{ Status = Reject, Quantity = 0 }");
            });
        }

        [Fact]
        public async Task Should_Get_Quantity_Timesheet_Status_With_Filter_Have_Checkout_And_Have_Checkin()
        {
            var inputStartDate = new DateTime(2022, 12, 1);
            var inputEndDate = new DateTime(2022, 12, 31);
            var inputProjectId = 5;
            HaveCheckInFilter? inputFilter = HaveCheckInFilter.HaveCheckInAndHaveCheckOut;
            var searchText = "";
            var branchId = 0;
            await WithUnitOfWorkAsync(async () =>
            {
                var result = await _timesheet.GetQuantiyTimesheetStatus(inputStartDate, inputEndDate, inputProjectId, inputFilter, searchText, branchId);
                var listStatus = ((IEnumerable)result).Cast<object>().ToList();
                Assert.Equal(5, listStatus.Count);
                listStatus[0].ToString().ShouldBe("{ Status = All, Quantity = 0 }");
                listStatus[1].ToString().ShouldBe("{ Status = Approve, Quantity = 2 }");
                listStatus[2].ToString().ShouldBe("{ Status = None, Quantity = 0 }");
                listStatus[3].ToString().ShouldBe("{ Status = Pending, Quantity = 0 }");
                listStatus[4].ToString().ShouldBe("{ Status = Reject, Quantity = 0 }");
            });
        }

        [Fact]
        public async Task Should_Get_Quantity_Timesheet_Status_With_Filter_Have_Checkout_Or_Have_Checkin()
        {
            var inputStartDate = new DateTime(2022, 12, 1);
            var inputEndDate = new DateTime(2022, 12, 31);
            var inputProjectId = 5;
            HaveCheckInFilter? inputFilter = HaveCheckInFilter.HaveCheckInOrHaveCheckOut;
            var searchText = "";
            var branchId = 0;
            await WithUnitOfWorkAsync(async () =>
            {
                var result = await _timesheet.GetQuantiyTimesheetStatus(inputStartDate, inputEndDate, inputProjectId, inputFilter, searchText, branchId);
                var listStatus = ((IEnumerable)result).Cast<object>().ToList();
                Assert.Equal(5, listStatus.Count);
                listStatus[0].ToString().ShouldBe("{ Status = All, Quantity = 0 }");
                listStatus[1].ToString().ShouldBe("{ Status = Approve, Quantity = 4 }");
                listStatus[2].ToString().ShouldBe("{ Status = None, Quantity = 0 }");
                listStatus[3].ToString().ShouldBe("{ Status = Pending, Quantity = 0 }");
                listStatus[4].ToString().ShouldBe("{ Status = Reject, Quantity = 0 }");
            });
        }

        [Fact]
        public async Task Should_Get_Quantity_Timesheet_Status_With_Filter_No_Have_Checkout_And_No_Have_Checkin()
        {
            var inputStartDate = new DateTime(2022, 12, 1);
            var inputEndDate = new DateTime(2022, 12, 31);
            var inputProjectId = 5;
            HaveCheckInFilter? inputFilter = HaveCheckInFilter.NoCheckInAndNoCheckOut;
            var searchText = "";
            var branchId = 0;
            await WithUnitOfWorkAsync(async () =>
            {
                var result = await _timesheet.GetQuantiyTimesheetStatus(inputStartDate, inputEndDate, inputProjectId, inputFilter, searchText, branchId);
                var listStatus = ((IEnumerable)result).Cast<object>().ToList();
                Assert.Equal(5, listStatus.Count);
                listStatus[0].ToString().ShouldBe("{ Status = All, Quantity = 0 }");
                listStatus[1].ToString().ShouldBe("{ Status = Approve, Quantity = 24 }");
                listStatus[2].ToString().ShouldBe("{ Status = None, Quantity = 0 }");
                listStatus[3].ToString().ShouldBe("{ Status = Pending, Quantity = 9 }");
                listStatus[4].ToString().ShouldBe("{ Status = Reject, Quantity = 0 }");
            });
        }
    }
}
