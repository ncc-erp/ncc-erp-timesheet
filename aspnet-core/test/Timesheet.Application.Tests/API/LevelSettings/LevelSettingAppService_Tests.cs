using Abp.Configuration;
using Ncc.Configuration;
using Ncc.IoC;
using Shouldly;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Timesheet.APIs.Timesheets.Dto.LevelSettings;
using Timesheet.APIs.Timesheets.LevelSettings;
using Xunit;

namespace Timesheet.Application.Tests.API.LevelSettings
{
    public class LevelSettingAppService_Tests : TimesheetApplicationTestBase
    {
        /// <summary>
        /// 2/2 function 
        /// 2/2 test cases passed
        /// Update day 16/01/2923
        /// </summary>
        private readonly LevelSettingAppService _levelSetting;
        private readonly IWorkScope _work;
        public LevelSettingAppService_Tests()
        {
            _work = Resolve<IWorkScope>();
            _levelSetting = new LevelSettingAppService(_work);
            _levelSetting.SettingManager = Resolve<ISettingManager>();
        }

        [Fact]
        public async Task Should_Get()
        {
            await WithUnitOfWorkAsync(async () =>
            {
                var expected = "[\n{\n\"id\":\"0\",\n\"name\":\"Intern_0\",\n\"salary\": \"0\"\n\n},\n{\n\"id\":\"1\",\n\"name\":\"Intern_600K\",\n\"salary\": \"1000000\"\n\n},\n{\n\"id\":\"2\",\n\"name\":\"Intern_2M\",\n\"salary\": \"2000000\"\n\n},\n{\n\"id\":\"3\",\n\"name\":\"Intern_4M\",\n\"salary\": \"4000000\"\n\n},\n{\n\"id\":\"4\",\n\"name\":\"Fresher-\",\n\"subLevels\":[\n{\n\"id\":\"1\",\n\"name\":\"sub-level 1\",\n\"salary\":\"6000000\"\n},\n{\n\"id\":\"2\",\n\"name\":\"sub-level 2\",\n\"salary\":\"7000000\"\n}\n]\n},\n{\n\"id\":\"5\",\n\"name\":\"Fresher\",\n\"subLevels\":[\n{\n\"id\":\"3\",\n\"name\":\"sub-level 1\",\n\"salary\":\"7000000\"\n},\n{\n\"id\":\"4\",\n\"name\":\"sub-level 2\",\n\"salary\":\"8000000\"\n}\n]\n},\n{\n\"id\":\"6\",\n\"name\":\"Fresher+\",\n\"subLevels\":[\n{\n\"id\":\"5\",\n\"name\":\"sub-level 1\",\n\"salary\":\"8000000\"\n},\n{\n\"id\":\"6\",\n\"name\":\"sub-level 2\",\n\"salary\":\"9000000\"\n}\n]\n}\n]\\";
                var result = await _levelSetting.Get();
                Assert.Contains(result.UserLevelSetting,expected);
                Assert.Equal("85", result.PercentSalaryProbationary);
            });
        }

        [Fact]
        public async Task Should_Set()
        {
            var input = new LevelSettingBaseDto
            {
                UserLevelSetting = "User Level Setting",
                PercentSalaryProbationary = "100"
            };
            await WithUnitOfWorkAsync(async () =>
            {               
                var result = await _levelSetting.Set(input);
                Assert.Equal(input.UserLevelSetting, result.UserLevelSetting);
                Assert.Equal(input.PercentSalaryProbationary, result.PercentSalaryProbationary);
            });
            await WithUnitOfWorkAsync(async () =>
            {
                var UserLevelSetting = await _levelSetting.SettingManager.GetSettingValueForApplicationAsync(AppSettingNames.UserLevelSetting);
                var PercentSalaryProbationary = await _levelSetting.SettingManager.GetSettingValueForApplicationAsync(AppSettingNames.PercentSalaryProbationary);
                Assert.Equal(input.UserLevelSetting, UserLevelSetting);
                Assert.Equal(input.PercentSalaryProbationary, PercentSalaryProbationary);
            });
        }
    }
}
