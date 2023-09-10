using Abp.Configuration;
using Abp.Domain.Uow;
using Abp.ObjectMapping;
using Abp.Runtime.Session;
using Abp.UI;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Ncc.Authorization.Roles;
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
using Timesheet.APIs.HRMv2;
using Timesheet.APIs.HRMv2.Dto;
using Timesheet.APIs.OverTimeHours;
using Timesheet.APIs.OverTimeHours.Dto;
using Timesheet.DomainServices;
using Timesheet.DomainServices.Dto;
using Timesheet.Entities;
using Timesheet.Services.HRMv2;
using Timesheet.Services.Komu;
using Timesheet.Services.Project.Dto;
using Xunit;
using static Ncc.Entities.Enum.StatusEnum;

namespace Timesheet.Application.Tests.API.HRMV2
{
    /// <summary>
    /// 14/20 functions (6 functions call HRMv2 API - can't test)
    /// 21/21 test cases passed
    /// update day 17/01/2023
    /// </summary>

    public class HRMv2AppService_Test : TimesheetApplicationTestBase
    {
        private readonly HRMv2AppService _appService;
        private readonly IWorkScope _workScope;

        private List<MyTimesheet> listMyTimesheet = new List<MyTimesheet>
        {
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
            }
        };

        public HRMv2AppService_Test()
        {
            var _overTimeHourAppService = Resolve<OverTimeHourAppService>();

            var httpClient = Resolve<HttpClient>();
            var configuration = Substitute.For<IConfiguration>();
            configuration.GetValue<string>("HRMv2Service:BaseAddress").Returns("http://www.myserver.com");
            configuration.GetValue<string>("HRMv2Service:SecurityCode").Returns("secretCode");
            var logger = Resolve<ILogger<HRMv2Service>>();
            var settingManager = Substitute.For<ISettingManager>();
            settingManager.GetSettingValueForApplicationAsync(AppSettingNames.OpenTalkTaskId).Returns(Task.FromResult("2"));
            var _HRMv2Service = Substitute.For<HRMv2Service>(httpClient, configuration, logger);

            var userManager = Resolve<UserManager>();

            var loggerKomu = Resolve<ILogger<KomuService>>();
            configuration.GetValue<string>("KomuService:BaseAddress").Returns("http://www.myserver.com");
            configuration.GetValue<string>("KomuService:SecurityCode").Returns("secretCode");
            configuration.GetValue<string>("KomuService:DevModeChannelId").Returns("_channelIdDevMode");
            configuration.GetValue<string>("KomuService:EnableKomuNotification").Returns("_isNotifyToKomu");
            var komuService = Substitute.For<KomuService>(httpClient, loggerKomu, configuration, settingManager);
            var abpSession = Resolve<IAbpSession>();
            var roleManager = Resolve<RoleManager>();
            var objectMapper = Resolve<IObjectMapper>();
            _workScope = Resolve<IWorkScope>();

            var _userServices = Substitute.For<UserServices>(userManager, komuService, abpSession, roleManager, objectMapper, _workScope);
            _userServices.UnitOfWorkManager = Resolve<IUnitOfWorkManager>();

            _appService = new HRMv2AppService(
                    _overTimeHourAppService,
                    _HRMv2Service,
                    _userServices,
                    _workScope
                )
            {
                UnitOfWorkManager = Resolve<IUnitOfWorkManager>()
            };
            foreach (var ts in listMyTimesheet)
            {
                _workScope.InsertAsync(ts);
            }
            _appService.SettingManager = settingManager;
        }

        [Fact]
        public async Task UpdateAvatarFromHrm_Test()
        {
            UpdateAvatarDto input = new UpdateAvatarDto
            {
                AvatarPath = "abc/abc.jpg",
                EmailAddress = "toai.nguyencong@ncc.asia"
            };

            WithUnitOfWork(() =>
            {
                _appService.UpdateAvatarFromHrm(input);
            });

            WithUnitOfWork(() =>
            {
                var newUserAvtPath = _workScope.GetAll<User>().ToList().Where(u => u.EmailAddress == input.EmailAddress).FirstOrDefault();

                Assert.Equal(newUserAvtPath.AvatarPath, input.AvatarPath);
            });
        }

