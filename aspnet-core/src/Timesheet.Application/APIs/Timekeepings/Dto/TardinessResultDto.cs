using System;
using System.Collections.Generic;
using System.Text;
using Timesheet.Paging;

namespace Timesheet.APIs.Timekeepings.Dto
{
    public class TardinessResultDto
    {
        public GridResult<GetTardinessUserDto> GridResult { get; set; }
        public int TotalPunishmentAmount { get; set; }
    }
}
