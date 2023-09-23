using Ncc.IoC;
using Ncc.Timesheets.Customers;
using Shouldly;
using System.Threading.Tasks;
using Timesheet.Paging;
using Xunit;
using NSubstitute;
using Timesheet.Timesheets.Customers.Dto;
using Abp.UI;
using Abp.Application.Services.Dto;
using System.Linq.Dynamic.Core;
using System.Linq;

namespace Timesheet.Application.Tests.API.Timesheets.Customers
{
    public class CustomerAppService_Tests : TimesheetApplicationTestBase
    {
        //Summary: 4/4 funtions, 9/9 test cases passed
        private readonly IWorkScope _workScope;
        private readonly CustomerAppService _customerAppService;

        public CustomerAppService_Tests()
        {
            _workScope = Resolve<IWorkScope>();

            _customerAppService = Substitute.For<CustomerAppService>(_workScope);

            _customerAppService.ObjectMapper = Resolve<Abp.ObjectMapping.IObjectMapper>();

        }

        [Fact]
        public async Task Should_Get_All_Paging()
        {
            await WithUnitOfWorkAsync(async () =>
            {
                var input = new GridParam
                {
                    SkipCount = 1,
                    MaxResultCount = 3,
                };

                var result = await _customerAppService.GetAllPagging(input);

                result.TotalCount.ShouldBeGreaterThanOrEqualTo(5);
                result.Items.Count.ShouldBe(3);
                result.Items.ShouldContain(item => item.Id == 4);
                result.Items.ShouldContain(item => item.Id == 2);
                result.Items.ShouldContain(item => item.Id == 3);
                result.Items.ShouldContain(item => item.Code == "Client 4");
                result.Items.ShouldContain(item => item.Code == "Client 3");
                result.Items.ShouldContain(item => item.Code == "Client 2");
                result.Items.ShouldNotContain(item => item.Id == 5);
                result.Items.ShouldNotContain(item => item.Id == 1);
            });
        }

        [Fact]
        public async Task Should_Get_All_Paging_By_Search_Text()
        {
            await WithUnitOfWorkAsync(async () =>
            {
                var input = new GridParam
                {
                    SearchText = "Client 4"
                };

                var result = await _customerAppService.GetAllPagging(input);

                result.TotalCount.ShouldBe(1);
                result.Items.Count.ShouldBe(1);
                result.Items.ShouldContain(item => item.Id == 4);
                result.Items.ShouldContain(item => item.Name == "Client 4");
                result.Items.ShouldContain(item => item.Code == "Client 4");
                result.Items.ShouldNotContain(item => item.Id == 1);
                result.Items.ShouldNotContain(item => item.Id == 2);
                result.Items.ShouldNotContain(item => item.Id == 3);
                result.Items.ShouldNotContain(item => item.Id == 5);
            });
        }

        [Fact]
        public async Task Should_Get_All_Customer()
        {
            await WithUnitOfWorkAsync(async () =>
            {
                var result = await _customerAppService.GetAll();

                result.ShouldNotBeNull();
                result.Count.ShouldBeGreaterThanOrEqualTo(5);
                result.ShouldContain(item => item.Id == 1);
                result.ShouldContain(item => item.Id == 2);
                result.ShouldContain(item => item.Id == 3);
                result.ShouldContain(item => item.Id == 4);
                result.ShouldContain(item => item.Id == 5);
            });
        }

        [Fact]
        public async Task Should_Insert_A_Valid_Customer()
        {
            var expectCustomer = new Ncc.Entities.Customer
            {
                Id = -1,
                Name = "Esg",
                Address = "New york",
                Code = "Esg"
            };

            await WithUnitOfWorkAsync(async () =>
            {
                var result = await _customerAppService.Save(new CustomerDto
                {
                    Id = -1,
                    Name = "Esg",
                    Address = "New york",
                    Code = "Esg"
                });

                result.ShouldNotBeNull();
                result.Id.ShouldBe(expectCustomer.Id);
                result.Name.ShouldBe(expectCustomer.Name);
                result.Address.ShouldBe(expectCustomer.Address);
                result.Code.ShouldBe(expectCustomer.Code);
            });

            await WithUnitOfWorkAsync(async () =>
            {
                var allCustomer = _workScope.GetAll<Ncc.Entities.Customer>();
                var customer = await _workScope.GetAsync<Ncc.Entities.Customer>(expectCustomer.Id);

                allCustomer.Count().ShouldBe(6);
                allCustomer.ToList().Find(item => item.Name == expectCustomer.Name).ShouldNotBeNull();
                customer.Id.ShouldBe(expectCustomer.Id);
                customer.Name.ShouldBe(expectCustomer.Name);
                customer.Address.ShouldBe(expectCustomer.Address);
                customer.Code.ShouldBe(expectCustomer.Code);
            });
        }