        [Fact]
        public async Task UpdateAvatarFromHrm_Should_Not_Update_AvartarPath_Not_Exits()
        {
            UpdateAvatarDto input = new UpdateAvatarDto
            {
                AvatarPath = "",
            };

            WithUnitOfWork(() =>
            {
                _appService.UpdateAvatarFromHrm(input);
            });

            WithUnitOfWork(() =>
            {
                var newUserAvtPath = _workScope.GetAll<User>().ToList().Where(u => u.EmailAddress == input.EmailAddress).FirstOrDefault();
                newUserAvtPath.ShouldBeNull();
            });
        }

        [Fact]
        public async Task UpdateAvatarFromHrm_Should_Not_Update_By_EmailAddress_Not_Exits()
        {
            UpdateAvatarDto input = new UpdateAvatarDto
            {
                EmailAddress = ""
            };

            WithUnitOfWork(() =>
            {
                _appService.UpdateAvatarFromHrm(input);
            });

            WithUnitOfWork(() =>
            {
                var newUserAvtPath = _workScope.GetAll<User>().ToList().Where(u => u.EmailAddress == input.EmailAddress).FirstOrDefault();
                newUserAvtPath.ShouldBeNull();
            });
        }

        [Fact]
        public async Task GetPunishmentBasicUserUnlockTS_Test()
        {
            int expectTotalCount = 4;

            await WithUnitOfWorkAsync(async () =>
            {
                //LockUnlockTimesheetType = MyTimesheet;
                var result = await _appService.GetPunishmentBasicUserUnlockTS(2022, 12);
                Assert.Equal(expectTotalCount, result.Count());

                result.Last().Email.ShouldBe("thanh.trantien@ncc.asia");
                result.Last().Money.ShouldBe(120000);
            });
        }

        [Fact]
        public async Task GetPunishmentPMUnlockTS_Test()
        {
            await WithUnitOfWorkAsync(async () =>
            {
                //LockUnlockTimesheetType = ApproveRejectTimesheet;
                var result = await _appService.GetPunishmentPMUnlockTS(2022, 12);

                result.Last().Email.ShouldBe("duong.tranduc@ncc.asia");
                result.Last().Money.ShouldBe(140000);
            });
        }

        [Fact]
        public async Task GetPunishtmentCheckin_Test()
        {
            var expectTotalCount = 1;
            var all1 = new List<Timekeeping>();
            var all2 = new List<Timekeeping>();

            WithUnitOfWork(() =>
           {
               _workScope.Insert<Timekeeping>(new Timekeeping
               {
                   UserId = 22,
                   UserEmail = "hien.ngothu@ncc.asia",
                   CheckIn = "10:00",
                   CheckOut = "17:00",
                   DateAt = DateTime.Parse("2022-12-20"),
                   IsLocked = false,
                   IsDeleted = false,
                   StatusPunish = CheckInCheckOutPunishmentType.Late,
                   MoneyPunish = 20000
               });
           });

            WithUnitOfWork(() =>
            {
                var result = _appService.GetPunishtmentCheckin(2022, 12);

                Assert.Equal(expectTotalCount, result.Count());
                result.Last().Email.ShouldBe("hien.ngothu@ncc.asia");
                result.Last().Money.ShouldBe(20000);
            });
        }

        [Fact]
        public async Task GetAllRequestDay_Test()
        {
            InputCollectDataForPayslipDto input = new InputCollectDataForPayslipDto
            {
                Year = 2022,
                Month = 12,
                UpperEmails = new List<string> { "HIEU.TRANTUNG@NCC.ASIA", "TRANG.VUQUYNH@NCC.ASIA" }
            };

            WithUnitOfWork(() =>
            {
                var result = _appService.GetAllRequestDay(input);
                Assert.Single(result);

                result.First().NormalizedEmailAddress.ShouldBe("TRANG.VUQUYNH@NCC.ASIA");
                result.First().OffDates.Count().ShouldBe(3);
                result.First().OffDates.First().DateAt.Date.ShouldBe(new DateTime(2022, 12, 6).Date);
                result.First().OffDates.First().DayOffTypeId.ShouldBe(2);
                result.First().OffDates.First().DayValue.ShouldBe(1);
                result.First().OffDates.First().LeaveDay.ShouldBe(3);
                result.First().WorkAtHomeOnlyDates.Count().ShouldBe(3);
                result.First().WorkAtHomeOnlyDates.First().Date.ShouldBe(new DateTime(2022, 12, 6).Date);
            });
        }

