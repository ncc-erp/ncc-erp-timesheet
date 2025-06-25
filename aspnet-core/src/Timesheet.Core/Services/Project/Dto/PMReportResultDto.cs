using System;
using System.Collections.Generic;
using System.Text;

namespace Timesheet.Services.Project.Dto
{
    public class PMReportResultDto
    {
        public List<PMReportItemDto> between3To5PM { get; set; }
        public List<PMReportItemDto> after5PMOrMissing { get; set; }
    }

    public class PMReportItemDto
    {
        public int pmId { get; set; }
        public int projectId { get; set; }
        public DateTime? timeSendReport { get; set; }
        public string userName { get; set; }
        public string emailAddress { get; set; }
        public int money { get; set; }

    }


}
