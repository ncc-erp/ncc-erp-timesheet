using Abp.Configuration;
using Microsoft.Extensions.Configuration;
using Ncc.IoC;
using Shouldly;
using System;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Threading.Tasks;
using Timesheet.APIs.Timesheets.EmailSaoDoSetting.Dto;
using Xunit;

namespace Timesheet.Application.Tests.API.TimeSheets.EmailSaoDoSetting
{
    /// <summary>
    /// 2/2 functions
    /// 2/2 test cases passed
    /// update day 16/01/2023
    /// </summary>
    /// 
    public class EmailSaoDoSetting_Tests : TimesheetApplicationTestBase
    {
        private readonly APIs.Timesheets.EmailSaoDoSetting.EmailSaoDoSetting _emailSaoDoSetting;
        private readonly IWorkScope _workScope;

        public EmailSaoDoSetting_Tests()
        {
            _workScope = Resolve<IWorkScope>();
            _emailSaoDoSetting = Resolve<APIs.Timesheets.EmailSaoDoSetting.EmailSaoDoSetting>(_workScope);
        }

        [Fact]
        public async Task Get_Happy_Test()
        {
            await WithUnitOfWorkAsync(async () =>
            {
                var result = await _emailSaoDoSetting.Get();
                result.CanSendEmailToSaoDo.ShouldBe("true");
                result.EmailSaoDo.ShouldBe(Configuration["DefaultSettings:EmailSaoDo"]);
            });
        }

        [Fact]
        public async Task Change_Happy_Test()
        {
            var expectTotalCount = 0;

            var emaiSaoDo = new EmaiSaoDoDto
            {
                CanSendEmailToSaoDo = "false",
                EmailSaoDo = "saodotest@ncc.asia"
            };

            await WithUnitOfWorkAsync(async () =>
            {
                expectTotalCount = _workScope.GetAll<Setting>().Count();

                var result = await _emailSaoDoSetting.Change(emaiSaoDo);
                result.CanSendEmailToSaoDo.ShouldBe("false");
                result.EmailSaoDo.ShouldBe("saodotest@ncc.asia");
            });

            WithUnitOfWork(() =>
            {
                var result = _workScope.GetAll<Setting>().ToList();
                Assert.Equal(expectTotalCount + 2, result.Count());

                result.Find(x => x.Name.Equals("App.SendEmailToSaoDo")).Value.ShouldBe("false");
                result.Find(x => x.Name.Equals("App.EmailSaoDo")).Value.ShouldBe("saodotest@ncc.asia");
            });
        }
    }
}