        [Fact]
        public async Task GetChamCongInfo_Test()
        {
            InputCollectDataForPayslipDto input = new InputCollectDataForPayslipDto
            {
                Year = 2022,
                Month = 12,
                UpperEmails = new List<string> { "HIEU.TRANTUNG@NCC.ASIA", "TRANG.VUQUYNH@NCC.ASIA" }
            };

            WithUnitOfWork(() =>
            {
                var result = _appService.GetChamCongInfo(input);

                result.First().NormalizeEmailAddress.ShouldBe("TRANG.VUQUYNH@NCC.ASIA");
                result.First().NormalWorkingDates.Count().ShouldBe(13);
                result.First().OpenTalkDates.Count().ShouldBe(0);
                result.First().NormalWorkingDates.First().Date.ShouldBe(new DateTime(2022, 12, 26).Date);
            });
        }

        [Fact]
        public async Task GetSettingOffDates_Test()
        {

            WithUnitOfWork(() =>
            {
                var result = _appService.GetSettingOffDates(2022, 12);

                Assert.Equal(4, result.Count());
                result.First().Date.ShouldBe(new DateTime(2022, 12, 4).Date);
            });
        }

        [Fact]
        public async Task GetOTTimesheets_Test()
        {
            var expectTotalCount = 2;
            var input = new InputCollectDataForPayslipDto
            {
                Month = 12,
                Year = 2022,
                UpperEmails = new List<String> { "ADMIN@ASPNETBOILERPLATE.COM", "LINH.NGUYENTHUY@NCC.ASIA" }
            };

            await WithUnitOfWorkAsync(async () =>
            {
                var result = await _appService.GetOTTimesheets(input);
                Assert.Equal(expectTotalCount, result.Count());

                result.First().ListOverTimeHour.Count().ShouldBe(2);
                result.Last().ListOverTimeHour.Count().ShouldBe(3);
                result.Last().ListOverTimeHour.Last().Date.ShouldBe(new DateTime(2022, 12, 19).Date);
                result.Last().ListOverTimeHour.Last().OTHour.ShouldBe(8);
            });
        }

        [Fact]
        public async Task CreateUser_Test()
        {
            var expectId = 0L;
            var expectTotalCount = 0;
            var expectUser = new CreateUpdateByHRMV2Dto
            {
                Name = "Test",
                Surname = "Test",
                EmailAddress = "Test@ncc.asia",
                Sex = Sex.Male,
                WorkingStartDate = DateTime.Now,
                PositionCode = "IT",
                BranchCode = "HN3",
                LevelCode = "Intern_3",
            };

            await WithUnitOfWorkAsync(async () =>
            {
                expectTotalCount = _workScope.GetAll<User>().Count();

                expectId = await _appService.CreateUser(expectUser);
                expectId.ShouldNotBeNull();
            });

            WithUnitOfWork(() =>
            {
                var allUsers = _workScope.GetAll<User>();
                Assert.Equal(expectTotalCount + 1, allUsers.Count());
                var createdUser = allUsers.Where(x => x.Id == expectId).FirstOrDefault();
                createdUser.EmailAddress.ShouldBe(expectUser.EmailAddress);
                createdUser.Name.ShouldBe(expectUser.Name);
                createdUser.Surname.ShouldBe(expectUser.Surname);
                createdUser.Sex.ShouldBe(expectUser.Sex);
                createdUser.Type.ShouldBe(Usertype.Internship);
            });
        }

        [Fact]
        public async Task CreateUser_Should_Not_Create_User_With_EmailAddress_Exist()
        {
            var expectUser = new CreateUpdateByHRMV2Dto
            {
                Name = "Test",
                Surname = "Test",
                EmailAddress = "toai.nguyencong@ncc.asia",
                Sex = Sex.Male,
                Type = Usertype.Staff,
                WorkingStartDate = DateTime.Now,
                PositionCode = "IT",
                BranchCode = "HN3",
                LevelCode = "Intern_3",
            };

            await WithUnitOfWorkAsync(async () =>
            {
                var exception = await Assert.ThrowsAsync<UserFriendlyException>(async () =>
                {
                    var expectId = await _appService.CreateUser(expectUser);
                });
                Assert.Equal($"failed to create user from HRM, user with email {expectUser.EmailAddress} is already exist", exception.Message);
            });
        }

