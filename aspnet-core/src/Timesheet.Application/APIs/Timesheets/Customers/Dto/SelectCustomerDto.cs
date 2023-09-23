using Abp.Application.Services.Dto;
using Abp.AutoMapper;
using Ncc.Entities;

namespace Timesheet.Timesheets.Customers.Dto
{
    [AutoMapTo(typeof(Customer))]
    public class SelectCustomerDto : EntityDto<long>
    {
        public string Name { get; set; }
        public string Code { get; set; }
    }
}
