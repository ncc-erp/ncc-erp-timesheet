using Abp.Authorization.Roles;
using Abp.Authorization;
using Abp.Configuration;
using Abp.Domain.Repositories;
using Abp.Domain.Uow;
using Abp.Runtime.Caching;
using Abp.Runtime.Session;
using Abp.Zero.Configuration;
using AutoMapper;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Ncc.Authorization.Roles;
using Ncc.Authorization.Users;
using Ncc.Entities;
using Ncc.IoC;
using NSubstitute;
using Shouldly;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using Timesheet.APIs.HRM;
using Timesheet.APIs.OverTimeHours;
using Timesheet.DomainServices;
using Timesheet.Services.Komu;
using Xunit;
using Timesheet.APIs.RetroDetails;
using Timesheet.Timesheets.Projects;
using Timesheet.Application.Tests.API.Info;
using Abp.UI;
using Timesheet.APIs.Positions;
using System;
using static Ncc.Entities.Enum.StatusEnum;
using Timesheet.Entities;
using Timesheet.APIs.HRM.Dto;
using Microsoft.EntityFrameworkCore;
using Timesheet.DomainServices.Dto;
using Timesheet.APIs.RequestDays.Dto;
using NSubstitute.ExceptionExtensions;

namespace Timesheet.Application.Tests.API
{
    public class HRMAppService_Test : HRMAppServiceTestBase
    {

        private readonly  HRMAppService _HRMAppService;

        private readonly IWorkScope _workScope;
        public HRMAppService_Test()
        {

            _HRMAppService = InstanceHRMAppService();
            _workScope = Resolve<IWorkScope>();
        }

        [Fact]
        // Get All Normal Working
        public void GetAllNormalWorkingTest1()
        {
            var y = 2022;
            var m = 12;
            WithUnitOfWork(() =>
            {
                var result = _HRMAppService.GetAllNormalWorking(y, m);
                result.Count.ShouldBe(21);
                result[0].NormalizedEmailAddress.ShouldBe("ADMIN@ASPNETBOILERPLATE.COM");
                result[0].ListWorkingHour.ShouldBe(null);
                result[0].TotalOpenTalk.ShouldBe(0);
                result[0].TotalWorkingHour.ShouldBe(0);
                result[0].TotalWorkingHourOfMonth.ShouldBe(0);
                result[0].UserId.ShouldBe(1);

                result[4].NormalizedEmailAddress.ShouldBe("HIEU.TRANTRUNG@NCC.ASIA");
                result[4].ListWorkingHour.Count().ShouldBe(12);
                result[4].TotalOpenTalk.ShouldBe(1);
                result[4].TotalWorkingHour.ShouldBe(80);
                result[4].TotalWorkingHourOfMonth.ShouldBe(76);
                result[4].UserId.ShouldBe(6);
                result[4].ListWorkingHour.First().Day.ShouldBe(1);
                result[4].ListWorkingHour.First().DayName.ShouldBe("Thursday");
                result[4].ListWorkingHour.First().IsOpenTalk.ShouldBe(false);
                result[4].ListWorkingHour.First().WorkingHour.ShouldBe(8);


            });
        }

        [Fact]
        // Get All Normal Working null year or month
        public async void GetAllNormalWorkingTest2()
        {
            var y = 20221;
            var m = 12;
            await WithUnitOfWorkAsync(async () =>
            {
                await Assert.ThrowsAsync<ArgumentOutOfRangeException>(() => {
                    _HRMAppService.GetAllNormalWorking(y, m);
                    return System.Threading.Tasks.Task.CompletedTask;
                });
            });
        }

        [Fact]
        // Get All Over Time
        public void GetAllOverTimeTest1()
        {
            var y = 2022;
            var m = 12;
            WithUnitOfWork(async() =>
            {
                var result = await _HRMAppService.GetAllOverTime(y, m);
                result.Count.ShouldBe(5);
                result[0].Branch.ShouldBe(null);
                result[0].NormalizedEmailAddress.ShouldBe("HIEU.TRANTRUNG@NCC.ASIA");
                result[0].UserId.ShouldBe(6);
                result[0].ListOverTimeHour.Count.ShouldBe(1);
                result[0].ListOverTimeHour[0].Day.ShouldBe(27);
                result[0].ListOverTimeHour[0].Coefficient.ShouldBe(1);
                result[0].ListOverTimeHour[0].OTHour.ShouldBe(8);
                result[0].ListOverTimeHour[0].OTHourWithCoefficient.ShouldBe(8);
                result[0].ListOverTimeHour[0].WorkingHour.ShouldBe(8);
            });
        }

