using Abp.Application.Services.Dto;
using Ncc.Users;
using Shouldly;
using System.Linq;
using System.Threading.Tasks;
using Timesheet.DomainServices;
using Timesheet.DomainServices.Dto;
using Xunit;
using static Ncc.Entities.Enum.StatusEnum;

namespace Timesheet.Application.Tests.API.Users
{
    public class UserAppService_WorkMode_Tests : UserAppServiceTestBase
    {
        private readonly UserAppService _userAppService;
        private readonly IUserServices _userServices;

        public UserAppService_WorkMode_Tests()
        {
            _userAppService = InstanceUserAppService();
            _userServices = Resolve<IUserServices>();
        }

        [Fact]
        public async Task CreateUserAsync_Should_Persist_WorkMode_WFH_And_Default_Office()
        {
            long branchId = 0;
            long positionId = 0;
            UsingDbContext(ctx =>
            {
                branchId = ctx.Branchs.First().Id;
                positionId = ctx.Positions.First().Id;
            });

            long wfhUserId = 0;
            long officeUserId = 0;

            await WithUnitOfWorkAsync(async () =>
            {
                var wfhUser = await _userServices.CreateUserAsync(new CreateUserDto
                {
                    UserName = "wfh_user_ut",
                    Name = "WFH",
                    Surname = "Tester",
                    EmailAddress = "wfh_user_ut@ncc.asia",
                    Password = "Password123!",
                    IsActive = true,
                    BranchId = branchId,
                    PositionId = positionId,
                    MorningStartAt = "08:30",
                    MorningEndAt = "12:00",
                    MorningWorking = "3.5",
                    AfternoonStartAt = "13:00",
                    AfternoonEndAt = "17:30",
                    AfternoonWorking = "4.5",
                    WorkMode = WorkMode.WFH
                });
                wfhUserId = wfhUser.Id;

                var officeUser = await _userServices.CreateUserAsync(new CreateUserDto
                {
                    UserName = "office_user_ut",
                    Name = "Office",
                    Surname = "Tester",
                    EmailAddress = "office_user_ut@ncc.asia",
                    Password = "Password123!",
                    IsActive = true,
                    BranchId = branchId,
                    PositionId = positionId,
                    MorningStartAt = "08:30",
                    MorningEndAt = "12:00",
                    MorningWorking = "3.5",
                    AfternoonStartAt = "13:00",
                    AfternoonEndAt = "17:30",
                    AfternoonWorking = "4.5"
                });
                officeUserId = officeUser.Id;
            });

            UsingDbContext(context =>
            {
                var wfhUser = context.Users.Find(wfhUserId);
                wfhUser.ShouldNotBeNull();
                wfhUser.WorkMode.ShouldBe(WorkMode.WFH);

                var officeUser = context.Users.Find(officeUserId);
                officeUser.ShouldNotBeNull();
                officeUser.WorkMode.ShouldBe(WorkMode.Office);
            });
        }

        [Fact]
        public async Task UpdateUserAsync_Should_Change_User_WorkMode_Between_Office_And_WFH()
        {
            var userId = await CreateUser("toggle_workmode_ut");
            long branchId = 0;
            long positionId = 0;
            UsingDbContext(ctx =>
            {
                branchId = ctx.Branchs.First().Id;
                positionId = ctx.Positions.First().Id;
                var u = ctx.Users.Find(userId);
                u.BranchId = branchId;
                u.PositionId = positionId;
                u.MorningStartAt = "08:30";
                u.MorningEndAt = "12:00";
                u.MorningWorking = 3.5;
                u.AfternoonStartAt = "13:00";
                u.AfternoonEndAt = "17:30";
                u.AfternoonWorking = 4.5;
                u.isWorkingTimeDefault = false;
            });

            await WithUnitOfWorkAsync(async () =>
            {
                await _userServices.UpdateUserAsync(new UserDto
                {
                    Id = userId,
                    UserName = "toggle_workmode_ut",
                    Name = "Test",
                    Surname = "User",
                    EmailAddress = "toggle_workmode_ut@test.com",
                    IsActive = true,
                    BranchId = branchId,
                    PositionId = positionId,
                    isWorkingTimeDefault = false,
                    WorkMode = WorkMode.WFH
                }, false);
            });

            UsingDbContext(context =>
            {
                context.Users.Find(userId).WorkMode.ShouldBe(WorkMode.WFH);
            });

            await WithUnitOfWorkAsync(async () =>
            {
                await _userServices.UpdateUserAsync(new UserDto
                {
                    Id = userId,
                    UserName = "toggle_workmode_ut",
                    Name = "Test",
                    Surname = "User",
                    EmailAddress = "toggle_workmode_ut@test.com",
                    IsActive = true,
                    BranchId = branchId,
                    PositionId = positionId,
                    isWorkingTimeDefault = false,
                    WorkMode = WorkMode.Office
                }, false);
            });

            UsingDbContext(context =>
            {
                context.Users.Find(userId).WorkMode.ShouldBe(WorkMode.Office);
            });
        }

        [Fact]
        public async Task Get_Should_Return_Correct_WorkMode_On_UserDto()
        {
            var userId = await CreateUser("get_wfh_ut");
            UsingDbContext(context =>
            {
                var user = context.Users.Find(userId);
                user.WorkMode = WorkMode.WFH;
            });

            await WithUnitOfWorkAsync(async () =>
            {
                var dto = await _userAppService.Get(new EntityDto<long>(userId));
                dto.ShouldNotBeNull();
                dto.WorkMode.ShouldBe(WorkMode.WFH);
            });
        }
    }
}
