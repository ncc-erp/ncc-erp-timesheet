using System;
using System.Collections.Generic;
using System.Text;
using Timesheet.Entities;
using static Ncc.Entities.Enum.StatusEnum;

namespace Timesheet.APIs.ProjectManagementBranchDirectors.ManageUserProjectForBranchs.Dto
{
    public class GetAllValueOfUserInProjectByUserIdDto
    {
        public long ProjectId { get; set; }
        public string ProjectName { get; set; }
        public string ProjectCode { get; set; }
        public ValueOfUserType ValueOfUserType { get; set; }
        public float ShadowPercentage { get; set; }
        public float WorkingHours { get; set; }
    }

    public class SumWorkingTimeByProjectDto
    {
        public long ProjectId { get; set; }
        public DateTime DateAt { get; set; }
        public TypeOfWork TypeOfWork { get; set; }
        public TimesheetStatus Status { get; set; }
        public int WorkingTime { get; set; }
    }
}
