using Abp.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Text;
using Timesheet.Anotations;

namespace Timesheet.APIs.ProjectManagementBranchDirectors.ManageUserForBranchs.Dto
{
    public class UserStatisticInProjectDto
    {
        [ApplySearch]
        public long ProjectId { get; set; }
        [ApplySearch]
        public string ProjectCode { get; set; }
        [ApplySearch]
        public string ProjectName { get; set; }
        public int TotalUser { get; set; }
        public int MemberCount { get; set; }
        public int ExposeCount { get; set; }
        public int ShadowCount { get; set; }
    }
}