        [Fact]
        // Get All Over Time null month or year
        public async System.Threading.Tasks.Task GetAllOverTimeTest2()
        {
            var y = 20222;
            var m = 12;
            await Assert.ThrowsAsync<ArgumentOutOfRangeException>(async() =>
            {
                await _HRMAppService.GetAllOverTime(y, m);
            });
        }

        [Fact]
        // Get All Absenceday
        public void GetAllAbsencedayTest1()
        {
            var y = 2022;
            var m = 12;
            WithUnitOfWork(async() =>
            {
                var result = await _HRMAppService.GetAllAbsenceday(y, m);
                result.Count.ShouldBe(3);
                result[0].NormalizedEmailAddress.ShouldBe("TRANG.VUQUYNH@NCC.ASIA");
                result[0].UserId.ShouldBe(17);
                result[0].absenceDayDetails.Count.ShouldBe(3);
                result[0].absenceDayDetails[0].DateAt.ShouldBe(new DateTime(2022, 12, 6));
                result[0].absenceDayDetails[0].DateTypeName.ShouldBe("Fullday");
                result[0].absenceDayDetails[0].DayType.ShouldBe(DayType.Fullday);
                result[0].absenceDayDetails[0].Hour.ShouldBe(8);
                result[0].absenceDayDetails[0].Id.ShouldBe(1);
                result[0].absenceDayDetails[0].Status.ShouldBe(OffTypeStatus.CoPhep);
            });
        }

        [Fact]
        // Get All Day Off
        public void GetAllDayOffTest1()
        {
            var y = 2022;
            var m = 12;
            WithUnitOfWork(async() =>
            {
                var result = await _HRMAppService.GetAllDayOff(y, m);
                result.Count.ShouldBe(4);
                result[0].Branch.ShouldBe(null);
                result[0].Coefficient.ShouldBe(2);
                result[0].DayOff.ShouldBe(new DateTime(2022,12,4));
                result[0].Id.ShouldBe(1);
                result[0].Name.ShouldBe(null);
            });
        }

        [Fact]
        // Get All Project
        public void GetAllProjectTest1()
        {
            WithUnitOfWork( async() =>
            {
                var result = await _HRMAppService.GetAllProject();
                result.Count.ShouldBe(7);
                result[0].Id.ShouldBe(1);
                result[0].Name.ShouldBe("Project UCG");
                result[0].Code.ShouldBe("Project UCG");
                result[0].Pms.ShouldBe("admin admin");
            });
        }

        [Fact]
        // Update User When Off
        public void UpdateUserWhenOffTest1()
        {
            var input = new UpdateUserWhenOffDto
            {
                Email = "hieu.trantrung@ncc.asia",
                IsStopWork = true,
                IsActive = true,
            };

            WithUnitOfWork(async() =>
            {
                var result = await _HRMAppService.UpdateUserWhenOff(input);
                result.ShouldBe(input);
            });

            WithUnitOfWork(async () =>
            {
                var updatedUser = await _workScope.GetAll<User>()
                .Where(x => x.EmailAddress.ToLower().Trim() == input.Email.ToLower().Trim())
                .FirstOrDefaultAsync();

                updatedUser.IsActive.ShouldBeTrue();
                updatedUser.IsStopWork.ShouldBeTrue();
            });
        }

        [Fact]
        // Update User When Off user not found
        public async void UpdateUserWhenOffTest2()
        {
            var input = new UpdateUserWhenOffDto
            {
                Email = "hieu22.trantrung@ncc.asia",
                IsStopWork = true,
                IsActive = true,
            };

            await WithUnitOfWorkAsync(async () =>
            {
                var exception = await Assert.ThrowsAsync<UserFriendlyException>(async () =>
                {
                    await _HRMAppService.UpdateUserWhenOff(input);
                });
                Assert.Equal("Email of Timesheet is not the same as hrm", exception.Message);
            });
        }