        [Fact]
        public async Task UpdateUser_Test()
        {
            var expectUser = new CreateUpdateByHRMV2Dto
            {
                Name = "Test",
                Surname = "Test",
                EmailAddress = "toai.nguyencong@ncc.asia",
                PositionCode = "IT",
                BranchCode = "HN3",
                LevelCode = "Intern_3",
            };

            await WithUnitOfWorkAsync(async () =>
            {
                await _appService.UpdateUser(expectUser);
            });

            WithUnitOfWork(() =>
            {
                var createdUser = _workScope.GetAll<User>().Where(x => x.EmailAddress == expectUser.EmailAddress).FirstOrDefault();
                createdUser.EmailAddress.ShouldBe(expectUser.EmailAddress);
                createdUser.Name.ShouldBe(expectUser.Name);
                createdUser.Surname.ShouldBe(expectUser.Surname);
                createdUser.Sex.ShouldBe(expectUser.Sex);
            });
        }

        [Fact]
        public async Task ConfirmUserQuit_Test()
        {
            var input = new UpdateUserStatusFromHRMDto
            {
                EmailAddress = "hien.ngothu@ncc.asia",
                DateAt = DateTime.Now,
            };

            await WithUnitOfWorkAsync(async () =>
            {
                var result = await _appService.ConfirmUserQuit(input);
                result.ToString().ShouldBe(input.ToString());

                var updatedUser = _workScope.GetAll<User>()
                .Where(x => x.EmailAddress.ToLower().Trim() == input.EmailAddress.ToLower().Trim()).First();
                updatedUser.IsActive.ShouldBeFalse();
                updatedUser.IsStopWork.ShouldBeTrue();
            });
        }

        [Fact]
        public async Task ConfirmUserQuit_Should_Throw_Exception()
        {
            var input = new UpdateUserStatusFromHRMDto
            {
                EmailAddress = "toai.nguyencon@ncc.asia",
                DateAt = DateTime.Now,
            };

            await WithUnitOfWorkAsync(async () =>
            {
                var exception = await Assert.ThrowsAsync<UserFriendlyException>(async () =>
                {
                    await _appService.ConfirmUserQuit(input);
                });
                Assert.Equal("Can't found user with the same email with HRM Tool", exception.Message);
            });
        }

        [Fact]
        public async Task ConfirmUserPause_Test()
        {
            var input = new UpdateUserStatusFromHRMDto
            {
                EmailAddress = "hien.ngothu@ncc.asia",
                DateAt = DateTime.Now,
            };

            await WithUnitOfWorkAsync(async () =>
            {
                var result = await _appService.ConfirmUserPause(input);
                result.ToString().ShouldBe(input.ToString());

                var updatedUser = _workScope.GetAll<User>()
                .Where(x => x.EmailAddress.ToLower().Trim() == input.EmailAddress.ToLower().Trim()).First();
                updatedUser.IsActive.ShouldBeTrue();
                updatedUser.IsStopWork.ShouldBeTrue();
            });
        }

        [Fact]
        public async Task ConfirmUserPause_Should_Throw_Exception()
        {
            var input = new UpdateUserStatusFromHRMDto
            {
                EmailAddress = "toai.nguyencon@ncc.asia",
                DateAt = DateTime.Now,
            };

            await WithUnitOfWorkAsync(async () =>
            {
                var exception = await Assert.ThrowsAsync<UserFriendlyException>(async () =>
                {
                    await _appService.ConfirmUserPause(input);
                });
                Assert.Equal("Can't found user with the same email with HRM Tool", exception.Message);
            });
        }

