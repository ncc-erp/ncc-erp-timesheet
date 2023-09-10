using System;
using System.Collections.Generic;
using System.Text;

namespace Timesheet.DomainServices.Dto
{
    public class InputGenerateDataTeamBuildingDto
    {
        public int Year { get; set; }
        public int Month { get; set; }
        public int? Day { get; set; }
    }
}
