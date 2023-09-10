using Abp.Application.Services.Dto;
using Abp.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Ncc;
using Ncc.Entities;
using Ncc.Entities.Enum;
using Ncc.IoC;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Timesheet.APIs.TeamBuildingProject.Dto;
using Timesheet.Extension;
using Timesheet.Paging;
using Timesheet.Services.Project;

namespace Timesheet.APIs.TeamBuildingProject
{
    public class TeamBuildingProjectAppService : AppServiceBase
    {
        public static ProjectService _projectService;
        public TeamBuildingProjectAppService(ProjectService projectService, IWorkScope workScope) : base(workScope)
        {
            _projectService = projectService;
        }
        
        [HttpPost]
        [AbpAuthorize(Ncc.Authorization.PermissionNames.TeamBuilding_Project)]
        public async Task<GridResult<TeamBuildingProjectDto>> GetAllPagging(GridParam input)
        {
            var listPMFromProject = _projectService.GetProjectPMName();
            var query = WorkScope.GetAll<Project>()
                .Where(s => s.Status == StatusEnum.ProjectStatus.Active)
                .Select(s => new TeamBuildingProjectDto
                {
                    Id = s.Id,
                    Name = s.Name,
                    ProjectType = s.ProjectType,
                    PMEmail = listPMFromProject.Where(x => x.ProjectCode == s.Code).FirstOrDefault().PMEmail,
                    IsAllowTeamBuilding = s.IsAllowTeamBuilding
                });

            return await query.GetGridResult(query, input);
        }

        [HttpPost]
        [AbpAuthorize(Ncc.Authorization.PermissionNames.TeamBuilding_Project_SelectProjectTeamBuilding)]
        public async System.Threading.Tasks.Task SelectIsAllowTeamBuilding(List<SelectProjectIsAllowTeamBuildingDto> project)
        {
            var listProject = WorkScope.GetAll<Project>()
                .Where(s => project.Select(x => x.ProjectId).Contains(s.Id))
                .ToList();

            var DicProject =  project.ToDictionary(s => s.ProjectId, s => s.IsAllowTeamBuilding);

            foreach (var item in listProject)
            {
                if (DicProject.ContainsKey(item.Id))
                {
                    item.IsAllowTeamBuilding = DicProject[item.Id];
                }

            }
            await WorkScope.UpdateRangeAsync(listProject);
        }

        [HttpGet]
        public async Task<List<TeamBuildingProjectDto>> GetAllProjectTeamBuilding()
        {
            var listPMFromProject = _projectService.GetProjectPMName();
            var query = await WorkScope.GetAll<Project>()
                .Where(s => s.Status == StatusEnum.ProjectStatus.Active)
                .Select(s => new TeamBuildingProjectDto
                {
                    Id = s.Id,
                    Name = s.Name,
                    PMEmail = listPMFromProject.Where(x => x.ProjectCode == s.Code).FirstOrDefault().PMEmail,
                    ProjectType = s.ProjectType,
                    IsAllowTeamBuilding = s.IsAllowTeamBuilding
                }).ToListAsync();

            return query.ToList();
        }
    }
}
