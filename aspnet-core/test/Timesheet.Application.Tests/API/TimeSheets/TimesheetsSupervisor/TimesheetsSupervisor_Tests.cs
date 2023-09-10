using Ncc.IoC;
using Shouldly;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Timesheet.Entities;
using Timesheet.Extension;
using Xunit;
using static Ncc.Entities.Enum.StatusEnum;
namespace Timesheet.Application.Tests.API.TimeSheets.TimesheetsSupervisor
{
    /// <summary>
    /// 4/4 functions
    /// 6/6 test cases passed
    /// update day 13/01/2023
    /// </summary>
   
    public class TimesheetsSupervisor_Tests : TimesheetApplicationTestBase
    {
        private readonly Timesheet.Timesheets.TimesheetsSupervisor.TimesheetsSupervisor _timesheetsSupervisor;
        private readonly IWorkScope _workScope;

        private List<UnlockTimesheet> listUnlockTimesheet = new List<UnlockTimesheet>
        {
            new UnlockTimesheet
            {
               UserId = 6,
               Type = LockUnlockTimesheetType.ApproveRejectTimesheet,
            },
            new UnlockTimesheet
            {
               UserId = 1,
               Type = LockUnlockTimesheetType.ApproveRejectTimesheet,
            }
        };

        public TimesheetsSupervisor_Tests()
        {
            _workScope = Resolve<IWorkScope>();
            _timesheetsSupervisor = Resolve<Timesheet.Timesheets.TimesheetsSupervisor.TimesheetsSupervisor>(_workScope);

            foreach (var item in listUnlockTimesheet)
            {
                _workScope.InsertAsync(item);
            }
        }

        [Fact]
        public async Task GetAll_Happy_Test()
        {
            var expectTotalCount = 31;

            await WithUnitOfWorkAsync(async () =>
            {
                var result = await _timesheetsSupervisor.GetAll(DateTime.Parse("2022/12/01"), DateTime.Parse("2022/12/31"), TimesheetStatus.Approve);
                Assert.Equal(expectTotalCount, result.Count);

                var myTimeSheet = result.Last();
                myTimeSheet.Id.ShouldBe(46);
                myTimeSheet.CustomerName.ShouldBe("abc");
                myTimeSheet.ProjectCode.ShouldBe("Project abc");
                myTimeSheet.ProjectId.ShouldBe(5);
                myTimeSheet.ProjectName.ShouldBe("Project abc");
                myTimeSheet.Status.ShouldBe(TimesheetStatus.Approve);
                myTimeSheet.TaskId.ShouldBe(1);
                myTimeSheet.TaskName.ShouldBe("Coding");
                myTimeSheet.TypeOfWork.ShouldBe(TypeOfWork.NormalWorkingHours);
                myTimeSheet.DateAt.Date.ShouldBe(new DateTime(2022,12,26).Date);
                myTimeSheet.User.ShouldBe("Hiếu Trần Trung");
                myTimeSheet.UserId.ShouldBe(6);
                myTimeSheet.WorkType.ShouldBe("Official");
                myTimeSheet.WorkingTime.ShouldBe(480);
                myTimeSheet.IsCharged.ShouldBe(false);
                myTimeSheet.IsOffDay.ShouldBe(false);
                myTimeSheet.IsTemp.ShouldBe(false);
                myTimeSheet.IsUserInProject.ShouldBe(false);
                myTimeSheet.ListPM.Count().ShouldBe(2);
                myTimeSheet.ListPM.First().ShouldBe("admin admin");
            });
        }

        [Fact]
        public async Task GetAll_Empty_Data_Test()
        {
            var expectTotalCount = 0;

            await WithUnitOfWorkAsync(async () =>
            {
                var result = await _timesheetsSupervisor.GetAll(DateTime.Parse("2022/10/01"), DateTime.Parse("2022/10/30"), TimesheetStatus.Pending);
                Assert.Equal(expectTotalCount, result.Count);
            });
        }

        [Fact]
        public async Task GetQuantityTimesheetSupervisorStatus_Happy_Test()
        {
            var expectTotalCount = 5;

            await WithUnitOfWorkAsync(async () =>
            {
                var result = await _timesheetsSupervisor.GetQuantityTimesheetSupervisorStatus(DateTime.Parse("2022/12/01"), DateTime.Parse("2022/12/31"));
                List<Object> list = new List<Object>((IEnumerable<Object>)result);
                Assert.Equal(expectTotalCount, list.Count);

                list[0].ToString().ShouldBe("{ Status = All, Quantity = 0 }");
                list[1].ToString().ShouldBe("{ Status = Approve, Quantity = 31 }");
                list[2].ToString().ShouldBe("{ Status = None, Quantity = 0 }");
                list[3].ToString().ShouldBe("{ Status = Pending, Quantity = 17 }");
                list[4].ToString().ShouldBe("{ Status = Reject, Quantity = 2 }");
            });
        }

        [Fact]
        public async Task UnlockPM_Happy_Test_1()
        {
            var id = 1;
            long expectTotalCount = 0L;

            await WithUnitOfWorkAsync(async () =>
             {
                 expectTotalCount = _workScope.GetAll<UnlockTimesheet>().Count();
                 await _timesheetsSupervisor.UnlockPM(id);
             });

            WithUnitOfWork(() =>
          {
              var result = _workScope.GetAll<UnlockTimesheet>();
              Assert.Equal(expectTotalCount, result.Count());

              result.Last().Id.ShouldBe(4);
              result.Last().UserId.ShouldBe(1);
              result.Last().Type.ShouldBe(LockUnlockTimesheetType.ApproveRejectTimesheet);

          });
        }

        [Fact]
        public async Task UnlockPM_Happy_Test_2()
        {
            var id = 3;
            long expectTotalCount = 0L;

            await WithUnitOfWorkAsync(async () =>
            {
                expectTotalCount = _workScope.GetAll<UnlockTimesheet>().Count();
                await _timesheetsSupervisor.UnlockPM(id);
            });

            WithUnitOfWork(() =>
            {
                var result = _workScope.GetAll<UnlockTimesheet>();
                Assert.Equal(expectTotalCount + 2, result.Count());

                result.Last().Id.ShouldBe(6);
                result.Last().UserId.ShouldBe(3);
                result.Last().Type.ShouldBe(LockUnlockTimesheetType.ApproveRejectTimesheet);

            });
        }

        [Fact]
        public async Task LockPM_Happy_Test()
        {
            var id = 1;
            long expectTotalCount = 0L;

            await WithUnitOfWorkAsync(async () =>
            {  
                expectTotalCount = _workScope.GetAll<UnlockTimesheet>().Count();
                await _timesheetsSupervisor.LockPM(id);
            });

            WithUnitOfWork(() =>
            {
                var result = _workScope.GetAll<UnlockTimesheet>();
                Assert.Equal(expectTotalCount - 1, result.Count());
            });
        }
    }
}