        [Fact]
        public async Task ConfirmUserMaternityLeave_Test()
        {
            var input = new UpdateUserStatusFromHRMDto
            {
                EmailAddress = "hien.ngothu@ncc.asia",
                DateAt = DateTime.Now,
            };

            await WithUnitOfWorkAsync(async () =>
            {
                var result = await _appService.ConfirmUserMaternityLeave(input);
                result.ToString().ShouldBe(input.ToString());

                var updatedUser = _workScope.GetAll<User>()
                .Where(x => x.EmailAddress.ToLower().Trim() == input.EmailAddress.ToLower().Trim()).First();
                updatedUser.IsActive.ShouldBeTrue();
                updatedUser.IsStopWork.ShouldBeTrue();
            });
        }

        [Fact]
        public async Task ConfirmUserMaternityLeave_Should_Throw_Exception()
        {
            var input = new UpdateUserStatusFromHRMDto
            {
                EmailAddress = "toai.nguyencon@ncc.asia",
                DateAt = DateTime.Now,
            };

            await WithUnitOfWorkAsync(async () =>
            {
                var exception = await Assert.ThrowsAsync<UserFriendlyException>(async () =>
                {
                    await _appService.ConfirmUserMaternityLeave(input);
                });
                Assert.Equal("Can't found user with the same email with HRM Tool", exception.Message);
            });
        }

        [Fact]
        public async Task ConfirmUserBackToWork_Test()
        {
            var input = new UpdateUserStatusFromHRMDto
            {
                EmailAddress = "hien.ngothu@ncc.asia",
                DateAt = DateTime.Now,
            };

            await WithUnitOfWorkAsync(async () =>
            {
                var result = await _appService.ConfirmUserBackToWork(input);
                result.ToString().ShouldBe(input.ToString());

                var updatedUser = _workScope.GetAll<User>()
                .Where(x => x.EmailAddress.ToLower().Trim() == input.EmailAddress.ToLower().Trim()).First();
                updatedUser.IsActive.ShouldBeTrue();
                updatedUser.IsStopWork.ShouldBeFalse();
            });
        }

        [Fact]
        public async Task ConfirmUserBackToWork_Should_Throw_Exception()
        {
            var input = new UpdateUserStatusFromHRMDto
            {
                EmailAddress = "toai.nguyencon@ncc.asia",
                DateAt = DateTime.Now,
            };

            await WithUnitOfWorkAsync(async () =>
            {
                var exception = await Assert.ThrowsAsync<UserFriendlyException>(async () =>
                {
                    await _appService.ConfirmUserBackToWork(input);
                });
                Assert.Equal("Can't found user with the same email with HRM Tool", exception.Message);
            });
        }

        [Fact]

        public void GetDicUserIdToOpentalkCount_Test1()
        {
            int year = 2023;
            int month = 6;

            Dictionary<string, int> output = new Dictionary<string, int>();

            output.Add("HIEU.TRANTRUNG@NCC.ASIA", 2);

            WithUnitOfWork(() =>
           {
               var result = _appService.GetDicUserIdToOpentalkCount(year, month);

               Assert.Equal(output, result);
           });
        }

        [Fact]
        public void GetDicUserIdToOpentalkCount_Test2()
        {
            int year = 2023;
            int month = 7;

            Dictionary<string, int> output = new Dictionary<string, int>();

            output.Add("HIEU.TRANTRUNG@NCC.ASIA", 2);

            WithUnitOfWork(() =>
            {
                var result = _appService.GetDicUserIdToOpentalkCount(year, month);

                Assert.Equal(output, result);
            });
        }

        [Fact]
        public void GetChamCongInfo_Test1()
        {
            InputCollectDataForPayslipDto input = new InputCollectDataForPayslipDto
            {
                Year = 2023,
                Month = 7,
                UpperEmails = new List<string> { "HIEU.TRANTRUNG@NCC.ASIA" }
            };

            WithUnitOfWork(() =>
            {
                var result = _appService.GetChamCongInfo(input);

                result.First().NormalizeEmailAddress.ShouldBe("HIEU.TRANTRUNG@NCC.ASIA");
                result.First().NormalWorkingDates.Count().ShouldBe(1);
                result.First().OpenTalkDates.Count().ShouldBe(3);
            });
        }

