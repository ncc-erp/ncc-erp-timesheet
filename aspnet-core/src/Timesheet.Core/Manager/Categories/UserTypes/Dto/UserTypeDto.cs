using System;
using System.Collections.Generic;
using System.Text;

namespace Timesheet.Manager.Categories.UserTypes.Dto
{
    public class UserTypeDto
    {
        public string Name { get; set; }
        public long Id { get; set; }
        public string Color { get; set; }

        public string Code { get; set; }
        public string ShortName { get; set; }
        public long ContractPeriodMonth { get; set; }
    }
}
