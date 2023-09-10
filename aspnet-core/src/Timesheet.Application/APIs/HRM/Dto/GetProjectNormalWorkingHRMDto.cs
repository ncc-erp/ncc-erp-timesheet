using System;
using System.Collections.Generic;
using System.Text;

namespace Timesheet.APIs.HRM.Dto
{
    public class GetProjectNormalWorkingHRMDto
    {
        public long UserId { get; set; }
        public IEnumerable<ProjectNormalWorkingDetailDto> ProjectNormalWorking{ get; set; }
    }
    public class ProjectNormalWorkingDetailDto
    {
        public long ProjectId { get; set; }
        public string ProjectName { get; set; }
        public double TotalWorkingHour { get; set; }
    }


}
