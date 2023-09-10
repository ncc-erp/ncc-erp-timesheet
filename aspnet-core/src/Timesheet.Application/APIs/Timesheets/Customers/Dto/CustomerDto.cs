using Abp.Application.Services.Dto;
using Abp.AutoMapper;
using Ncc.Entities;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;
using Timesheet.Anotations;

namespace Timesheet.Timesheets.Customers.Dto
{
    [AutoMapTo(typeof(Customer))]
    public class CustomerDto: EntityDto<long>
    {
        [Required]
        [ApplySearchAttribute]
        public string Name { get; set; }

        [ApplySearchAttribute]
        public string Address { get; set; }
        [Required]
        [ApplySearchAttribute]
        public string Code { get; set; }
    }
}
