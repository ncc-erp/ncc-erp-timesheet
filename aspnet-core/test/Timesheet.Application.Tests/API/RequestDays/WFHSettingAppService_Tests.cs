using Abp.Configuration;
using Ncc.IoC;
using Shouldly;
using System.Threading.Tasks;
using Timesheet.APIs.MyAbsenceDays.WFHSetting;
using Timesheet.APIs.MyAbsenceDays.WFHSetting.Dto;
using Xunit;

namespace Timesheet.Application.Tests.API.RequestDays
{
    public class WFHSettingAppService_Tests : TimesheetApplicationTestBase
    {
        private readonly WFHSetting _wfhSetting;

        public WFHSettingAppService_Tests()
        {
            var workScope = Resolve<IWorkScope>();
            _wfhSetting = new WFHSetting(workScope)
            {
                SettingManager = Resolve<ISettingManager>()
            };
        }

        [Fact]
        public async Task Get_Should_Return_Office_And_WFH_Policy_Settings()
        {
            await WithUnitOfWorkAsync(async () =>
            {
                var result = await _wfhSetting.Get();

                result.ShouldNotBeNull();
                result.numOfRemoteDays.ShouldNotBeNullOrEmpty();
                result.officeDaysInAdvanceToRequestRemote.ShouldNotBeNullOrEmpty();
                result.wfhMaxOfficeDaysPerWeek.ShouldNotBeNullOrEmpty();
                result.wfhDaysInAdvanceToRequestOffice.ShouldNotBeNullOrEmpty();
            });
        }

        [Fact]
        public async Task Change_Should_Persist_All_Updated_Policy_Settings()
        {
            var input = new WFHSettingDto
            {
                numOfRemoteDays = "2",
                allowInternToWorkRemote = "true",
                allowProbationToWorkRemote = "false",
                totalTimeTardinessAndEarlyLeave = "2.5",
                officeDaysInAdvanceToRequestRemote = "5",
                wfhMaxOfficeDaysPerWeek = "3",
                wfhDaysInAdvanceToRequestOffice = "4"
            };

            await WithUnitOfWorkAsync(async () =>
            {
                await _wfhSetting.Change(input);
                var updated = await _wfhSetting.Get();

                updated.numOfRemoteDays.ShouldBe("2");
                updated.allowInternToWorkRemote.ShouldBe("true");
                updated.allowProbationToWorkRemote.ShouldBe("false");
                updated.totalTimeTardinessAndEarlyLeave.ShouldBe("2.5");
                updated.officeDaysInAdvanceToRequestRemote.ShouldBe("5");
                updated.wfhMaxOfficeDaysPerWeek.ShouldBe("3");
                updated.wfhDaysInAdvanceToRequestOffice.ShouldBe("4");
            });
        }
    }
}
