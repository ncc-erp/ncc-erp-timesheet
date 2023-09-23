using Ncc.Entities;
using Ncc.IoC;
using System.Linq;
using Xunit;

namespace Timesheet.Application.Tests.API
{
    public class Test1 : TimesheetApplicationTestBase
    {
        private readonly IWorkScope _workScope;
        public Test1()
        {
            _workScope = Resolve<IWorkScope>();
        }
        [Fact]
        public void TestCase1()
        {
            Assert.Equal(1, 1);
            WithUnitOfWork(() =>
            {
                var allCustomer = _workScope.GetAll<Customer>().ToList();
                Assert.True(allCustomer.Count > 0);
            });
        }
    }
}