        [Fact]
        // Get User Register Time
        public void GetUserRegisterTimeTest1()
        {
            var branch = "HN1";
            DateTime date = new DateTime(2022,12,28);

            WithUnitOfWork(async () =>
            {
                var result = await _HRMAppService.GetUserRegisterTime(branch,date);
                result.Count.ShouldBe(1);
                result[0].AfternoonEndAt.ShouldBe("18:00");
                result[0].DateChangeWorkingTime.ShouldBe(new DateTime(2022, 12, 28));
                result[0].EmailAddress.ShouldBe("trang.vuquynh@ncc.asia");
                result[0].MorningStartAt.ShouldBe("09:00");
                result[0].UserId.ShouldBe(17);
            });
        }

        [Fact]
        // Get User Register Time date not found
        public void GetUserRegisterTimeTest2()
        {
            var branch = "H124N12";
            DateTime date = new DateTime(2022, 12, 27);

            WithUnitOfWork(async () =>
            {
                var result = await _HRMAppService.GetUserRegisterTime(branch, date);
                result.Count.ShouldBe(0);
            });
        }

        [Fact]
        // Get Request By Time And Type
        public void GetRequestByTimeAndTypeTest1()
        {
            var name = "";
            var type = RequestType.Off;
            DateTime startdate = new DateTime(2022, 12, 8);
            DateTime enddate = new DateTime(2022, 12, 19);

            WithUnitOfWork(async () =>
            {
                var result = await _HRMAppService.GetRequestByTimeAndType(startdate,enddate,name,type);
                result.Count.ShouldBe(4);
                result[0].AbsenceTime.ShouldBe(null);
                result[0].AvatarFullPath.ShouldBe("");
                result[0].AvatarPath.ShouldBe("");
                result[0].Branch.ShouldBe("HN1");
                result[0].BranchColor.ShouldBe(null);
                result[0].BranchDisplayName.ShouldBe(null);
                result[0].CreateBy.ShouldBe(null);
                result[0].CreateTime.ShouldBe(new DateTime(0001,01,01));
                result[0].DateAt.ShouldBe(new DateTime(2022, 12, 08));
                result[0].DateType.ShouldBe(DayType.Fullday);
                result[0].DayOffName.ShouldBe("Nghỉ cưới bản thân (3 ngày phép)\t");
                result[0].FullName.ShouldBe("Trang Vũ Quỳnh");
                result[0].Hour.ShouldBe(8);
                result[0].Id.ShouldBe(3);
                result[0].LastModificationTime.ShouldBe(null);
                result[0].LastModifierUserName.ShouldBe(null);
                result[0].LeavedayType.ShouldBe(RequestType.Off);
                result[0].Name.ShouldBe("Trang");
                result[0].Reason.ShouldBe("cưới");
                result[0].Sex.ShouldBe(Sex.Female);
                result[0].ShortName.ShouldBe("Trang");
                result[0].Status.ShouldBe(RequestStatus.Approved);
                result[0].Type.ShouldBe(Usertype.Staff);
                result[0].UserId.ShouldBe(17);
            });
        }

        [Fact]
        // Get Request By Time And Type null 
        public void GetRequestByTimeAndTypeTest2()
        {
            var name = "";
            var type = RequestType.Off;
            DateTime startdate = new DateTime(2023, 12, 8);
            DateTime enddate = new DateTime(2023, 12, 19);

            WithUnitOfWork(async () =>
            {
                var result = await _HRMAppService.GetRequestByTimeAndType(startdate, enddate, name, type);
                result.Count.ShouldBe(0);
            });
        }

