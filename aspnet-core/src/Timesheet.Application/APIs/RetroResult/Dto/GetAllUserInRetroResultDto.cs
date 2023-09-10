using System;
using System.Collections.Generic;
using System.Text;

namespace Timesheet.APIs.RetroDetails.Dto
{
    public class GetAllUserInRetroResultDto
    {
        public long UserId { get; set; }
        public string FullNameAndEmail { get; set; }
    }
}