        // Case 1: 2 opentalk thứ bảy, 5 ngày đi làm
        [Fact]
        public void GetChamCongInfo_Test2()
        {
            InputCollectDataForPayslipDto input = new InputCollectDataForPayslipDto
            {
                Year = 2023,
                Month = 8,
                UpperEmails = new List<string> { "HIEU.TRANTRUNG@NCC.ASIA" }
            };

            WithUnitOfWork(() =>
            {
                var result = _appService.GetChamCongInfo(input);

                result.First().NormalizeEmailAddress.ShouldBe("HIEU.TRANTRUNG@NCC.ASIA");
                result.First().NormalWorkingDates.Count().ShouldBe(5);
                result.First().OpenTalkDates.Count().ShouldBe(2);
            });
        }

        // Case 2: 1 opentalk thứ bảy, 1 opentalk ngày thường,  5 ngày đi làm
        [Fact]
        public void GetChamCongInfo_Test3()
        {
            InputCollectDataForPayslipDto input = new InputCollectDataForPayslipDto
            {
                Year = 2023,
                Month = 9,
                UpperEmails = new List<string> { "HIEU.TRANTRUNG@NCC.ASIA" }
            };

            WithUnitOfWork(() =>
            {
                var result = _appService.GetChamCongInfo(input);

                result.First().NormalizeEmailAddress.ShouldBe("HIEU.TRANTRUNG@NCC.ASIA");
                result.First().NormalWorkingDates.Count().ShouldBe(5);
                result.First().OpenTalkDates.Count().ShouldBe(2);
            });
        }

        // Case 3: 2 opentalk ngày thường,  5 ngày đi làm
        [Fact]
        public void GetChamCongInfo_Test4()
        {
            InputCollectDataForPayslipDto input = new InputCollectDataForPayslipDto
            {
                Year = 2023,
                Month = 10,
                UpperEmails = new List<string> { "HIEU.TRANTRUNG@NCC.ASIA" }
            };

            WithUnitOfWork(() =>
            {
                var result = _appService.GetChamCongInfo(input);

                result.First().NormalizeEmailAddress.ShouldBe("HIEU.TRANTRUNG@NCC.ASIA");
                result.First().NormalWorkingDates.Count().ShouldBe(5);
                result.First().OpenTalkDates.Count().ShouldBe(2);
            });
        }

        // Case 4: 1 opentalk thứ bảy, 2 opentalk ngày thường, 5 ngày đi làm
        [Fact]
        public void GetChamCongInfo_Test5()
        {
            InputCollectDataForPayslipDto input = new InputCollectDataForPayslipDto
            {
                Year = 2023,
                Month = 11,
                UpperEmails = new List<string> { "HIEU.TRANTRUNG@NCC.ASIA" }
            };

            WithUnitOfWork(() =>
            {
                var result = _appService.GetChamCongInfo(input);

                result.First().NormalizeEmailAddress.ShouldBe("HIEU.TRANTRUNG@NCC.ASIA");
                result.First().NormalWorkingDates.Count().ShouldBe(5);
                result.First().OpenTalkDates.Count().ShouldBe(3);
            });
        }


