using System;
using System.Collections.Generic;
using System.Text;

namespace Timesheet.APIs.Retros.Dto
{
    public class GetRetroUserInfoDtocs
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Email { get; set; }
        public string ProjectName { get; set; }
        public string PMEmail { get; set; }

    }
}
