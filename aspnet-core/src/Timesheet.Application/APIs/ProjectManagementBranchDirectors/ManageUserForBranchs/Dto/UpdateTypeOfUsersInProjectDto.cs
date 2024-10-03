using Ncc.Entities;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace Timesheet.APIs.ProjectManagementBranchDirectors.ManageUserForBranchs.Dto
{
    public class UpdateTypeOfUsersInProjectDto
    {
        [Required]
        public IEnumerable<UserTypeDto> UserTypes { get; set; }
        [Required]
        public long ProjectId { get; set; }
    }
    public class UserTypeDto
    {
        public long ProjectUserId { get; set; }
        public ProjectUserType UserType { get; set; }
    }
}