        [Fact]
        public void ProcessOpentalkOneUserByNewWay_Test1()
        {
            // Danh sách các thứ bảy trong tháng 11/2023
            var listSaturdayDate = new List<DateTime>
            {
                new DateTime(2023, 11, 04, 0, 0, 0, DateTimeKind.Utc),
                new DateTime(2023, 11, 11, 0, 0, 0, DateTimeKind.Utc),
                new DateTime(2023, 11, 18, 0, 0, 0, DateTimeKind.Utc),
                new DateTime(2023, 11, 25, 0, 0, 0, DateTimeKind.Utc),
            };

            var input = new ChamCongInfoDto
            {
                NormalizeEmailAddress = "HIEU.TRANTRUNG@NCC.ASIA",
                OpenTalkDates = new List<DateTime>(),
                NormalWorkingDates = new List<DateTime>
                {
                    new DateTime(2023, 11, 03, 0, 0, 0, DateTimeKind.Utc)
                },
                StartWorkingDate = null,
                StopWorkingDate = null
            };

            // case 1: 2 buổi opentalk thứ 7, không có buổi opentalk trong tuần
            DateTime openTalkDate1 = new DateTime(2023, 11, 04, 0, 0, 0, DateTimeKind.Utc); // Saturday, 04-11-2023
            DateTime openTalkDate2 = new DateTime(2023, 11, 11, 0, 0, 0, DateTimeKind.Utc); // Saturday, 11-11-2023
            input.OpenTalkDates.Add(openTalkDate1);
            input.OpenTalkDates.Add(openTalkDate2);

            _appService.ProcessOpentalkOneUserByNewWay(input, 0, listSaturdayDate);

            Assert.Equal(2, input.OpenTalkDates.Count());
            Assert.Equal(openTalkDate1, input.OpenTalkDates[0]);
            Assert.Equal(openTalkDate2, input.OpenTalkDates[1]);

            // case 2: 1 buổi opentalk thứ 7, 1 buổi opentalk trong tuần
            input.OpenTalkDates = new List<DateTime>();
            DateTime openTalkDate3 = new DateTime(2023, 11, 04, 0, 0, 0, DateTimeKind.Utc); // Saturday, 04-11-2023
            input.OpenTalkDates.Add(openTalkDate3);

            _appService.ProcessOpentalkOneUserByNewWay(input, 1, listSaturdayDate);

            Assert.Equal(2, input.OpenTalkDates.Count());
            Assert.Equal(openTalkDate3, input.OpenTalkDates[0]);
            Assert.Equal(listSaturdayDate[1], input.OpenTalkDates[1]);

            // case 3: 1 buổi opentalk thứ 7, 2 buổi opentalk trong tuần
            input.OpenTalkDates = new List<DateTime>();
            DateTime openTalkDate4 = new DateTime(2023, 11, 04, 0, 0, 0, DateTimeKind.Utc); // Saturday, 04-11-2023
            input.OpenTalkDates.Add(openTalkDate3);

            _appService.ProcessOpentalkOneUserByNewWay(input, 2, listSaturdayDate);

            Assert.Equal(3, input.OpenTalkDates.Count());
            Assert.Equal(openTalkDate3, input.OpenTalkDates[0]);
            Assert.Equal(listSaturdayDate[1], input.OpenTalkDates[1]);
            Assert.Equal(listSaturdayDate[2], input.OpenTalkDates[2]);

            // case 4: 2 buổi opentalk trong tuần
            input.OpenTalkDates = new List<DateTime>();

            _appService.ProcessOpentalkOneUserByNewWay(input, 2, listSaturdayDate);

            Assert.Equal(2, input.OpenTalkDates.Count());
            Assert.Equal(listSaturdayDate[0], input.OpenTalkDates[0]);
            Assert.Equal(listSaturdayDate[1], input.OpenTalkDates[1]);

            // case 5: 3 buổi opentalk trong tuần
            input.OpenTalkDates = new List<DateTime>();

            _appService.ProcessOpentalkOneUserByNewWay(input, 3, listSaturdayDate);

            Assert.Equal(3, input.OpenTalkDates.Count());
            Assert.Equal(listSaturdayDate[0], input.OpenTalkDates[0]);
            Assert.Equal(listSaturdayDate[1], input.OpenTalkDates[1]);
            Assert.Equal(listSaturdayDate[2], input.OpenTalkDates[2]);

            // case 6: 4 buổi opentalk trong tuần
            input.OpenTalkDates = new List<DateTime>();

            _appService.ProcessOpentalkOneUserByNewWay(input, 4, listSaturdayDate);

            Assert.Equal(4, input.OpenTalkDates.Count());
            Assert.Equal(listSaturdayDate[0], input.OpenTalkDates[0]);
            Assert.Equal(listSaturdayDate[1], input.OpenTalkDates[1]);
            Assert.Equal(listSaturdayDate[2], input.OpenTalkDates[2]);
            Assert.Equal(listSaturdayDate[3], input.OpenTalkDates[3]);

            // case 7: 5 buổi opentalk trong tuần
            input.OpenTalkDates = new List<DateTime>();

            _appService.ProcessOpentalkOneUserByNewWay(input, 5, listSaturdayDate);

            Assert.Equal(4, input.OpenTalkDates.Count());
            Assert.Equal(listSaturdayDate[0], input.OpenTalkDates[0]);
            Assert.Equal(listSaturdayDate[1], input.OpenTalkDates[1]);
            Assert.Equal(listSaturdayDate[2], input.OpenTalkDates[2]);
            Assert.Equal(listSaturdayDate[3], input.OpenTalkDates[3]);
        }

