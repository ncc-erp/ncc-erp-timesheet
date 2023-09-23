using Abp.Configuration;
using Ncc.IoC;
using Xunit;
using Timesheet.APIs.Timesheets.SendKomuPunishedUserCheckInOutSetting;
using Timesheet.APIs.Timesheets.SendKomuPunishedUserCheckInOutSetting.Dto;

namespace Timesheet.Application.Tests.API.TimeSheets.SendKomuPunishedUserCheckInSettingAppServices
{
    /* <Summary>
    2/2 functions
    2 passed testcase
    Updated date: 16/11/2023
    </Summary>*/

    public class SendKomuPunishedUserCheckInSettingAppServiceTest : TimesheetApplicationTestBase
    {
        private readonly SendKomuPunishedUserCheckInSettingAppService _sendKomuPunishedUser;

        public SendKomuPunishedUserCheckInSettingAppServiceTest()
        {
            var workScope = Resolve<IWorkScope>();
            _sendKomuPunishedUser = new SendKomuPunishedUserCheckInSettingAppService(workScope);
            _sendKomuPunishedUser.SettingManager = Resolve<ISettingManager>();
        }


        [Fact]
        public async void Get()
        {
            var expectedTimeSendPunishUser = "14";
            var expectedChannelNotifyPunishUser = "";
            var expectedPercentOfTrackerOnWorking = "90";

            await WithUnitOfWorkAsync(async () =>
            {
                var result = await _sendKomuPunishedUser.GetPunishedCheckInConfig();

                Assert.Equal(expectedTimeSendPunishUser, result.TimeSendPunishUser);
                Assert.Equal(expectedChannelNotifyPunishUser, result.ChannelNotifyPunishUser);
                Assert.Equal(expectedPercentOfTrackerOnWorking, result.PercentOfTrackerOnWorking);
            });
        }
        [Fact]
        public async void Change()
        {
            var komuSendNotifyPunishedUser = new KomuSendNotifyPunishedUser
            {
                TimeSendPunishUser = "45",
                ChannelNotifyPunishUser = "IMS",
                PercentOfTrackerOnWorking = "95",
            };
            await WithUnitOfWorkAsync(async () =>
            {
                var result = await _sendKomuPunishedUser.ChangePunishedCheckInConfig(komuSendNotifyPunishedUser);

                Assert.Equal(komuSendNotifyPunishedUser.TimeSendPunishUser, result.TimeSendPunishUser);
                Assert.Equal(komuSendNotifyPunishedUser.ChannelNotifyPunishUser, result.ChannelNotifyPunishUser);
                Assert.Equal(komuSendNotifyPunishedUser.PercentOfTrackerOnWorking, result.PercentOfTrackerOnWorking);
            });
        }
    }
}