        [Fact]
        public async Task Should_Update_A_Valid_Customer()
        {
            var expectCustomer = new Ncc.Entities.Customer
            {
                Id = 2,
                Name = "NCC update",
                Address = "Address update",
                Code = "NCC update"
            };

            await WithUnitOfWorkAsync(async () =>
            {
                var result = await _customerAppService.Save(new CustomerDto
                {
                    Id = 2,
                    Name = "NCC update",
                    Address = "Address update",
                    Code = "NCC update"
                });

                result.ShouldNotBeNull();
                result.Id.ShouldBe(expectCustomer.Id);
                result.Name.ShouldBe(expectCustomer.Name);
                result.Address.ShouldBe(expectCustomer.Address);
                result.Code.ShouldBe(expectCustomer.Code);
            });

            await WithUnitOfWorkAsync(async () =>
            {
                var customer = await _workScope.GetAsync<Ncc.Entities.Customer>(2);

                customer.Id.ShouldBe(expectCustomer.Id);
                customer.Name.ShouldBe(expectCustomer.Name);
                customer.Address.ShouldBe(expectCustomer.Address);
                customer.Code.ShouldBe(expectCustomer.Code);
            });
        }

        [Fact]
        public async Task Should_Thorw_Exception_When_Save_A_Customer_With_Duplicate_Name()
        {
            await WithUnitOfWorkAsync(async () =>
            {
                var exception = await Assert.ThrowsAsync<UserFriendlyException>(async () =>
                {
                    await _customerAppService.Save(new CustomerDto
                    {
                        Name = "Client 1",
                        Address = "New york",
                        Code = "Client 6"
                    });
                });

                exception.Message.ShouldBe("Customer name Client 1 already exist");
            });
        }

        [Fact]
        public async Task Should_Thorw_Exception_When_Save_A_Customer_With_Duplicate_Code()
        {
            await WithUnitOfWorkAsync(async () =>
            {
                var exception = await Assert.ThrowsAsync<UserFriendlyException>(async () =>
                {
                    await _customerAppService.Save(new CustomerDto
                    {
                        Name = "Client 8",
                        Address = "New york",
                        Code = "Client 1"
                    });
                });

                exception.Message.ShouldBe("Customer code Client 1 already exist");
            });
        }

        [Fact]
        public async Task Should_Delete_A_Valid_Customer()
        {
            await WithUnitOfWorkAsync(async () =>
            {
                await _customerAppService.Delete(new EntityDto<long>
                {
                    Id = 5,
                });
               
            });

            await WithUnitOfWorkAsync(async () =>
            {
                var allCustomer = _workScope.GetAll<Ncc.Entities.Customer>();

                allCustomer.Count().ShouldBe(4);
                allCustomer.ShouldNotContain(item => item.Id == 5);
            });
        }
        [Fact]
        public async Task Should_Throw_Exception_When_Delete_A_Customer_Having_Project()
        {
            await WithUnitOfWorkAsync(async () =>
            {
                var exception = await Assert.ThrowsAsync<UserFriendlyException>(async () =>
                {
                    await _customerAppService.Delete(new EntityDto<long>
                    {
                        Id = 1,
                    });
                });

                exception.Message.ShouldBe("Client Id 1 has project");
            });

            await WithUnitOfWorkAsync(async () =>
            {
                var allCustomer = _workScope.GetAll<Ncc.Entities.Customer>();
                
                allCustomer.Count().ShouldBe(5);
                allCustomer.ShouldContain(item => item.Id == 1);
            });
        }

    };
}