        // Test case có StartWorkingDate, EndWorkingDate  
        [Fact]
        public void ProcessOpentalkOneUserByNewWay_Test2()
        {
            // Danh sách các thứ bảy trong tháng 11/2023
            var listSaturdayDate = new List<DateTime>
            {
                new DateTime(2023, 11, 04, 0, 0, 0, DateTimeKind.Utc),
                new DateTime(2023, 11, 11, 0, 0, 0, DateTimeKind.Utc),
                new DateTime(2023, 11, 18, 0, 0, 0, DateTimeKind.Utc),
                new DateTime(2023, 11, 25, 0, 0, 0, DateTimeKind.Utc),
            };

            var input = new ChamCongInfoDto
            {
                NormalizeEmailAddress = "HIEU.TRANTRUNG@NCC.ASIA",
                OpenTalkDates = new List<DateTime>(),
                NormalWorkingDates = new List<DateTime>
                {
                    new DateTime(2023, 11, 03, 0, 0, 0, DateTimeKind.Utc)
                },
                StartWorkingDate = new DateTime(),
                StopWorkingDate = new DateTime(),
            };

            // case 1: 1 buổi opentalk trong tuần, StartWorkingDate = null, EndWorkingDate = null
            input.StartWorkingDate = null;
            input.StopWorkingDate = null; 

            _appService.ProcessOpentalkOneUserByNewWay(input, 1, listSaturdayDate);

            Assert.Equal(1, input.OpenTalkDates.Count());
            Assert.Equal(listSaturdayDate[0], input.OpenTalkDates[0]);


            // case 2: 1 buổi opentalk trong tuần, StartWorkingDate trước thứ bảy đầu tiên của tháng tính lương, EndWorkingDate = null
            input.OpenTalkDates = new List<DateTime>();
            input.StartWorkingDate = new DateTime(2023, 11, 01, 0, 0, 0, DateTimeKind.Utc);

            _appService.ProcessOpentalkOneUserByNewWay(input, 1, listSaturdayDate);

            Assert.Equal(1, input.OpenTalkDates.Count());
            Assert.Equal(listSaturdayDate[0], input.OpenTalkDates[0]);


            // case 3: 1 buổi opentalk trong tuần, StartWorkingDate sau thứ bảy đầu tiên của tháng tính lương, EndWorkingDate = null
            input.OpenTalkDates = new List<DateTime>();
            input.StartWorkingDate = new DateTime(2023, 11, 06, 0, 0, 0, DateTimeKind.Utc);

            _appService.ProcessOpentalkOneUserByNewWay(input, 1, listSaturdayDate);

            Assert.Equal(1, input.OpenTalkDates.Count());
            Assert.Equal(listSaturdayDate[1], input.OpenTalkDates[0]);


            // case 4: 1 buổi opentalk trong tuần, StartWorkingDate trước thứ bảy đầu tiên của tháng tính lương, EndWorkingDate sau tháng tính lương
            input.OpenTalkDates = new List<DateTime>();
            input.StartWorkingDate = new DateTime(2023, 11, 01, 0, 0, 0, DateTimeKind.Utc);
            input.StopWorkingDate = new DateTime(2023, 12, 01, 0, 0, 0, DateTimeKind.Utc);

            _appService.ProcessOpentalkOneUserByNewWay(input, 1, listSaturdayDate);

            Assert.Equal(1, input.OpenTalkDates.Count());
            Assert.Equal(listSaturdayDate[0], input.OpenTalkDates[0]);


            // case 5: 1 buổi opentalk trong tuần, StartWorkingDate trước thứ bảy đầu tiên của tháng tính lương, EndWorkingDate trước thứ bảy cuối cùng của tháng tính lương
            input.OpenTalkDates = new List<DateTime>();
            input.StartWorkingDate = new DateTime(2023, 11, 01, 0, 0, 0, DateTimeKind.Utc);
            input.StopWorkingDate = new DateTime(2023, 11, 24, 0, 0, 0, DateTimeKind.Utc);

            _appService.ProcessOpentalkOneUserByNewWay(input, 1, listSaturdayDate);

            Assert.Equal(1, input.OpenTalkDates.Count());
            Assert.Equal(listSaturdayDate[0], input.OpenTalkDates[0]);
        }
    }
}