        /*[Fact]
        // Create A Valid User
        public void CreateUserTest1()
        {
            var user = new CreateUserDto
            {
                UserName = "Nguyễn Công Toại",
                Name = "Toại",
                Surname = "Nguyễn",
                EmailAddress = "toainc@ncc.asia.com",
                PhoneNumber = "0966886688",
                Address = "Hà Nội",
                Type = Usertype.Staff,
                PositionId = 1,
                AfternoonEndAt = "9:00",
                MorningWorking = "3.5",
                MorningStartAt = "8:30",
                MorningEndAt = "12:00",
                ManagerId = 1,
                AfternoonStartAt = "1:00",
                AfternoonWorking = "4.5",
                AllowedLeaveDay = 1,
                BeginLevel = UserLevel.Intern_1,
                BranchCode = "abc",
                BranchId = 1,
                EndDateAt = DateTime.Now,
                IsActive = true,
                isWorkingTimeDefault = true,
                JobTitle = "abc",
                Level = UserLevel.Senior,
                RoleNames = new string[] { },
                Password = "abc",
                Salary = 10000000,
                SalaryAt = DateTime.Now,
                Sex = Sex.Male,
                AvatarPath = "abc",
                RegisterWorkDay = "abc",
                StartDateAt = DateTime.Now,
                UserCode = "abc"
            };

            WithUnitOfWork(async () =>
            {
                var result = await _HRMAppService.CreateUser(user);

                result.ShouldNotBeNull();
                result.ShouldBe(23);
            });

            WithUnitOfWork(async () =>
            {
                var result = await _workScope.GetAsync<User>(23);

                result.ShouldNotBeNull();
            });
        }*/
        //TODO: fail due to setRole function in userService

        [Fact]
        // Get Branch Id By Code
        public void GetBranchIdByCodeTest1()
        {
            var code = "HN1";
            WithUnitOfWork(async () =>
            {
                var result = await _HRMAppService.GetBranchIdByCode(code);

                var expectBranchId = 1;
                result.ShouldNotBeNull();
                result.ShouldBe(expectBranchId);
            });
        }

        [Fact]
        // Update A Valid User New
        public void UpdateUserNewTest1()
        {
            var user = new CreateUserDto
            {
                Id = 3,
                EmailAddress = "tien.nguyenhuu@ncc.asia",
                Type = Usertype.Staff,
                IsActive = true,
                Sex = Sex.Female,
                Name = "Toại",
                Surname = "Nguyễn",
                Level = UserLevel.SeniorPlus,
                BranchCode = "HN1",
                UserName = "Nguyễn Công Toại",
                AvatarPath = "abc",
                PhoneNumber = "0966668888",
            };

            WithUnitOfWork(async () =>
            {
                await _HRMAppService.UpdateUserNew(user);
            });

            WithUnitOfWork(async () =>
            {
                var result = await _workScope.GetAsync<User>(user.Id);

                result.ShouldNotBeNull();
                result.EmailAddress.ShouldBe(user.EmailAddress);
                result.IsActive.ShouldBeTrue();
                result.Name.ShouldBe(user.Name);
                result.Surname.ShouldBe(user.Surname);
                result.Level.ShouldBe(user.Level);
                result.Sex.ShouldBe(user.Sex);
                result.BranchId.ShouldBe(1);
                result.UserName.ShouldBe(user.UserName);
                result.AvatarPath.ShouldBe(user.AvatarPath);
                result.PhoneNumber.ShouldBe(user.PhoneNumber);
            });
        }

        [Fact]
        // Update A Valid User New Deactive
        public void UpdateUserNewTest2()
        {
            var user = new CreateUserDto
            {
                Id = 3,
                EmailAddress = "tien.nguyenhuu@ncc.asia",
                Type = Usertype.Staff,
                IsActive = false,
                Sex = Sex.Female,
                Name = "Toại",
                Surname = "Nguyễn",
                Level = UserLevel.SeniorPlus,
                BranchCode = "HN1",
                UserName = "Nguyễn Công Toại",
                AvatarPath = "abc",
                PhoneNumber = "0966668888",
            };

            WithUnitOfWork(async () =>
            {
                await _HRMAppService.UpdateUserNew(user);
            });

            WithUnitOfWork(async () =>
            {
                var result = await _workScope.GetAsync<User>(user.Id);
                var projects = _workScope.GetAll<ProjectUser>().Where(s => s.UserId == user.Id).ToList();

                result.ShouldNotBeNull();
                result.EmailAddress.ShouldBe(user.EmailAddress);
                result.IsActive.ShouldBe(user.IsActive);
                result.Name.ShouldBe(user.Name);
                result.Surname.ShouldBe(user.Surname);
                result.Level.ShouldBe(user.Level);
                result.Sex.ShouldBe(user.Sex);
                result.BranchId.ShouldBe(1);
                result.UserName.ShouldBe(user.UserName);
                result.AvatarPath.ShouldBe(user.AvatarPath);
                result.PhoneNumber.ShouldBe(user.PhoneNumber);
                projects.ShouldAllBe(item => item.Type == ProjectUserType.DeActive);
            });
        }

