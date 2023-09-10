using Abp.Dependency;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Timesheet.Services.Project.Dto;

namespace Timesheet.Services.Project
{
    public interface IProjectService
    {
        Task<List<UpdateStarRateToProjectDto>> UpdateStarRateToProject(List<UpdateStarRateToProjectDto> input);
    }
}