        [Fact]
        // Update A Valid User
        public void UpdateUserTest1()
        {
            var user = new CreateUserDto
            {
                Id = 3,
                EmailAddress = "tien.nguyenhuu@ncc.asia",
                Type = Usertype.Staff,
                IsActive = true,
                Sex = Sex.Female,
                Name = "Toại",
                Surname = "Nguyễn",
                Level = UserLevel.SeniorPlus,
                BranchCode = "HN1",
                UserName = "Nguyễn Công Toại",
                AvatarPath = "abc",
                PhoneNumber = "0966668888",
            };

            WithUnitOfWork(async () =>
            {
                await _HRMAppService.UpdateUserNew(user);
            });

            WithUnitOfWork(async () =>
            {
                var result = await _workScope.GetAsync<User>(user.Id);

                result.ShouldNotBeNull();
                result.EmailAddress.ShouldBe(user.EmailAddress);
                result.IsActive.ShouldBeTrue();
                result.Name.ShouldBe(user.Name);
                result.Surname.ShouldBe(user.Surname);
                result.Level.ShouldBe(user.Level);
                result.Sex.ShouldBe(user.Sex);
                result.BranchId.ShouldBe(1);
                result.UserName.ShouldBe(user.UserName);
                result.AvatarPath.ShouldBe(user.AvatarPath);
            });
        }

        [Fact]
        // Update A Valid User Deactive
        public void UpdateUserTest2()
        {
            var user = new CreateUserDto
            {
                Id = 3,
                EmailAddress = "tien.nguyenhuu@ncc.asia",
                Type = Usertype.Staff,
                IsActive = false,
                Sex = Sex.Female,
                Name = "Toại",
                Surname = "Nguyễn",
                Level = UserLevel.SeniorPlus,
                BranchCode = "HN1",
                UserName = "Nguyễn Công Toại",
                AvatarPath = "abc",
                PhoneNumber = "0966668888",
            };

            WithUnitOfWork(async () =>
            {
                await _HRMAppService.UpdateUserNew(user);
            });

            WithUnitOfWork(async () =>
            {
                var result = await _workScope.GetAsync<User>(user.Id);
                var projects = _workScope.GetAll<ProjectUser>().Where(s => s.UserId == user.Id).ToList();

                result.ShouldNotBeNull();
                result.EmailAddress.ShouldBe(user.EmailAddress);
                result.IsActive.ShouldBe(user.IsActive);
                result.Name.ShouldBe(user.Name);
                result.Surname.ShouldBe(user.Surname);
                result.Level.ShouldBe(user.Level);
                result.Sex.ShouldBe(user.Sex);
                result.BranchId.ShouldBe(1);
                result.UserName.ShouldBe(user.UserName);
                result.AvatarPath.ShouldBe(user.AvatarPath);
                projects.ShouldAllBe(item => item.Type == ProjectUserType.DeActive);
            });
        }

        [Fact]
        // Get All Publishment
        public void GetAllPublishmentTest1()
        {
            var year = 2023;
            var month = 1;
            var unlockType = LockUnlockTimesheetType.ApproveRejectTimesheet;

            WithUnitOfWork(async () =>
            {
                var result = await _HRMAppService.GetAllPublishment(year, month, unlockType);

                result.Count.ShouldBe(1);
                result.ShouldContain(item => item.UserId == 15);
                result[0].Amount.ShouldBe(180000);
                result[0].NormalizedEmailAddress.ShouldBe("HA.PHAMTHI@NCC.ASIA");
                result[0].Times.ShouldBe(9);
                result[0].UserId.ShouldBe(15);
            });
        }

        [Fact]
        // Get Timekeeping By Month
        public void GetTimekeepingByMonthTest1()
        {
            var year = 2022;
            var month = 12;

            WithUnitOfWork(async () =>
            {
                var result = await _HRMAppService.GetTimekeepingByMonth(year, month);

                result.Count.ShouldBe(18);
                result.ShouldContain(item => item.UserId == 1 && item.NormalizedEmailAddress == "ADMIN@ASPNETBOILERPLATE.COM");
            });
        }
    }
}
